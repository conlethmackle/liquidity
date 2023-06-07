using AccountBalanceManager;
using BlazorLiquidity.Server.Hubs;
using BlazorLiquidity.Server.OrdersAndTrades;
using BlazorLiquidity.Server.Receiver;
using BlazorLiquidity.Shared;
using ClientConnections;
using Common.Messages;
using Common.Models;
using Common.Models.DTOs;
using Common.Models.Entities;
using ConnectorStatus;
using DataStore;
using FairValueProcessing;
using MessageBroker;
using Microsoft.AspNetCore.SignalR;
using MultipleStrategyManager;
using OrderAndTradeProcessing;
using OrderBookProcessing;
using StrategyMessageListener;
using System.Collections.Concurrent;
using System.Text.Json;
using BlazorLiquidity.Server.FairValueProcessing;
using Timer = System.Timers.Timer;

namespace BlazorLiquidity.Server.RealTime
{
    
    public interface IRealTimeUpdater
    {
      Task Init(int strategyConfigId);
      void  Init(IInventoryManager accountInventoryManager, IOrderAndTradeProcessing orderAndTradeProcessing);
      void OnStrategyAlivePing(StrategyInstanceConnectionStatus status);
      void OnStrategyControlResponse(StrategyControlResponse strategyResponseMsg);
   }
    public class RealTimeUpdater : IRealTimeUpdater
    {
      const string InstanceName = "DashboardServer";
      protected readonly ILogger<RealTimeUpdater> _logger;
      protected  IInventoryManager _inventoryManager { get; set; }
      protected  IOrderAndTradeProcessing _orderAndTradeProcessing { get; set; }
      protected readonly IOrderBookProcessor _orderBookProcessor;
      protected readonly IFairValuePricingUI _fairValuePricing;
      protected readonly IPortfolioRepository _portfolioRepository;
     // protected readonly IPrivateClientConnections _privateClientConnections;
    //  protected readonly IPublicClientConnections _publicClientConnections;
      protected readonly IMessageBroker _messageBroker;
      protected readonly IMessageReceiver _messageReceiver;
      protected readonly IConnectorStatusListener _connectorStatusListener;
      protected readonly IHubContext<PortfolioHub, IPortfolioHub> _hub;
     
      protected  string _portfolioName;
      protected  string _configName;
      protected ConcurrentQueue<MessageQueueData> _messageQueue = new ();
      private Dictionary<int, bool> _InitialisedTable = new Dictionary<int, bool>();
      protected PeriodicTimer _periodicTimer;
      protected Timer _timer;
      private CancellationTokenSource _cts;
      private Dictionary<string, bool> _balanceSentTable { get; set; } = new Dictionary<string, bool>();
      private Timer _strategyAliveTimer;
      private int _currentStrategyAliveCounter = 0;
      private int _numberOfStrategyAlivePings = 0;

      public RealTimeUpdater(ILoggerFactory loggerFactory,
                        //  IInventoryManager inventoryManager,
                        //  IOrderAndTradeProcessing orderTradeProcessing,
                          IOrderBookProcessor orderBookProcessor,
                          IFairValuePricingUI fairValuePricing,
                          IPortfolioRepository repository,
                    
                          IMessageReceiver messageReceiver,
                          IConnectorStatusListener connectorStatusListener,
                          IHubContext<PortfolioHub, IPortfolioHub> hub,
                          IMessageBroker messageBroker)
      {
          _logger = loggerFactory.CreateLogger<RealTimeUpdater>();
    
          _orderBookProcessor = orderBookProcessor;
          _fairValuePricing = fairValuePricing;
          _portfolioRepository = repository;
          _hub = hub;
         
          _messageReceiver = messageReceiver;
          _messageBroker = messageBroker;
          _connectorStatusListener = connectorStatusListener;
          _connectorStatusListener.OnConnectorIsUp += OnConnectorIsUp;
          _connectorStatusListener.OnConnectorIsDown += OnConnectorIsDown;
          _connectorStatusListener.OnPrivateConnectivityIssue += OnPrivateConnectivityIssue;
          _connectorStatusListener.OnPrivateConnectivityIssueCleared += OnPrivateConnectivityIssueCleared;
             _fairValuePricing.OnFairValuePriceChange += OnFairValuePriceChange;
          _messageReceiver.OnPrivateLoginStatus += PrivateClientLogonMsg;
         _messageReceiver.OnStrategyAlivePing += OnStrategyAlivePing;
         _messageReceiver.OnStartOfDay += OnStartOfDay;
         _fairValuePricing.Init();
      }

      

      public void Init(IInventoryManager accountInventoryManager, IOrderAndTradeProcessing orderAndTradeProcessing)
      {
         _inventoryManager = accountInventoryManager;
         _orderAndTradeProcessing = orderAndTradeProcessing;
         _inventoryManager.OnBalanceUpdate += OnBalanceUpdate;
         _inventoryManager.OnOpeningBalance += OnOpeningBalanceUpdate;
         _orderAndTradeProcessing.OnNewOrder += OnNewOrder;
         _orderAndTradeProcessing.OnCancelledOrder += OnCancelledOrder;
         _orderAndTradeProcessing.OnPartiallyFilledOrder += OnPartiallyFilledOrder;
         _orderAndTradeProcessing.OnFilledOrder += OnFilledOrder;
         _orderAndTradeProcessing.OnNewTrade += OnNewTrade;
         _orderAndTradeProcessing.OnOpenOrdersResponse += OnOpenOrdersResponse;

      }


      private void NoStrategyAlivePingsReceived()
      {
         var status = new StrategyInstanceConnectionStatus
         {
            InstanceName = _configName,
            Status = false,
            ErrorMsg = "",
         };

         var qData = new MessageQueueData()
         {
            MessageType = QueueMsgTypes.STRATEGY_ALIVE_PING,
            Venue = status.InstanceName,
            Data = JsonSerializer.Serialize(status)
         };
         _messageQueue.Enqueue(qData);
      }

      public async Task Init(int strategyConfigId)
      {
         if (_InitialisedTable.ContainsKey(strategyConfigId)) 
             return;
         _InitialisedTable.Add(strategyConfigId, true);
         var configs = await _portfolioRepository.GetOpeningLiquidationSubscriptionForStrategySPSubscriptionId(strategyConfigId);
         if (configs != null)
         {
            _portfolioName = configs.SP.Name;
            _configName = configs.StrategySPSubscriptionConfig.ConfigName;
            _inventoryManager.InitCoins(configs.CoinPair.Name);
            await _connectorStatusListener.Init(_configName);
            await _messageReceiver.Start(_portfolioName, _configName, strategyConfigId);

            foreach (var exchange in configs?.StrategySPSubscriptionConfig.ExchangeDetails)
            {
               try
               {
                  GetOrderBooks(exchange);
                  LoginToVenue(_portfolioName, _configName, exchange);
                  //    _inventoryManager.GetOpeningBalancesDirect(exchange.Venue.VenueName, InstanceName, _portfolioName);
               }
               catch (Exception e)
               {
                  _logger.LogError(e, "Error in getting orderbooks {Error}", e.Message);
               }
            }
            StartRealTime();
         }
      }

      public void StartRealTime()
      {
         Task.Run(async () =>
         {
            try
            {
               for (;;)
               {
                  while (!_messageQueue.IsEmpty)
                  {
                     if (_messageQueue.TryDequeue(out var result))
                     {
                        //Console.WriteLine($"Sending a message of type {result.MessageType.ToString()}");
                        await _hub.Clients.All.RealTimeUpdate(result);
                     }
                  }

                  // Check StrategyAlivePing
                  _currentStrategyAliveCounter++;
                  if (_currentStrategyAliveCounter == 10) // TODO - magic number
                  {
                     _currentStrategyAliveCounter = 0;
                     if (_numberOfStrategyAlivePings == 0)
                     {
                        NoStrategyAlivePingsReceived();
                     }
                     _numberOfStrategyAlivePings = 0;
                  }

                  await Task.Delay(TimeSpan.FromSeconds(1));
               }
            }
            catch (Exception e)
            {
               //Handle the exception but don't propagate it
               _logger.LogError(e, "Error in  periodic timer {Error}", e.Message);
            }
            
         });
      }

      private void OnOpenOrdersResponse(string venue, List<OwnOrderChange> openOrders)
      {
         if (openOrders.Count == 0) return;
         openOrders.ForEach(o =>
         {
            o.Instance = _configName;
            o.Account = _portfolioName;
         });
      //   string[] venues = venue.Split("_");
         var qData = new MessageQueueData
         {
            MessageType = QueueMsgTypes.OPEN_ORDERS_RESPONSE,
            Venue = venue,
            Data = JsonSerializer.Serialize(openOrders)
         };
         _messageQueue.Enqueue(qData);
      }

      private void OnNewTrade(string venue, TradeMsg tradeMsg)
      {
         _logger.LogInformation("Realtime updater - Queuing new Trade - {OrderId}", tradeMsg.OrderId);
         string[] venues = venue.Split("_");
         var qData = new MessageQueueData
         {
            MessageType = QueueMsgTypes.TRADE,
            Venue = venue,
            Data = JsonSerializer.Serialize(tradeMsg)
         };
         _messageQueue.Enqueue(qData);

      }

      private void OnFilledOrder(string venue, OwnOrderChange order)
      {
         string[] venues = venue.Split("_");

         var qData = new MessageQueueData
         {
            MessageType = QueueMsgTypes.FILLEDORDER,
            Venue = venue,
            Data = JsonSerializer.Serialize(order)
         };
         _messageQueue.Enqueue(qData);
      }

      private void OnPartiallyFilledOrder(string venue, OwnOrderChange order)
      {
         string[] venues = venue.Split("_");
         var qData = new MessageQueueData
         {
            MessageType = QueueMsgTypes.PARTIALLYFILLEDORDER,
            Venue = venue,
            Data = JsonSerializer.Serialize(order)
         };
         _messageQueue.Enqueue(qData);
      }

      private void OnCancelledOrder(string venue, OwnOrderChange order)
      {
         string[] venues = venue.Split("_");
         var qData = new MessageQueueData
         {
            MessageType = QueueMsgTypes.CANCELLEDORDER,
            Venue = venue,
            Data = JsonSerializer.Serialize(order)
         };
         _messageQueue.Enqueue(qData);
      }

      private void OnNewOrder(string venue, OwnOrderChange order)
      {
         string[] venues = venue.Split("_");
         var qData = new MessageQueueData
         {
            MessageType = QueueMsgTypes.NEWORDER,
            Venue = venue,
            Data = JsonSerializer.Serialize(order)
         };
         _messageQueue.Enqueue(qData);
      }

      private void OnFairValuePriceChange(string venue, string symbol, decimal fairValuePrice)
      {
     //    _logger.LogInformation($"****REALTIME UPDATER*********** Got a fair value update price is {fairValuePrice}");
         var data = new FairValueData()
         {
               Venue = venue,
               Price = fairValuePrice,
               Symbol = symbol,
         };
         var qData = new MessageQueueData
         {
            MessageType = QueueMsgTypes.FAIRVALUEUPDATE,
            Data = JsonSerializer.Serialize(data)
         };
         _messageQueue.Enqueue(qData);
            //await _hub.Clients.All.FairValueUpdate(symbol, fairValuePrice);
      }

      private void OnOpeningBalanceUpdate(string venue, string currency, Common.Models.ExchangeBalance balance)
      {
         var data = new BalanceUpdate()
         {
            Balance = balance,
            Currency = currency,
            Venue = venue
         };
         var qData = new MessageQueueData
         {
            MessageType = QueueMsgTypes.OPENINGBALANCE,
            Venue = venue,
            Data = JsonSerializer.Serialize(data)
         };
         _messageQueue.Enqueue(qData);
            //await _hub.Clients.All.OpeningBalance(venue, currency, balance);
      }

        private void OnBalanceUpdate(string venue, string currency, ExchangeBalance balance)
        {
         var data = new BalanceUpdate()
         {
            Balance = balance,
            Currency = currency,
            Venue = venue
         };
         var qData = new MessageQueueData
         {
            MessageType = QueueMsgTypes.BALANCEUPDATE,
            Venue = venue,
            Data = JsonSerializer.Serialize(data)
         };
         _messageQueue.Enqueue(qData);
      }

     
      private void LoginToVenue(string portfolioName, string instanceName, ExchangeDetailsDTO exchange)
      {
   //      _logger.LogInformation("Logging in to {Venue} for Account {Name} and instance {Instance}",
   //                                  exchange.Venue.VenueName, portfolioName, instanceName);
         MessageBusCommand msgPrivate = new MessageBusCommand()
         {
            AccountName = portfolioName,
            InstanceName = instanceName,
            CommandType = CommandTypesEnum.CONNECT_PRIVATE,
         };

         var apiKeys = exchange.ApiKey;
         PrivateConnectionLogon logon = new PrivateConnectionLogon()
         {
            SPName = portfolioName,
            ConfigInstance = instanceName,
            ApiKey = apiKeys.Key,
            PassPhrase = apiKeys.PassPhrase,
            Secret = apiKeys.Secret,
         };

         try
         {
            var data = JsonSerializer.Serialize(logon);
            msgPrivate.Data = data;
            var bytesRef = MessageBusCommand.ProtoSerialize(msgPrivate);
            _messageBroker.PublishToSubject(exchange.Venue.VenueName, bytesRef);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error in sending Login command {Error}", e.Message);
         }

      }

      private void PrivateClientLogonMsg(string venue, PrivateClientLoginStatus status)
      {
         if (status.IsLoggedIn)
         {
            _inventoryManager.GetOpeningBalancesDirect(venue, InstanceName, _portfolioName);
         }
      }

      private void OnConnectorIsDown(string venue)
      {
         QueueConnectorStatus(venue, false, true, "");
      }

      private void OnConnectorIsUp(string venue)
      {
         QueueConnectorStatus(venue, true, true, "");
      }

      private void OnPrivateConnectivityIssueCleared(string venue)
      {
         throw new NotImplementedException();
      }

      private void OnPrivateConnectivityIssue(string venue)
      {
         throw new NotImplementedException();
      }

      private void QueueConnectorStatus(string venue, bool status, bool isPublic, string msg = null)
      {
         
         var data = new ConnectorStatusMsg();
         if (isPublic)
         {
            data.Public.IsConnected = status;
            data.Public.Venue = venue;
            data.Public.ErrorMsg = msg;
         }
         else
         {
            data.Account = _portfolioName;
            data.Instance = _configName;
            data.Private.IsConnected = status;
            data.Private.Venue = venue;
            data.Private.ErrorMsg = msg;
         }

         var qData = new MessageQueueData
         {
            MessageType = QueueMsgTypes.CONNECTOR_STATUS,
            Venue = venue,
            IsPublic = isPublic,
            Data = JsonSerializer.Serialize(data)
         };
         _messageQueue.Enqueue(qData);
      }

      private void OnStartOfDay()
      {
         var qData = new MessageQueueData
         {
            MessageType = QueueMsgTypes.START_OF_DAY
         };
         _messageQueue.Enqueue(qData);
      }

      private void GetOrderBooks(ExchangeDetailsDTO exchange)
      {
         _logger.LogInformation("Sending a request to {Venue} for Order book for {CoinPairs}",
                                   exchange.Venue.VenueName, exchange.CoinPairs);
         var pairs = exchange.CoinPairs;
         MessageBusCommand msgPublic = new MessageBusCommand()
         {
            AccountName = _portfolioName,
            InstanceName = InstanceName, //+ "." + _configName,
            CommandType = CommandTypesEnum.GET_ORDERBOOK,
            Data = pairs
         };
         var bytesRef = MessageBusCommand.ProtoSerialize(msgPublic);
         _messageBroker.PublishToSubject(exchange.Venue.VenueName, bytesRef);
      }

      public void OnStrategyControlResponse(StrategyControlResponse strategyResponseMsg)
      {
         if (strategyResponseMsg.ConfigName == _configName)
         {
            var qData = new MessageQueueData()
            {
               MessageType = QueueMsgTypes.STRATEGY_CONTROL_MSG,
               Venue = strategyResponseMsg.ConfigName,
               Data = JsonSerializer.Serialize(strategyResponseMsg)
            };
            _messageQueue.Enqueue(qData);
         }
      }

      public void OnStrategyAlivePing(StrategyInstanceConnectionStatus status)
      {

         if (status.InstanceName == _configName)
         {
            _numberOfStrategyAlivePings++;

            var qData = new MessageQueueData
            {
               MessageType = QueueMsgTypes.STRATEGY_ALIVE_PING,
               Venue = status.InstanceName,
               Data = JsonSerializer.Serialize(status)
            };
            _messageQueue.Enqueue(qData);
            _logger.LogInformation("Received Strategy Alive ping for {}", _configName);
         }

      }
   }
}
