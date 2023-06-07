using AccountBalanceManager;
using SyncfusionLiquidity.Server.Hubs;
using SyncfusionLiquidity.Server.OrdersAndTrades;
using SyncfusionLiquidity.Server.Receiver;
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
using OrderAndTradeProcessing;
using OrderBookProcessing;
using StrategyMessageListener;
using System.Collections.Concurrent;
using System.Text.Json;


namespace SyncfusionLiquidity.Server.RealTime
{
    
    public interface IRealTimeUpdater
    {
      Task Init(int strategyConfigId);
    }
    public class RealTimeUpdater : IRealTimeUpdater
    {
      const string InstanceName = "DashboardServer";
      protected readonly ILogger<RealTimeUpdater> _logger;
      protected readonly IInventoryManager _inventoryManager;
      protected readonly IOrderAndTradeProcessing _orderAndTradeProcessing;
      protected readonly IOrderBookProcessor _orderBookProcessor;
      protected readonly IFairValuePricing _fairValuePricing;
      protected readonly IPortfolioRepository _portfolioRepository;
      protected readonly IPrivateClientConnections _privateClientConnections;
      protected readonly IPublicClientConnections _publicClientConnections;
      protected readonly IMessageBroker _messageBroker;
      protected readonly IPortFolioMessageReceiver _messageReceiver;
      protected readonly IConnectorStatusListener _connectorStatusListener;
      protected readonly IHubContext<PortfolioHub, IPortfolioHub> _hub;
     
      protected  string _portfolioName;
      protected  string _configName;
      protected ConcurrentQueue<MessageQueueData> _messageQueue = new ConcurrentQueue<MessageQueueData>();
      private Dictionary<int, bool> _InitialisedTable = new Dictionary<int, bool>();
      protected PeriodicTimer _periodicTimer;
      private CancellationTokenSource _cts;
      private Dictionary<string, bool> _balanceSentTable { get; set; } = new Dictionary<string, bool>();

      public RealTimeUpdater(ILoggerFactory loggerFactory,
                          IInventoryManager inventoryManager,
                          IOrderAndTradeProcessing orderTradeProcessing,
                          IOrderBookProcessor orderBookProcessor,
                          IFairValuePricing fairValuePricing,
                          IPortfolioRepository repository,
                          IPrivateClientConnections privateClientConnections,
                          IPublicClientConnections publicClientConnections,                        
                          IPortFolioMessageReceiver messageReceiver,
                          IConnectorStatusListener connectorStatusListener,
                          IHubContext<PortfolioHub, IPortfolioHub> hub,
                          IMessageBroker messageBroker)
      {
          _logger = loggerFactory.CreateLogger<RealTimeUpdater>();
          _inventoryManager = inventoryManager;
          _orderAndTradeProcessing = orderTradeProcessing;
          _orderBookProcessor = orderBookProcessor;
          _fairValuePricing = fairValuePricing;
          _portfolioRepository = repository;
          _privateClientConnections = privateClientConnections;
          _publicClientConnections = publicClientConnections;
          _hub = hub;
         
          _messageReceiver = messageReceiver;
          _messageBroker = messageBroker;
          _connectorStatusListener = connectorStatusListener;
          _connectorStatusListener.OnConnectorIsUp += OnConnectorIsUp;
          _connectorStatusListener.OnConnectorIsDown += OnConnectorIsDown;
          _inventoryManager.OnBalanceUpdate += OnBalanceUpdate;
          _inventoryManager.OnOpeningBalance += OnOpeningBalanceUpdate;
         _fairValuePricing.OnFairValuePriceChange += OnFairValuePriceChange;

         _messageReceiver.OnPrivateLoginStatus += PrivateClientLogonMsg;
         _messageReceiver.OnStrategyAlivePing += OnStrategyAlivePing;

         _orderAndTradeProcessing.OnNewOrder += OnNewOrder;
         _orderAndTradeProcessing.OnCancelledOrder += OnCancelledOrder;
         _orderAndTradeProcessing.OnPartiallyFilledOrder += OnPartiallyFilledOrder;
         _orderAndTradeProcessing.OnFilledOrder += OnFilledOrder;
         _orderAndTradeProcessing.OnNewTrade += OnNewTrade;
         _orderAndTradeProcessing.OnOpenOrdersResponse += OnOpenOrdersResponse;

         _periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
         _cts = new CancellationTokenSource();
      }

      public async Task Init(int strategyConfigId)
      {
         if (_InitialisedTable.ContainsKey(strategyConfigId)) 
             return;
         _InitialisedTable.Add(strategyConfigId, true);
         var configs = await _portfolioRepository.GetLiquidationStrategyConfigByStrategyConfigId(strategyConfigId);
         _portfolioName = configs.StrategySPSubscriptionConfig.SP.Name;
         _configName = configs.StrategySPSubscriptionConfig.ConfigName;
         _inventoryManager.InitCoins(configs.Symbol);
         await _messageReceiver.Start(_portfolioName, _configName);

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

         await PeriodicTimerExpired();
      }

      private async Task PeriodicTimerExpired()
      {
         try
         {
            while (await _periodicTimer.WaitForNextTickAsync(_cts.Token))
            {
               while (!_messageQueue.IsEmpty)
               {
                  if (_messageQueue.TryDequeue(out var result))
                  {
                     //Console.WriteLine($"Sending a message of type {result.MessageType.ToString()}");
                     await _hub.Clients.All.RealTimeUpdate(result);
                  }
               }
            }
         }
         catch (Exception e)
         {
            //Handle the exception but don't propagate it
            _logger.LogError(e, "Error in  periodic timer {Error}", e.Message);
         }
      }

      private void OnStrategyAlivePing(StrategyInstanceConnectionStatus status)
      {
         if (status.InstanceName == _configName)
         {
            var qData = new MessageQueueData
            {
               MessageType = QueueMsgTypes.STRATEGY_ALIVE_PING,
               Venue = status.InstanceName,
               Data = JsonSerializer.Serialize(status)
            };
            _messageQueue.Enqueue(qData);
         }
      }

      private void OnOpenOrdersResponse(string venue, List<OwnOrderChange> openOrders)
      {
         if (openOrders.Count == 0) return;
         string[] venues = venue.Split("_");
         var qData = new MessageQueueData
         {
            MessageType = QueueMsgTypes.OPEN_ORDERS_RESPONSE,
            Venue = venues[0],
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
            Venue = venues[0],
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
            Venue = venues[0],
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
            Venue = venues[0],
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
            Venue = venues[0],
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
            Venue = venues[0],
            Data = JsonSerializer.Serialize(order)
         };
         _messageQueue.Enqueue(qData);
      }

      private void OnFairValuePriceChange(string symbol, decimal fairValuePrice)
      {
  //       _logger.LogInformation("Got a fair value update");
         var data = new FairValueData()
         {
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
         _logger.LogInformation("Logging in to {Venue} for Account {Name} and instance {Instance}",
                                     exchange.Venue.VenueName, portfolioName, instanceName);
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

      private void OnConnectorIsDown(string obj)
      {
            
      }

      private void OnConnectorIsUp(string obj)
      {
            

      }

      private void GetOrderBooks(ExchangeDetailsDTO exchange)
      {
         _logger.LogInformation("Sending a request to {Venue} for Order book for {CoinPairs}",
                                   exchange.Venue.VenueName, exchange.Coinpairs);
         var pairs = exchange.Coinpairs;
         MessageBusCommand msgPublic = new MessageBusCommand()
         {
            AccountName = _portfolioName,
            InstanceName = InstanceName,
            CommandType = CommandTypesEnum.GET_ORDERBOOK,
            Data = JsonSerializer.Serialize(pairs)
         };
         var bytesRef = MessageBusCommand.ProtoSerialize(msgPublic);
         _messageBroker.PublishToSubject(exchange.Venue.VenueName, bytesRef);
      }


   }
}
