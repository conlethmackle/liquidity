using Common;
using Common.Messages;
using Common.Models;
using ConnectorCommon;
using ConnectorStatus;
using DataStore;
using KuCoin.Config;
using KuCoin.Extensions;
using KuCoin.Models;
using KuCoin.RestApi;
using KuCoin.WebSocket;
using MessageBroker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

namespace KuCoin.Clients
{
  

   public class PrivateClient : IPrivateClient
   {            
      private readonly ILogger<PrivateClient> _logger;
      private readonly IKuCoinWebSocket _webSocketHandler;
      private readonly IKuCoinRestApiClient _restClient;
      private readonly IMessageBroker _messageBroker;
      private readonly IConnectorClientStatus _clientStatus;
      private readonly IPortfolioRepository _portfolioRepository;

      private string _privateEndpoint { get; set; }
      private string _publicEndpoint { get; set; }
      private string _token { get; set; }
      private string _connection { get; set; }
      private string _connectId { get; set; }

      private readonly string[] _genericSymbols;
      private List<string> _exchangeSymbols = new List<string>();
      private Dictionary<string, string> _exchangeToGenericLookup = new Dictionary<string, string>();
      private Dictionary<string, string> _genericToExchangeLookup = new Dictionary<string, string>();

      private JsonSerializerOptions _jsonSerializerOptions { get; set; }
      private object _orderBookLock = new object();
      private List<WebsocketServerDetails> _privateServers { get; set; }
      private List<WebsocketServerDetails> _publicServers { get; set; }
      private WebsocketServerDetails _currentPrivateServer { get; set; }
      private Timer _privatePingTimer { get; set; }
      private Timer _privatePongTimer { get; set; }
      private Timer _restartSocketTimer { get; set; }
      private string _accountName { get; set; }
      private string _instanceName { get; set; }
      private string _passPhrase { get; set; }
      private string _apiKey { get; set; }
      private string _secretKey { get; set; }
      private string _orderTopic { get; set; }
      private string _tradeTopic { get; set; }
      private string _balanceTopic { get; set; }
      private string _statusTopic { get; set; }
      private WebsocketConnectState _connectionState = WebsocketConnectState.NOT_CONNECTED;
      private bool _loggedInState = false;
      public PrivateClient(ILoggerFactory loggerFactory,                    
                           IMessageBroker messageBroker, 
                           IKuCoinRestApiClient restApiClient,                       
                           IKuCoinWebSocket webSocketHandler,
                           IOptions<SymbolDataConfig> symbolData,
                           IConnectorClientStatus clientStatus,
                           IPortfolioRepository repository
                           )
      {
         _logger = loggerFactory.CreateLogger<PrivateClient>();
         _webSocketHandler = webSocketHandler;
         _restClient = restApiClient;
         _messageBroker = messageBroker;
         _clientStatus = clientStatus;
         _portfolioRepository = repository;
         _genericSymbols = symbolData.Value.CoinPairs;
      }

      public bool LoggedInState()
      {
         return   _loggedInState;
      }

      public async Task Init(PrivateConnectionLogon accountsConfig)
      {
         try
         {
            if (_loggedInState)
            {
               SendLoginStatusMsg(true, "Already Logged in to Kucoin");
               return;
            }
            _accountName = accountsConfig.SPName;
            _instanceName = accountsConfig.ConfigInstance;
            _passPhrase = accountsConfig.PassPhrase;
            _apiKey = accountsConfig.ApiKey;
            _secretKey = accountsConfig.Secret;
            _restClient.Configure(accountsConfig);
            _orderTopic = _accountName + "." + _instanceName + Constants.ORDERS_TOPIC;
            _tradeTopic = _accountName + "." + _instanceName + Constants.TRADES_TOPIC;
            _balanceTopic = _accountName + "." + _instanceName + Constants.BALANCES_TOPIC;
            _statusTopic = _accountName + "." + _instanceName + Constants.STATUS_TOPIC;
            _logger.LogInformation("******************* Logged into Kucoin Exchange for {SP}", accountsConfig.SPName);

            foreach (var symbol in _genericSymbols)
            {
               // Todo - sort this out - exchange should know enough to get from Venues and make 
               // VenueId as foreign key
               var exchangeSymbol = await _portfolioRepository.GetExchangeCoinPairSymbolFromGenericCoinPairSymbol(KucoinConnectorConstants.ConnectorName, symbol);
               if (exchangeSymbol == null)
               {
                  _logger.LogError("No exchange symbol for {Symbol}", symbol);
                  throw new Exception($"No exchange symbol for {symbol}");
               }
              
              
               if (!_exchangeSymbols.Contains(exchangeSymbol.ExchangeCoinpairName))
                  _exchangeSymbols.Add(exchangeSymbol.ExchangeCoinpairName);
               if (!_genericToExchangeLookup.ContainsKey(symbol))
                  _genericToExchangeLookup.Add(symbol, exchangeSymbol.ExchangeCoinpairName);
               if (!_exchangeToGenericLookup.ContainsKey(exchangeSymbol.ExchangeCoinpairName))
                  _exchangeToGenericLookup.Add(exchangeSymbol.ExchangeCoinpairName, symbol);
            }

            await SetupPrivateWebsocket();
         }
         catch(Exception e)
         {
            _logger.LogCritical(e, " *************** Cannot connect to KuCoin Private Servers  {Error}***************************", e.Message);
            throw;
         }
      }

      private async Task SetupPrivateWebsocket()
      {
         var result  = await _restClient.GetPrivateWebsocketInfo();
         if (result.Success)
         {
            var websocketTokenAndServer = result.Value;
            _privateServers = websocketTokenAndServer.Servers;
            _currentPrivateServer = websocketTokenAndServer.Servers[0];
            _privateEndpoint = websocketTokenAndServer.Servers[0].Endpoint;
            InitPrivateTimers(_currentPrivateServer);
            _token = websocketTokenAndServer.Token;
            _connectId = RandomString(16);
            _connection = _privateEndpoint + "?" + "token=" + _token + "&[connectId=" + _connectId + "]";
            _webSocketHandler.InitPrivate(_privateEndpoint, _token, _connectId);
            await _webSocketHandler.ConnectPrivate();
            _webSocketHandler.RunPrivate(ProcessPrivateResponse);
         }
         else
         {
            SendLoginStatusMsg(false, $"Error connecting for private websocket information {result.Error}");
            _logger.LogError("Error connecting for private websocket information {Error}", result.Error);
            throw new Exception($"Error connecting for private websocket information {result.Error}");
         }
      }

      private void InitPrivateTimers(WebsocketServerDetails privateServer)
      {
         _privatePingTimer = new Timer();
         _privatePingTimer.Elapsed += PrivateSendPingTimerCallback;
         _privatePingTimer.Interval = privateServer.PingInterval;
         _privatePingTimer.Enabled = true;

         _privatePongTimer = new Timer();
         _privatePongTimer.Elapsed += PrivatePongTimerCallback;
         _privatePongTimer.Interval = privateServer.PingTimeout;
         //_privatePongTimer.Enabled = true;

         _restartSocketTimer = new Timer();
         _restartSocketTimer.Elapsed += PrivateRestartWebsocketTimer;
         _restartSocketTimer.Interval = 1000;
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
            ResponseType = ResponseTypeEnums.PRIVATE_CLIENT_LOGIN_STATUS,
            FromVenue = Common.Constants.KUCOIN,
            Data = JsonSerializer.Serialize(loginStatus)
         };
         PublishHelper.PublishToTopic(_statusTopic, response, _messageBroker);
      }

      public static string RandomString(int length)
      {
         var random = new Random();
         const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
         return new string(Enumerable.Repeat(chars, length)
             .Select(s => s[random.Next(s.Length)]).ToArray());
      }

      private void PrivateSendPingTimerCallback(object o, ElapsedEventArgs e)
      {
         _privatePingTimer.Enabled = false;
         if (_webSocketHandler.IsPrivateConnected())
         {
            var pingData = new PingPongData()
            {
               Id = _connectId,
               Type = "ping"
            };
            var ping = JsonSerializer.Serialize(pingData);
            _webSocketHandler.SendPrivate(ping);
         }
         _privatePongTimer.Enabled = true;
         if (_connectionState == WebsocketConnectState.CONNECTED)
            _privatePingTimer.Enabled = true;
      }

      private void PrivatePongTimerCallback(object o, ElapsedEventArgs e)
      {
         _privatePongTimer.Enabled = false;
         if (_webSocketHandler.IsPrivateConnected())
         {
            // This indicates that the socket is down
            if (_connectionState == WebsocketConnectState.CONNECTED)
            {
               _logger.LogError("***************** KuCoin Pong timer for {Account} has expired Websocket down", _accountName);
               _clientStatus.UpdatePrivateWebSocketStatus(_statusTopic, _accountName, _instanceName, true,
                  $"Pong Timer has fired - private websocket down {_instanceName}"); //_accountName, false, "Pong Timer has fired - private websocket down");
               _restartSocketTimer.Enabled = true;
               _connectionState = WebsocketConnectState.NOT_CONNECTED;
              
            }          
         }        
      }

      private async void PrivateRestartWebsocketTimer(object sender, ElapsedEventArgs e)
      {
         // TODO - need to cycle round the list of available servers. Should have a restart module I think
         // that can do both private and public clients
         _restartSocketTimer.Enabled = false;
         _connectionState = WebsocketConnectState.CONNECTING;
         await _webSocketHandler.DisconnectPrivate("Restarting the private websocket connection");
         await SetupPrivateWebsocket();
         _restartSocketTimer.Enabled = true;
      }

      public async Task Start(Func<string, Task> privateAction)
      {
         await Restart(privateAction);
         //_websocketPart.Run(action);
      }

      public async Task Restart(Func<string, Task> privateAction)
      {
         try
         {
            await _webSocketHandler.ConnectPrivate();          
            _webSocketHandler.RunPrivate(privateAction);
         }
         catch (Exception e)
         {
            _logger.LogError($"Error connecting to websocket {e.Message}");
            throw;
         }
      }

      #region Commands
      public async Task PlaceOrder(PlaceOrderCmd cmd)
      {
         try
         {
            Result<OrderIdHolder> res = null;

            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.PLACE_ORDER_RESPONSE,
               FromVenue = Constants.KUCOIN,
               AccountName = _accountName
            };
            
            if (cmd.OrderType == OrderTypeEnum.MARKET)
            {
               var kuCoinOrder = cmd.CloneForMarket();
               kuCoinOrder.Symbol = _genericToExchangeLookup[kuCoinOrder.Symbol];
               res = await _restClient.PlaceSingleMarketOrder(kuCoinOrder);
            }
            else
            {
               var kuCoinOrder = cmd.Clone();
               kuCoinOrder.Symbol = _genericToExchangeLookup[kuCoinOrder.Symbol];
               res = await _restClient.PlaceSingleOrder(kuCoinOrder);
            }
            
           
            if (res.Success)
            {
               var orderIdHolder = res.Value;
               orderIdHolder.ClientOrderId = cmd.ClientOrderId;
               orderIdHolder.Symbol = cmd.Symbol;
               orderIdHolder.Account = _accountName;
               orderIdHolder.Instance = _instanceName;

               response.Data = JsonSerializer.Serialize(res.Value);
            }
            else
            {
               response.Success = false;
              
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
            foreach (var symbol in genericSymbols)
            {
               var exchangeSymbol = _genericToExchangeLookup[symbol];
               var response = new MessageBusReponse()
               {
                  ResponseType = ResponseTypeEnums.GET_OPEN_ORDERS_RESPONSE,
                  FromVenue = Constants.KUCOIN,
                  AccountName = _accountName,
                  //Data = data
               };
               var result = await _restClient.GetOpenOrdersList(exchangeSymbol);
               if (result.Success)
               {
                  var toListOrders = result.Value.ToList();
                  var orderList = new List<OrderQueryResponse>();
                  toListOrders.ForEach(o =>
                  {
                     var oo = o.Clone();
                     oo.Symbol = _exchangeToGenericLookup[o.Symbol];
                     oo.Account = _accountName;
                     oo.Instance = _instanceName;
                     orderList.Add(oo);
                  });
                  response.Data = JsonSerializer.Serialize(orderList);
               }
               else
               {
                  response.ResponseType = ResponseTypeEnums.OPEN_ORDERS_ERROR;
                  response.Success = false;
                  var data = new PlaceOrderErrorResponse()
                  {
                     ErrorMessage = result.Error,
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
            _logger.LogError("Error getting Open Orders {Error}", e.Message);
            throw;
         }
      }

      public async Task GetBalances(int jobNo)
      {
         try
         {
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.GET_BALANCE_RESPONSE,
               FromVenue = Constants.KUCOIN,
               AccountName = _accountName,
            };

            var result = await _restClient.GetAccounts();
            if (result.Success)
            {
               var kuCoinBalances = result.Value.ToList();
               var normalisedBalances = new List<ExchangeBalance>();
               kuCoinBalances.ForEach(b =>
               {
                  if (b.AccountType.Equals("trade"))
                  {
                     var bal = b.Clone();
                     bal.Account = _accountName;
                     bal.Instance = _instanceName;
                     normalisedBalances.Add(bal);
                  }
               }
               );
               response.Data = JsonSerializer.Serialize(normalisedBalances);
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
            _logger.LogError("Error in GetBalances {Error}", e.Message);
            throw;
         }
      }

      public async Task CancelOrder(OrderIdHolder orderId)
      {
         try
         {
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.CANCEL_ORDERS_RESPONSE,
               FromVenue = Constants.KUCOIN,
               AccountName = _accountName, 
               
            };

            orderId.Symbol = _genericToExchangeLookup[orderId.Symbol];
            var result = await _restClient.CancelOrder(orderId.OrderId);
            if (result.Success)
            {
               result.Value.ClientOrderId = orderId.ClientOrderId;
               var x = result.Value;
               x.Account = _accountName;
               x.Instance = _instanceName;
               var data = JsonSerializer.Serialize(x);
               response.Data = data;
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
            PublishHelper.Publish(_orderTopic, response, _messageBroker);
         }
         catch (Exception e)
         {
            _logger.LogError("Error cancelling Order {OrderId} {Error}", orderId, e.Message);
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
               ResponseType = ResponseTypeEnums.CANCEL_ALL_ORDERS_RESPONSE,
               FromVenue = Constants.KUCOIN,
               AccountName = _accountName             
            };

            if (genericSymbol != null)
               symbol = _genericToExchangeLookup[genericSymbol];


            var result = await _restClient.CancelAllOrders(symbol);
            if (result.Success)
            {
               var data = JsonSerializer.Serialize(result.Value);
               response.Data = data;
            }
            else
            {
               response.Success = false;
               response.Data = result.Error;
            }
            PublishHelper.Publish(_orderTopic, response, _messageBroker);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error cancelling All Orders {Error}", e.Message);
            throw;
         }
      }
      #endregion

      #region Responses
      public async Task ProcessPrivateResponse(string reply)
      {
         try
         {           
            _logger.LogDebug(reply);
            var response = JsonSerializer.Deserialize<BasicResponseData>(reply);
            var type = response.Type;
            switch (type)
            {
               case "pong":
                  _privatePongTimer.Enabled = false;
                  _restartSocketTimer.Enabled = false;
                  if (_connectionState != WebsocketConnectState.CONNECTED)
                  {
                     _connectionState = WebsocketConnectState.CONNECTED;
                     _clientStatus.UpdatePrivateWebSocketStatus(_statusTopic, _accountName, _instanceName,true, "");
                     _logger.LogInformation("Websocket connection for {Account} is UP", _accountName);
                  }
                    
                  break;
               case "welcome":
                  // Successful login
                  SendLoginStatusMsg(true, "Successfully Logged in to Kucoin");
                  await SubscribeForEverything();

                  break;
               case "message":
                  HandlePrivateMessage(response, reply);
                  break;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error processing private response {Reply} {Error}", reply, e.Message);
         }
      }

      private void HandlePrivateMessage(BasicResponseData response, string reply)
      {
         switch (response.Topic)
         {
            case "/spotMarket/tradeOrders":
               HandleOrderUpdates(response, reply);
               break;
            case "/account/balance":
               HandleAccountUpdate(response, reply);
               break;
         }
      }

      private void HandleAccountUpdate(BasicResponseData response, string reply)
      {
         if (response.Subject.Equals("account.balance"))
         {
            var msg = JsonSerializer.Deserialize<ResponseData<AccountBalanceNotice>>(reply, _jsonSerializerOptions);
            var balanceChangeData = msg.Data;
            var bal = balanceChangeData.Clone();
            bal.Account = _accountName;
            bal.Instance = _instanceName;
            var data = JsonSerializer.Serialize(bal);
            var rabbitMsg = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.BALANCE_UPDATE,
               FromVenue = Constants.KUCOIN,
               Data = data
            };
            PublishHelper.PublishToTopic(_balanceTopic, rabbitMsg, _messageBroker);
          
         }
      }

      private void HandleOrderUpdates(BasicResponseData response, string reply)
      {
         switch (response.Subject)
         {
            case "orderChange":
               HandleOwnOrderChange(reply);
               break;
         }
      }

      private void HandleOwnOrderChange(string reply)
      {
         try
         {

            var msg = JsonSerializer.Deserialize<ResponseData<PrivateOrderChangeEvent>>(reply, _jsonSerializerOptions);

            var orderChangeData = msg.Data;

            var ownOrderUpdate = orderChangeData.Clone();
            ownOrderUpdate.Symbol = _exchangeToGenericLookup[ownOrderUpdate.Symbol];
            ownOrderUpdate.Account = _accountName;
            ownOrderUpdate.Instance = _instanceName;
            ownOrderUpdate.Venue = Constants.KUCOIN;
            var data = JsonSerializer.Serialize(ownOrderUpdate);
            var rabbitMsg = new MessageBusReponse()
            {
               //ResponseType = ResponseTypeEnums.OWN_ORDER_UPDATE,
               FromVenue = Constants.KUCOIN,
               Data = data
            };

            switch (orderChangeData.Type)
            {
               case "open":
                  rabbitMsg.ResponseType = ResponseTypeEnums.NEW_ORDER;
                  break;
               case "match":
                  CheckForATrade(orderChangeData);
                  return;
               case "filled":
                  rabbitMsg.ResponseType = ResponseTypeEnums.FILLED_ORDER;
                  break;
               case "canceled":
                  rabbitMsg.ResponseType = ResponseTypeEnums.CANCELLED_ORDER;
                  break;
               case "update":
                  rabbitMsg.ResponseType = ResponseTypeEnums.PARTIALLY_FILLED_ORDER;
                  break;
            }

            PublishHelper.PublishToTopic(_orderTopic, rabbitMsg, _messageBroker);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error in HandleOwnOrderChange {Error}", e.Message);
         }
      }

      private async void CheckForATrade(PrivateOrderChangeEvent e)
      {
         if (e.Type.Equals("match"))
         {
            var tradeMsg = e.CreateTradeMsg();
            tradeMsg.Symbol = _exchangeToGenericLookup[tradeMsg.Symbol];

            // This next step is a bloody pain. Need the fee data
            var result = await _restClient.GetRecentFills();
            if (result.Success)
            {
               var recentFillsList = result.Value.ToList().Distinct();
               var dict = recentFillsList.ToDictionary(x => x.TradeId, x => x);

               if (dict.ContainsKey(tradeMsg.TradeId))
               {
                  var fill = dict[tradeMsg.TradeId];
                  tradeMsg.Fee = fill.Fee;
                  tradeMsg.FeeCurrency = fill.FeeCurrency;
                  tradeMsg.FeeRate = fill.FeeRate;
                  tradeMsg.Account = _accountName;
                  tradeMsg.Instance = _instanceName;
                  tradeMsg.Venue = Constants.KUCOIN;
               }

               var tradeData = JsonSerializer.Serialize(tradeMsg);
               var rabbitTradeMsg = new MessageBusReponse()
               {
                  ResponseType = ResponseTypeEnums.TRADE_UPDATE,
                  FromVenue = Constants.KUCOIN,
                  Data = tradeData
               };
               PublishHelper.PublishToTopic(_tradeTopic, rabbitTradeMsg, _messageBroker);
            }
         }
      }

      private async Task SubscribeForEverything()
      {        
         await SubscribeForOwnOrders();
         await SubscribeForBalanceChanges();
      }

      private async Task SubscribeForOwnOrders()
      {
         Subscription sub = new Subscription()
         {
            Id = RandomString(16),
            Topic = "/spotMarket/tradeOrders",
            Type = "subscribe",
            Response = true,
            PrivateChannel = true
         };

         var msg = JsonSerializer.Serialize(sub);
         await _webSocketHandler.SendPrivate(msg);
      }

      private async Task SubscribeForBalanceChanges()
      {
         Subscription sub = new Subscription()
         {
            Id = RandomString(16),
            Topic = "/account/balance",
            Type = "subscribe",
            Response = true,
            PrivateChannel = true
         };

         var msg = JsonSerializer.Serialize(sub);
         await _webSocketHandler.SendPrivate(msg);
      }

      public Task DisconnectPrivate()
      {
         throw new NotImplementedException();
      }

      public Task PlaceMarketOrder(PlaceOrderCmd cmd)
      {
         throw new NotImplementedException();
      }
      #endregion

   }
}
