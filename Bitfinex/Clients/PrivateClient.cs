
using Bitfinex.Net.Clients;
using Bitfinex.Net.Objects;
using Bitfinex.Net.Objects.Models;
//using Binance.RestApi;
using Common;
using Common.Messages;
using Common.Models;
using ConnectorCommon;
using ConnectorStatus;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using DataStore;
using MessageBroker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Bitfinex.Config;
using Bitfinex.Extensions;
using Bitfinex.Net.Enums;
using Bitfinex.RestApi;
using Constants = Common.Constants;
using Bitfinex.Net.Objects.Models.Socket;
using Common.Models.DTOs;
using Common.Models.Entities;

namespace Bitfinex.Clients
{
   public class PrivateClient : IPrivateClient
   {
      private readonly IBitfinexRestApiClient _restApiClient;
      private readonly ILogger<PrivateClient> _logger;
      private readonly IMessageBroker _messageBroker;
      private readonly IPortfolioRepository _portfolioRepository;
      private readonly IConnectorClientStatus _clientStatus;

      private readonly List<string> _genericSymbols = new List<string>();
      private List<string> _exchangeSymbols = new List<string>();
      private Dictionary<string, string> _exchangeToGenericLookup = new Dictionary<string, string>();
      private Dictionary<string, string> _genericToExchangeLookup = new Dictionary<string, string>();
      private Dictionary<string, ExchangeCoinMappingsDTO> _coinMappingsTable = new();

      private readonly string _websocketEndpoint;
      private string _apiKey { get; set; }
      private string _secret { get; set; }
      private string _accountName { get; set; }
      private string _instanceName { get; set; }
      private readonly UInt64 _reconnectInterval;
      private readonly UInt64 _orderbookUpdateInterval;

      private string _orderTopic { get; set; }
      private string _tradeTopic { get; set; }
      private string _balanceTopic { get; set; }
      private string _statusTopic { get; set; }
      private bool _loggedInState = false;
      private PrivateConnectionLogon _accountConfig { get; set; }

      public PrivateClient(ILoggerFactory loggerFactory,
         IBitfinexRestApiClient restApiClient,
         IOptions<PrivateConnectionConfig> config,
         IOptions<SymbolDataConfig> symbolData,
         IMessageBroker messageBroker,
         IConnectorClientStatus clientStatus,
         IPortfolioRepository repository)
      {
         _logger = loggerFactory.CreateLogger<PrivateClient>();
         _restApiClient = restApiClient;
         _messageBroker = messageBroker;
         _portfolioRepository = repository;
         _clientStatus = clientStatus;

         _websocketEndpoint = config.Value.PrivateWSEndpoint;
       
         _reconnectInterval = config.Value.ReconnectIntervalMilliSecs;
         _orderbookUpdateInterval = config.Value.OrderbookUpdateInterval;
         _genericSymbols = symbolData.Value.CoinPairs.ToList();
      }

      public async Task Init(PrivateConnectionLogon accountsConfig)
      {
         if (_loggedInState)
         {
            _logger.LogInformation("Sending a Already Logged in to Bitfinex Spot message to strategy");
            SendLoginStatusMsg(true, "Already Logged in to Bitfinex Spot");
            return;
         }
         _accountConfig = accountsConfig;
         _accountName = accountsConfig.SPName;
         _instanceName = accountsConfig.ConfigInstance;
         _apiKey = accountsConfig.ApiKey;
         _secret = accountsConfig.Secret;
       

         _orderTopic = _accountName + "." + _instanceName + Common.Constants.ORDERS_TOPIC;
         _tradeTopic = _accountName + "." + _instanceName + Common.Constants.TRADES_TOPIC;
         _balanceTopic = _accountName + "." + _instanceName + Common.Constants.BALANCES_TOPIC;
         _statusTopic = _accountName + "." + _instanceName + Common.Constants.STATUS_TOPIC;

         var coinLookups = await _portfolioRepository.GetExchangeCoinLookups();
         _coinMappingsTable = coinLookups.ToDictionary(c => c.ExchangeCoinName, c => c);
         await _restApiClient.Init(accountsConfig, _statusTopic);

         
         foreach (var symbol in _genericSymbols)
         {
            // Todo - sort this out - exchange should know enough to get from Venues and make 
            // VenueId as foreign key
            var exchangeSymbol = await _portfolioRepository.GetExchangeCoinPairSymbolFromGenericCoinPairSymbol(Constants.BITFINEX, symbol);
            if (exchangeSymbol == null)
            {
               _logger.LogError("No exchange symbol for {Symbol}", symbol);
               throw new Exception($"No exchange symbol for {symbol}");
            }
            _exchangeSymbols.Add(exchangeSymbol.ExchangeCoinpairName);
            _genericToExchangeLookup.Add(symbol, exchangeSymbol.ExchangeCoinpairName);
            _exchangeToGenericLookup.Add(exchangeSymbol.ExchangeCoinpairName, symbol);
            var trades = await _restApiClient.GetTradeHistory(exchangeSymbol.ExchangeCoinpairName, 5);
            OnTradeUpdate(exchangeSymbol.ExchangeCoinpairName, trades);
         }
         await SubscribeToWebsocket();
      }

      private void OnTradeUpdate(string symbol, IEnumerable<BitfinexTradeSimple> tradeData)
      {
         if (tradeData == null)
         {
            
            _clientStatus.UpdatePrivateRestApiStatus(_statusTopic, _accountName, _instanceName, false, "Unable to get Trade History");
            return;
         }
         _clientStatus.UpdatePrivateRestApiStatus(_statusTopic, _accountName, _instanceName, true, "");
         var trades = tradeData.ToList();
         var lastBitfinexTrade = new BitfinexTradeSimple();
         if (trades.Any())
         {
            lastBitfinexTrade = trades[0];
         }

         if (lastBitfinexTrade.Quantity < 0)
         {
            lastBitfinexTrade.Quantity = lastBitfinexTrade.Quantity * -1;
         }

         //LAST_TRADED_PRICE_TOPIC
         var lastTrade = new LatestTrade
         {
            Symbol = _exchangeToGenericLookup[symbol],
            Price = lastBitfinexTrade.Price,
            Quantity = lastBitfinexTrade.Quantity,
            TradeTime = lastBitfinexTrade.Timestamp,
            
         };

         var response = new MessageBusReponse()
         {
            ResponseType = ResponseTypeEnums.LAST_TRADED_PRICE,
            FromVenue = Common.Constants.BITFINEX,
            AccountName = "",
            Data = JsonSerializer.Serialize(lastTrade)
         };
         var routingKey = _exchangeToGenericLookup[symbol] + "." + Constants.BITFINEX + Common.Constants.LAST_TRADED_PRICE_TOPIC;
         PublishHelper.PublishToTopic(routingKey, response, _messageBroker);
      }

      private BitfinexSocketClient _socketClient { get; set; }

      public async Task CancelAllOrdersCommand(string genericSymbol)
      {
         // Not going to use this
         try
         {
            string symbol = null;
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.CANCEL_ORDERS_RESPONSE,
               FromVenue = Common.Constants.BITFINEX,
               OriginatingSource = _instanceName
            };

            if (genericSymbol != null)
               symbol = _genericToExchangeLookup[genericSymbol];


            var result = await _restApiClient.CancelAllSpotOrders();
            if (result.Success)
            {
               var cancelOrderList = result.Value;
               response.Data = JsonSerializer.Serialize(cancelOrderList);
            }
            else
            {
               response.Success = false;
               response.Data = result.Error;
            }
            PublishHelper.PublishToTopic(_orderTopic, response, _messageBroker);
         }
         catch (Exception e)
         {
            _logger.LogError("Error cancelling Order {Error}", e.Message);
            throw;
         }
      }

      public async Task CancelOrder(OrderIdHolder orderId)
      {
         try
         {
            _logger.LogInformation("Cancelling ClientOid {ClientOid} and OrderId {OrderId}", orderId.ClientOrderId, orderId.OrderId);
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.CANCEL_ORDERS_RESPONSE,
               FromVenue = Common.Constants.BITFINEX,
               OriginatingSource = _instanceName
            };
            orderId.Symbol = _genericToExchangeLookup[orderId.Symbol];
           // var result = await _restApiClient.CancelSpotOrder(orderId);
            var result = await _socketClient.SpotStreams.CancelOrderAsync(Int64.Parse(orderId.OrderId));
            if (result.Success)
            {
               _logger.LogInformation("Successful Cancel for ClientOid {ClientOid} and OrderId {OrderId}", orderId.ClientOrderId, orderId.OrderId);
               var cancelOrderData = result.Data;
              
               var cancelOrderReply = new SingleCancelledOrderId()
               {
                  OrderId = cancelOrderData.Id.ToString(),
                  Account = _accountName,
                  Instance = _instanceName,
                  ClientOrderId = cancelOrderData.ClientOrderId.ToString()
               };
               response.Data = JsonSerializer.Serialize(cancelOrderReply);
            }
            else
            {
               response.ResponseType = ResponseTypeEnums.CANCEL_ORDER_ERROR;
               response.Success = false;
               var data = new CancelOrderResponseError()
               {
                  OrderId = orderId.OrderId,
                  ClientOid = orderId.ClientOrderId,
                  Error = result.Error.Message,
                  Account = _accountName,
                  Instance = _instanceName
               };
               response.Data = JsonSerializer.Serialize(data);
            }
            PublishHelper.PublishToTopic(_orderTopic, response, _messageBroker);
         }
         catch (Exception e)
         {
            _logger.LogError("Error cancelling Order {Error}", e.Message);
            throw;
         }
      }

      public async Task DisconnectPrivate()
      {
         try
         {
            // TODO - check that this actually does a disconnect
            await _socketClient.UnsubscribeAllAsync();
         }
         catch (Exception e)
         {
            // TODO - should we do  retry??
            _logger.LogError(e, "Error unsubscribing from  from all streams {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public async Task GetBalances(int jobNo)
      {
         try
         {
            _logger.LogInformation("GetBalance Request ");

            var response = new MessageBusReponse()
            {
               FromVenue = Common.Constants.BITFINEX,
               OriginatingSource = _instanceName
            };

            var result = await _restApiClient.GetBalances();
            if (result.Success)
            {
               var balances = result.Value;
               balances.ForEach(b =>
               {
                  if (_coinMappingsTable.ContainsKey(b.Currency))
                  {
                     b.Currency = _coinMappingsTable[b.Currency].Coin.Name;
                  }
                  b.Instance = _instanceName;
                  b.Account = _accountName;
               });
               _logger.LogInformation("GetBalance Successful Response ");
               response.ResponseType = ResponseTypeEnums.GET_BALANCE_RESPONSE;
               response.Data = JsonSerializer.Serialize(balances);
               response.JobNo = jobNo;
            }
            else
            {
               response.Success = false;
               var data = new PlaceOrderErrorResponse()
               {
                  ErrorMessage = result.Error,
                  Account = _accountName,
                  Instance = _instanceName
               };
               response.Data = JsonSerializer.Serialize(data);
               _logger.LogError("GetBalance Unsuccessful Response {Error} ", result.Error);
               response.ResponseType = ResponseTypeEnums.GET_BALANCE_RESPONSE_ERROR;
               response.JobNo = jobNo;
            }
            PublishHelper.PublishToTopic(_balanceTopic, response, _messageBroker);
         }
         catch (Exception e)
         {
            _logger.LogError("Error getting balances {Error}", e.Message);
            throw;
         }
      }

      public async Task GetOpenOrders(string[] genericSymbols)
      {
         try
         {
            string exchangeSymbol = null;
            foreach (var symbol in genericSymbols)
            {
               if (!String.IsNullOrEmpty(symbol))
               {
                  exchangeSymbol = _genericToExchangeLookup[symbol];
               }

               var response = new MessageBusReponse()
               {
                  ResponseType = ResponseTypeEnums.GET_OPEN_ORDERS_RESPONSE,
                  FromVenue = Common.Constants.BITFINEX,
                  OriginatingSource = _instanceName
               };

               //var binanceOrder = cmd.CloneForLimit();

               var res = await _restApiClient.GetOpenSpotOrders(exchangeSymbol);
               if (res.Success)
               {
                  var data = res.Value.ToList();
                  var orderList = new List<OrderQueryResponse>();
                  foreach (var order in data)
                  {
                     var oo = order.Clone();
                     oo.Symbol = _exchangeToGenericLookup[order.Symbol];
                     oo.Account = _accountName;
                     oo.Instance = _instanceName;
                     orderList.Add(oo);
                  }

                  _logger.LogInformation("Sending back {OrderCount} Open Orders for {Instance}", orderList.Count, _instanceName);
                  response.Data = JsonSerializer.Serialize(orderList);
               }
               else
               {
                  response.ResponseType = ResponseTypeEnums.OPEN_ORDERS_ERROR;
                  response.Success = false;
                  var data = new PlaceOrderErrorResponse()
                  {
                     ErrorMessage = res.Error,
                     Account = _accountName,
                     Instance = _instanceName
                  };
                  response.Data = JsonSerializer.Serialize(data);
               }
               PublishHelper.PublishToTopic(_orderTopic, response, _messageBroker);
            }
         }
         catch (Exception e)
         {
            _logger.LogError("Error getting open Orders {Error}", e.Message);
            throw;
         }
      }

      public bool LoggedInState()
      {
         return _loggedInState;
      }

      public async Task PlaceMarketOrder(PlaceOrderCmd cmd)
      {
         try
         {
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.PLACE_ORDER_RESPONSE,
               FromVenue = Common.Constants.BITFINEX,
               OriginatingSource = _instanceName
            };
            cmd.Price = Math.Round(cmd.Price, 2, MidpointRounding.ToEven);

            var bitfinexOrder = cmd.CloneForMarket();
            bitfinexOrder.Symbol = _genericToExchangeLookup[bitfinexOrder.Symbol];

            //var res = await _restApiClient.PlaceSpotLimitOrder(bitfinexOrder);

            var res = await _socketClient.SpotStreams.PlaceOrderAsync(OrderType.ExchangeMarket, bitfinexOrder.Symbol,
               bitfinexOrder.Quantity, null, Int32.Parse(cmd.ClientOrderId), null);

            if (res.Success)
            {
               var order = res.Data;
               var orderIdHolder = new OrderIdHolder()
               {
                  Account = _accountName,
                  Instance = _instanceName,
                  ClientOrderId = order.ClientOrderId.ToString(),
                  OrderId = order.Id.ToString(),
                  Symbol = _exchangeToGenericLookup[bitfinexOrder.Symbol]
               };
               response.Success = true;
               response.Data = JsonSerializer.Serialize(orderIdHolder);
            }
            else
            {
               response.ResponseType = ResponseTypeEnums.PLACE_ORDER_ERROR;
               response.Success = false;
               var data = new PlaceOrderErrorResponse()
               {
                  ClientOrderId = cmd.ClientOrderId,
                  ErrorMessage = res.Error.Message,
                  Account = _accountName,
                  Instance = _instanceName
               };
               response.Data = JsonSerializer.Serialize(data);
            }
            PublishHelper.PublishToTopic(_orderTopic, response, _messageBroker);
         }
         catch (Exception e)
         {
            _logger.LogError("Error placing Order {Error}", e.Message);
            throw;
         }
      }

      public async Task PlaceOrder(PlaceOrderCmd cmd)
      {
         try
         {
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.PLACE_ORDER_RESPONSE,
               FromVenue = Common.Constants.BITFINEX,
               OriginatingSource = _instanceName
            };
            cmd.Price = Math.Round(cmd.Price, 2, MidpointRounding.ToEven);

            var bitfinexOrder = cmd.CloneForLimit();
            bitfinexOrder.Symbol = _genericToExchangeLookup[bitfinexOrder.Symbol];
            if (!cmd.IsBuy)
            {
               bitfinexOrder.Quantity *= -1;
            }

            var res1 = await _socketClient.SpotStreams.PlaceOrderAsync(OrderType.ExchangeLimit, bitfinexOrder.Symbol,
               bitfinexOrder.Quantity, null, Int32.Parse(cmd.ClientOrderId), bitfinexOrder.Price);

               _logger.LogInformation("Limit Order Placed {Price} {Quantity}", bitfinexOrder.Price, bitfinexOrder.Quantity );
            if (res1.Success)
            {
               var order = res1.Data;
               var orderIdHolder = new OrderIdHolder()
               {
                  Account = _accountName,
                  Instance = _instanceName,
                  ClientOrderId = order.ClientOrderId.ToString(),
                  OrderId = order.Id.ToString(),
                  Symbol = _exchangeToGenericLookup[bitfinexOrder.Symbol]
               };
               response.Success = true;
               response.Data = JsonSerializer.Serialize(orderIdHolder);
            }
            else
            {
               
               response.ResponseType = ResponseTypeEnums.PLACE_ORDER_ERROR;
               response.Success = false;
               var data = new PlaceOrderErrorResponse()
               {
                  ClientOrderId = cmd.ClientOrderId,
                  ErrorMessage = res1.Error.Message,
                  Account = _accountName,
                  Instance = _instanceName
               };
               response.Data = JsonSerializer.Serialize(data);
            }

            PublishHelper.PublishToTopic(_orderTopic, response, _messageBroker);
         }
         catch (Exception e)
         {
            _logger.LogError("Error placing Order {Error}", e.Message);
            throw;
         }
      }

      private void SendLoginStatusMsg(bool status, string message)
      {
         _loggedInState = status;
         var loginStatus = new PrivateClientLoginStatus()
         {
            IsLoggedIn = true,
            AccountName = _accountName,
            InstanceName = _instanceName,
            Message = message
         };

         var response = new MessageBusReponse()
         {
            OriginatingSource = _instanceName,
            ResponseType = ResponseTypeEnums.PRIVATE_CLIENT_LOGIN_STATUS,
            FromVenue = Common.Constants.BITFINEX,
            Data = JsonSerializer.Serialize(loginStatus)
         };
         PublishHelper.PublishToTopic(_statusTopic, response, _messageBroker);
      }

      private async Task SubscribeToWebsocket()
      {
         try
         {
            _socketClient = new BitfinexSocketClient(new BitfinexSocketClientOptions()
            {
              
               ApiCredentials = new ApiCredentials(_apiKey, _secret),
               LogLevel = LogLevel.Debug,


               SpotStreamsOptions = new SocketApiClientOptions()
               {
                  BaseAddress = _websocketEndpoint,
                  AutoReconnect = true,
                  ReconnectInterval = TimeSpan.FromMilliseconds(_reconnectInterval)
               },
               
            });
            

            if (_socketClient != null)
            {
               var subWallet = await _socketClient.SpotStreams.SubscribeToBalanceUpdatesAsync(OnWalletUpdate);
               if (subWallet.Success)
               {
                  subWallet.Data.ConnectionClosed += ConnectionClosed;
                  subWallet.Data.ConnectionLost += ConnectionLost;
                  subWallet.Data.ConnectionRestored += ConnectionRestored;
               }
               else
               {

                  SendLoginStatusMsg(false, $"Failed to log into Bitfinex- error {subWallet.Error}");
                  _logger.LogError("Failed to log into Bitfinex - error {Error}", subWallet.Error);
                  return;
               }

               var subOwnOrders = await _socketClient.SpotStreams.SubscribeToUserTradeUpdatesAsync(OnOwnOrderUpdate, OnOwnTradeUpdate, OnPositionUpdate);
               if (subOwnOrders.Success)
               {
                  _logger.LogInformation("Successfully Logged into Bitfinex");
                  SendLoginStatusMsg(true, "Successfully Logged into Bitfinex");
               }
               else
               {
                  SendLoginStatusMsg(false, $"Failed to log into Bitfinex- error {subWallet.Error}");
                  _logger.LogError("Failed to log into Bitfinex - error {Error}", subWallet.Error);
                  return;
               }
               _clientStatus.UpdatePrivateWebSocketStatus(_statusTopic, _accountName, _instanceName, true, "All Good With Private Websocket for Bitfinex");
            }
         }
         catch (Exception e)
         {
            _clientStatus.UpdatePrivateWebSocketStatus(_statusTopic, _accountName, _instanceName, false, "Error With Private Websocket for Bitfinex");
            _logger.LogError(e, "Error connecting to Bitfinex Websocket {Error}", e.Message);
            throw;
         }
      }

      private void OnPositionUpdate(DataEvent<BitfinexSocketEvent<IEnumerable<BitfinexPosition>>> obj)
      {
         _logger.LogInformation("OnPositionUpdate");
      }

      private void OnOwnTradeUpdate(DataEvent<BitfinexSocketEvent<IEnumerable<BitfinexTradeDetails>>> update)
      {
         _logger.LogInformation("OnOwnTradeUpdate");
         var tradeList = update.Data.Data;
         foreach (var t in tradeList)
         {
            var trade = t.CreateTradeMsg();
            trade.Account = _accountName;
            trade.Instance = _instanceName;
            trade.Symbol = _exchangeToGenericLookup[trade.Symbol];
            trade.Venue = Constants.BITFINEX;
           
            var data = JsonSerializer.Serialize(trade);
            var rabbitMsg = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.TRADE_UPDATE,
               FromVenue = Common.Constants.BITFINEX,
               OriginatingSource = _instanceName,
               Data = data
            };
            _logger.LogInformation("Sending Trade message on Subject {Topic} with Trade Id {TradeId}  Order {OrderId} price {Price} quantity {Quantity} ",
               _tradeTopic, trade.TradeId,  trade.OrderId, trade.Price, trade.Quantity );
            PublishHelper.PublishToTopic(_tradeTopic, rabbitMsg, _messageBroker);
         }
      }
     

      private void OnOwnOrderUpdate(DataEvent<BitfinexSocketEvent<IEnumerable<BitfinexOrder>>> update)
      {
         _logger.LogInformation("OnOwnOrderUpdate");
         var orderList = update.Data.Data.ToList();
         foreach (var o in orderList)
         {
            var order = o.CloneForOwnOrder();
            if (_exchangeToGenericLookup.ContainsKey(order.Symbol))
               order.Symbol = _exchangeToGenericLookup[order.Symbol];
            order.Account = _accountName;
            order.Instance = _instanceName;
            order.Venue = Constants.BITFINEX;
            var orderData = JsonSerializer.Serialize(order);
            var orderRabbitMsg = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.OWN_ORDER_UPDATE,
               FromVenue = Common.Constants.BITFINEX,
               OriginatingSource = _instanceName,
               Data = orderData
            };
            _logger.LogInformation(
               "Sending OwnOrder message on Subject {Topic} Customer OrderId {CustomerOid} Order {OrderId} price {Price} quantity {Quantity} isbuy {IsBuy}",
               _orderTopic, order.ClientOid, order.OrderId, order.Price, order.Quantity, order.IsBuy);

            PublishHelper.PublishToTopic(_orderTopic, orderRabbitMsg, _messageBroker);
         }
      }

      private void ConnectionRestored(TimeSpan obj)
      {
         _logger.LogInformation("Bitfinex Websocket connection restored {Time} time interval {Span} millisecs", DateTime.Now, obj.TotalMilliseconds);
         _clientStatus.UpdatePrivateWebSocketStatus(_statusTopic, _accountName, _instanceName, true,
            $"Bitfinex Websocket up at {DateTime.UtcNow}");
      }
      
      private void ConnectionLost()
      {
         _logger.LogError("************** Bitfinex Websocket down {Time} **********", DateTime.Now);
         _clientStatus.UpdatePrivateWebSocketStatus(_statusTopic, _accountName, _instanceName, false,
            $"Bitfinex Websocket down at {DateTime.UtcNow}");
      }

      private void ConnectionClosed()
      {
         _logger.LogError("************** Bitfinex Websocket closed {Time} **********", DateTime.Now);
         _clientStatus.UpdatePrivateWebSocketStatus(_statusTopic, _accountName, _instanceName, false,
            $"Bitfinex Websocket closed at {DateTime.UtcNow}");
      }

      private void OnWalletUpdate(DataEvent<BitfinexSocketEvent<IEnumerable<BitfinexWallet>>> balances)
      {
         _logger.LogInformation("OnWalletUpdate");
         var balanceList = balances.Data.Data.ToList();
         foreach (var b in balanceList)
         {
            if (b.Type == WalletType.Exchange)
            {
               var balance = b.Clone();
               balance.Instance = _instanceName;
               balance.Account = _accountName;
               if (_coinMappingsTable.ContainsKey(balance.Currency))
               {
                  balance.Currency = _coinMappingsTable[balance.Currency].Coin.Name;
               }

               var data = JsonSerializer.Serialize(balance);
               var rabbitMsg = new MessageBusReponse()
               {
                  ResponseType = ResponseTypeEnums.BALANCE_UPDATE,
                  FromVenue = Common.Constants.BITFINEX,
                  OriginatingSource = _instanceName,
                  Data = data
               };
               PublishHelper.PublishToTopic(_balanceTopic, rabbitMsg, _messageBroker);
            }
         }
      }
   }
}
