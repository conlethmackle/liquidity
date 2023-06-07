using Binance.Config;
using Binance.Extensions;
using Binance.Net.Clients;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Spot.Socket;
using Binance.RestApi;
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
using Constants = Common.Constants;
using SymbolDataConfig = ConnectorCommon.SymbolDataConfig;

namespace Binance.Clients
{
   

   public class PrivateClient : IPrivateClient
   {
      private readonly IBinanceRestApiClient _restApiClient;
      private readonly ILogger<PrivateClient> _logger;
      private readonly IMessageBroker _messageBroker;
      private readonly IPortfolioRepository _portfolioRepository;
      private readonly IConnectorClientStatus _clientStatus;

      private readonly List<string> _genericSymbols = new List<string>();
      private List<string> _exchangeSymbols = new List<string>();
      private Dictionary<string, string> _exchangeToGenericLookup = new Dictionary<string, string>();
      private Dictionary<string, string> _genericToExchangeLookup = new Dictionary<string, string>();

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

      private BinanceSocketClient _socketClient { get; set; }

      public PrivateClient(ILoggerFactory loggerFactory, 
         IBinanceRestApiClient restApiClient, 
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

         _websocketEndpoint = config.Value.WebSocketEndpoint;
         // TODO - ENCRYPT !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
         //_apiKey = config.Value.ApiKey;
        // _secret = config.Value.SecretKey;
         _reconnectInterval = config.Value.ReconnectIntervalMilliSecs;
         _orderbookUpdateInterval = config.Value.OrderbookUpdateInterval;
         _genericSymbols = symbolData.Value.CoinPairs.ToList();
      }

      public bool LoggedInState()
      {
         return _loggedInState;
      }

      private async Task SubscribeToWebsocket()
      {
         try
         {
            var result = await _restApiClient.GetListenKey();
            if (result.Success)
            {
               var listenKey = result.Value;
               _socketClient = new BinanceSocketClient(new BinanceSocketClientOptions()
               {
                  ApiCredentials = new ApiCredentials(_accountConfig.ApiKey, _accountConfig.Secret),
                  LogLevel = LogLevel.Debug,
                  AutoReconnect = true,
                  ReconnectInterval = TimeSpan.FromMilliseconds(_reconnectInterval),
                  SpotStreamsOptions = new ApiClientOptions()
                  {
                     BaseAddress = _websocketEndpoint
                  },
               });
               _logger.LogInformation("******************* Logged into Binance Exchange for {SP}", _accountConfig.SPName);
               var subscription = await _socketClient.SpotStreams.SubscribeToUserDataUpdatesAsync(listenKey, OnOwnOrderUpdate, OnOcoOrderUpdate, OnAccountPositionMessage, OnAccountBalanceUpdate);
               if (!subscription.Success)
               {
                  _logger.LogError("Error subscribing to Spot UserDataUpdates {Error}", subscription.Error.Message);
                  throw new Exception($"Error subscribing to Spot UserDataUpdates {subscription.Error.Message}");
               }
               subscription.Data.ConnectionLost += ConnectionDown;// () => Console.WriteLine("Connection lost, trying to reconnect..");
               subscription.Data.ConnectionRestored += ConnectionRestored;//(t) => Console.WriteLine("Connection restored");
               var keepAliveTask = Task.Run(async () =>
               {
                  while (true)
                  {
                     await _restApiClient.KeepUserStreamAlive(listenKey);
                     await Task.Delay(TimeSpan.FromMinutes(10));
                  }
               });
               _logger.LogInformation("Sending a successful login message to strategy");
               // Probably ok here to indicate that we're logged in
               SendLoginStatusMsg(true, "Successfully Logged in to Binance Spot");
               _clientStatus.UpdatePrivateWebSocketStatus(_statusTopic, _accountName, _instanceName, true, "All good with private Websocket Binance");
               var result3 = await _restApiClient.GetAvailablePairs();
               if (result3.Success)
               {
                  var symbols = result3.Value;
                  foreach (var name in symbols)
                  {
                     _logger.LogInformation("Symbol is {Name}", name.Name);
                  }
               }

            }
            else
            {
               _logger.LogInformation("Sending an unsuccessful login from binance");
               SendLoginStatusMsg(false, $"Error during connection process to Binance {result.Error}");
               _logger.LogError("Error during connection process to Binance {Error}", result.Error);
               throw new Exception($"Error during connection process to Binance {result.Error}");
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error connecting to Binance Websocket {Error}", e.Message);
            throw;
         }
      }

      public async Task Init(PrivateConnectionLogon accountsConfig)
      {
         if (_loggedInState)
         {
            _logger.LogInformation("Sending a Already Logged in to Binance Spot message to strategy");
            SendLoginStatusMsg(true, "Already Logged in to Binance Spot");
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

         foreach (var symbol in _genericSymbols)
         {
            // Todo - sort this out - exchange should know enough to get from Venues and make 
            // VenueId as foreign key
            var exchangeSymbol = await _portfolioRepository.GetExchangeCoinPairSymbolFromGenericCoinPairSymbol(BinanceConnectorConstants.ConnectorName, symbol);
            if (exchangeSymbol == null)
            {
               _logger.LogError("No exchange symbol for {Symbol}", symbol);
               throw new Exception($"No exchange symbol for {symbol}");
            }
            _exchangeSymbols.Add(exchangeSymbol.ExchangeCoinpairName);
            _genericToExchangeLookup.Add(symbol, exchangeSymbol.ExchangeCoinpairName);
            _exchangeToGenericLookup.Add(exchangeSymbol.ExchangeCoinpairName, symbol);
         }

         _restApiClient.Init(accountsConfig, _statusTopic);

         await SubscribeToWebsocket();
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
            FromVenue = Common.Constants.BINANCE,
            Data = JsonSerializer.Serialize(loginStatus)
         };
         PublishHelper.PublishToTopic(_statusTopic, response, _messageBroker);
      }

      #region Commands

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

      public async Task PlaceOrder(PlaceOrderCmd cmd)
      {
         try
         {
            if (cmd.OrderType == OrderTypeEnum.LIMIT || cmd.OrderType == OrderTypeEnum.LIMITMAKER)
               _logger.LogInformation("Placing {Type} {Side} Order of Price {Price} and  Quantity {Quantity}", cmd.OrderType.ToString(), cmd.IsBuy ? "Buy" : "Sell",  cmd?.Price, cmd.Quantity);
            else if (cmd.OrderType == OrderTypeEnum.MARKET)
               _logger.LogInformation("Placing {Side} Market Order of Quantity {Quantity}", cmd.IsBuy?"Buy":"Sell", cmd.Quantity);
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.PLACE_ORDER_RESPONSE,
               FromVenue = Common.Constants.BINANCE,
               OriginatingSource = _instanceName
            };
            cmd.Price = Math.Round(cmd.Price, 2, MidpointRounding.ToEven);
          
            var binanceOrder = cmd.CloneForLimit();
            binanceOrder.Symbol = _genericToExchangeLookup[binanceOrder.Symbol];

            Result<OrderIdHolder> res = null;
            if (cmd.OrderType == OrderTypeEnum.LIMIT || cmd.OrderType == OrderTypeEnum.LIMITMAKER)
            {
                res = await _restApiClient.PlaceSpotLimitOrder(binanceOrder);
               _logger.LogInformation("Limit Order Placed {Price} {Quantity}", binanceOrder.Price, binanceOrder.Quantity);
            }
            else if (cmd.OrderType == OrderTypeEnum.MARKET)
            {
               res = await _restApiClient.PlaceSpotMarketOrder(binanceOrder);
            }
            if (res.Success)
            {
               var orderIdHolder = res.Value;
               orderIdHolder.ClientOrderId = cmd.ClientOrderId;
               orderIdHolder.Account = _accountName;
               orderIdHolder.Instance = _instanceName;
               response.Data = JsonSerializer.Serialize(orderIdHolder);
            }
            else
            {
               response.ResponseType = ResponseTypeEnums.PLACE_ORDER_ERROR;
               response.Success = false;
               var data = new PlaceOrderErrorResponse()
               {
                  ClientOrderId = cmd.ClientOrderId,
                  ErrorMessage = res.Error,
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
      public async Task GetOpenOrders(string[] genericSymbols)
      {
         try
         {
            string exchangeSymbol = null;
            foreach(var symbol in genericSymbols)
            {
               if (!String.IsNullOrEmpty(symbol))
               {
                  exchangeSymbol = _genericToExchangeLookup[symbol];
               }

               var response = new MessageBusReponse()
               {
                  ResponseType = ResponseTypeEnums.GET_OPEN_ORDERS_RESPONSE,
                  FromVenue = Common.Constants.BINANCE,
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
                  response.Success = false;
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
      public async Task CancelOrder(OrderIdHolder orderId)
      {
         try
         {
            _logger.LogInformation("Cancelling ClientOid {ClientOid} and OrderId {OrderId}", orderId.ClientOrderId, orderId.OrderId);
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.CANCEL_ORDERS_RESPONSE,
               FromVenue = Common.Constants.BINANCE,
               OriginatingSource = _instanceName
            };
            orderId.Symbol = _genericToExchangeLookup[orderId.Symbol];
            var result = await _restApiClient.CancelSpotOrder(orderId);
            if (result.Success)
            {
               _logger.LogInformation("Successful Cancel for ClientOid {ClientOid} and OrderId {OrderId}", orderId.ClientOrderId, orderId.OrderId);
               var cancelOrderList = result.Value;
               cancelOrderList.Account = _accountName;
               cancelOrderList.Instance = _instanceName;
               response.Data = JsonSerializer.Serialize(cancelOrderList);
            }
            else
            {
               response.ResponseType = ResponseTypeEnums.CANCEL_ORDER_ERROR;
               response.Success = false;
               var data = new CancelOrderResponseError()
               {
                  OrderId = orderId.OrderId,
                  ClientOid = orderId.ClientOrderId,
                  Error = result.Error,
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
      public async Task CancelAllOrdersCommand(string genericSymbol)
      {
         try
         {
            string symbol = null;
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.CANCEL_ORDERS_RESPONSE,
               FromVenue = Common.Constants.BINANCE,
               OriginatingSource = _instanceName
            };

            if (genericSymbol != null)
               symbol = _genericToExchangeLookup[genericSymbol];


            var result = await _restApiClient.CancelAllSpotOrders(symbol);
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
      public async Task GetBalances(int jobNo)
      {
         try
         {
            _logger.LogInformation("GetBalance Request ");

            var response = new MessageBusReponse()
            {
              
               FromVenue = Common.Constants.BINANCE,
               OriginatingSource = _instanceName
            };

            var result = await _restApiClient.GetBalances();
            if (result.Success)
            {
               _logger.LogInformation("GetBalance Successful Response ");
               response.ResponseType = ResponseTypeEnums.GET_BALANCE_RESPONSE;
               response.JobNo = jobNo;
               var bininanceBalances = result.Value.Balances.ToList();
               var normalisedBalances = new List<ExchangeBalance>();
               bininanceBalances.ForEach(b =>
                  {
                     var bal = b.Clone();
                     bal.Account = _accountName;
                     bal.Instance = _instanceName;
                     normalisedBalances.Add(bal);
                  }
               );
               response.Data = JsonSerializer.Serialize(normalisedBalances);
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

      private Task BulkOrderPlacementCommand(PlaceOrderCmd[] arg)
      {
         throw new NotImplementedException();
      }

      #endregion

      #region Callbacks 
      private void ConnectionRestored(TimeSpan span)
      {
         _logger.LogInformation("Binance Websocket connection restored {Time} time interval {Span} millisecs", DateTime.Now, span.TotalMilliseconds);
         _clientStatus.UpdatePrivateWebSocketStatus(_statusTopic, _accountName, _instanceName, true,
            $"Binance Websocket up at {DateTime.UtcNow}");
      }

      private void ConnectionDown()
      {
         _logger.LogError("************** Binance Websocket down {Time} **********", DateTime.Now);
         _clientStatus.UpdatePrivateWebSocketStatus(_statusTopic, _accountName, _instanceName,false,
            $"Binance Websocket down at {DateTime.UtcNow}");
      }

      private void OnAccountBalanceUpdate(DataEvent<BinanceStreamBalanceUpdate> balance)
      {
         _logger.LogInformation("************************ In OnAccountBalanceUpdate ********************** for {Account}", _accountName);
         
      }

      private void OnAccountPositionMessage(DataEvent<BinanceStreamPositionsUpdate> obj)
      {
         var balChange = obj.Data;
         var changes = balChange.Balances.ToList();
         foreach (var change in changes)
         {
            var bal = change.Clone();
            bal.Account = _accountName;
            bal.Instance = _instanceName;
            var data = JsonSerializer.Serialize(bal);
            var rabbitMsg = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.BALANCE_UPDATE,
               FromVenue = Common.Constants.BINANCE,
               OriginatingSource = _instanceName,
               Data = data
            };
            PublishHelper.PublishToTopic(_balanceTopic, rabbitMsg, _messageBroker);
         }
      }

      private void OnOcoOrderUpdate(DataEvent<BinanceStreamOrderList> obj)
      {
         // Todo - what are these!!
         throw new NotImplementedException();
      }

      private void OnOwnOrderUpdate(DataEvent<BinanceStreamOrderUpdate> update)
      {
         var o = update.Data;
         if (o.ExecutionType == Net.Enums.ExecutionType.Trade)
         {
            var trade = o.CreateTradeMsg();
            trade.Account = _accountName;
            trade.Instance = _instanceName;
            trade.Symbol = _exchangeToGenericLookup[trade.Symbol];
            trade.Venue = Constants.BINANCE;
            var data = JsonSerializer.Serialize(trade);
            var rabbitMsg = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.TRADE_UPDATE,
               FromVenue = Common.Constants.BINANCE,
               OriginatingSource = _instanceName,
               Data = data
            };
            _logger.LogInformation("Sending Trade message on Subject {Topic} with Trade Id {TradeId} Customer OrderId {CustomerOid} Order {OrderId} price {Price} quantity {Quantity} IsBuy {IsBuy}",
                                       _tradeTopic, trade.TradeId, trade.ClientOid, trade.OrderId, trade.Price, trade.Quantity, trade.IsBuy);
            PublishHelper.PublishToTopic(_tradeTopic, rabbitMsg, _messageBroker);
         }

         var order = o.Clone();
         order.Symbol = _exchangeToGenericLookup[order.Symbol];
         order.Account = _accountName;
         order.Instance = _instanceName;
         order.Venue = Constants.BINANCE;
         var orderData = JsonSerializer.Serialize(order);
         var orderRabbitMsg = new MessageBusReponse()
         {
            ResponseType = ResponseTypeEnums.OWN_ORDER_UPDATE,
            FromVenue = Common.Constants.BINANCE,
            OriginatingSource = _instanceName,
            Data = orderData
         };
         _logger.LogInformation("Sending OwnOrder message on Subject {Topic} Customer OrderId {CustomerOid} Order {OrderId} price {Price} quantity {Quantity} isbuy {IsBuy}",
                                      _orderTopic,  order.ClientOid, order.OrderId, order.Price, order.Quantity, order.IsBuy);

         PublishHelper.PublishToTopic(_orderTopic, orderRabbitMsg, _messageBroker);
      }

      public async Task Restart(Func<string, Task> action)
      {

      }

      public async Task Start(Func<string, Task> action)
      {

      }

      public async Task PlaceMarketOrder(PlaceOrderCmd cmd)
      {
         try
         {
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.PLACE_ORDER_RESPONSE,
               FromVenue = Common.Constants.BINANCE,
               OriginatingSource = _instanceName
            };
            cmd.Price = Math.Round(cmd.Price, 2, MidpointRounding.ToEven);

            var binanceOrder = cmd.CloneForMarket();
            binanceOrder.Symbol = _genericToExchangeLookup[binanceOrder.Symbol];

            var res = await _restApiClient.PlaceSpotMarketOrder(binanceOrder);
            if (res.Success)
            {
               var orderIdHolder = res.Value;
               res.Value.ClientOrderId = cmd.ClientOrderId;
               response.Data = JsonSerializer.Serialize(res.Value);
            }
            else
            {
               response.ResponseType = ResponseTypeEnums.PLACE_ORDER_ERROR;
               response.Success = false;
               var data = new PlaceOrderErrorResponse()
               {
                  ClientOrderId = cmd.ClientOrderId,
                  ErrorMessage = res.Error,
                  Account = _accountName,
                  Instance = _instanceName
               };
               response.Data = JsonSerializer.Serialize(data);
               response.Success = false;
               
            }
            PublishHelper.PublishToTopic(_orderTopic, response, _messageBroker);
         }
         catch (Exception e)
         {
            _logger.LogError("Error placing Order {Error}", e.Message);
            throw;
         }
      }
      #endregion
    }
}
