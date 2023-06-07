using AccountBalanceManager;
using BlazorLiquidity.Server.Hubs;
using BlazorLiquidity.Server.RealTime;
using BlazorLiquidity.Server.Receiver;
using ClientConnections;
using Common.Messages;
using Common.Models;
using ConnectorStatus;
using DataStore;
using FairValueProcessing;
using MessageBroker;
using Microsoft.AspNetCore.SignalR;
using OrderAndTradeProcessing;
using OrderBookProcessing;
using System.Collections.Concurrent;
using System.Text.Json;
using BlazorLiquidity.Server.FairValueProcessing;
using MultipleStrategyManager;

namespace BlazorLiquidity.Server.StrategyInstanceManager
{
   public interface IStrategyInstance
   {
      event Action<string, PrivateClientLoginStatus> OnPrivateLoginStatus;
      event Action<string, bool> OnConnectorPingStatus;
      event Action<StrategyInstanceConnectionStatus> OnStrategyAlivePing;

      Task Init(string spName, string instanceName, int configId, BlockingCollection<MessageBusReponse> msgQueue);
      void Run();
      IInventoryManager GetInventoryManager();
      IOrderAndTradeProcessing GetOrderAndTradeProcessingManager();

      // Task InitRealTimeUpdater(int configId);
   }

   public class StrategyInstance : IStrategyInstance
   {
      public event Action<string, PrivateClientLoginStatus> OnPrivateLoginStatus;
      public event Action<string, bool> OnConnectorPingStatus;
      public event Action<StrategyInstanceConnectionStatus> OnStrategyAlivePing;

      private readonly ILogger<StrategyInstance> _logger;
      
      private readonly IInventoryManager _inventoryManager;
      private readonly IOrderAndTradeProcessing _orderProcessingModule;
      private readonly IRealTimeUpdater _realTimeUpdater;
     
      private readonly IFairValuePricing _fairValuePricing;
      private readonly IPortfolioRepository _portfolioRepository;
     
      private readonly IMessageBroker _messageBroker;
    
      private readonly IConnectorStatusListener _connectorStatusListener;
      protected readonly IHubContext<PortfolioHub, IPortfolioHub> _hub;

      private string _instanceName { get; set; }
      private string _spName { get; set; }
      //private Queue<InstanceQueueData> _msgQueue { get; set; }
      private BlockingCollection<MessageBusReponse> _msgQueue { get; set; }
      private JsonSerializerOptions _jsonSerializerOptions { get; set; }
      private int _configId { get; set; }

      public StrategyInstance(ILoggerFactory loggerFactory,
                              IInventoryFactory inventoryFactory, 
                              IOrderAndTradeProcessingFactory orderAndTradeProcessingFactory,
                              IRealTimerUpdaterFactory realTimerUpdaterFactory,
                              IFairValuePricingUI fairValuePricing,
                              IPortfolioRepository repository,
                              IConnectorStatusListener connectorStatusListener,
                              IHubContext<PortfolioHub, IPortfolioHub> hub,
                              IMessageBroker messageBroker)
      {
         _logger = loggerFactory.CreateLogger<StrategyInstance>();        
         _inventoryManager = inventoryFactory.CreateInventoryInstance();
         _orderProcessingModule = orderAndTradeProcessingFactory.CreateOrderAndTradeProcessingInstance();
         _realTimeUpdater =
            realTimerUpdaterFactory.CreateRealTimeUpdaterInstance(_inventoryManager, _orderProcessingModule);

         _fairValuePricing = fairValuePricing;
         _portfolioRepository = repository;
  
         _hub = hub;
         _messageBroker = messageBroker;
         _connectorStatusListener = connectorStatusListener;
      }

      public async Task Init(string spName, string instanceName, int configId, BlockingCollection<MessageBusReponse> msgQueue)
      {
         _instanceName = instanceName;
         _spName = spName;
         _msgQueue = msgQueue;
         _configId = configId;
         _inventoryManager.InitConfig(spName, instanceName);
         await _realTimeUpdater.Init(configId);
        // await _orderProcessingModule.Init();
      }

      public void Run()
      {
         Task.Run(() => ProcessQueue());
      }

     private void ProcessQueue()
     {
         for(;;)
         {
            while(!_msgQueue.IsCompleted)
            {
               var msgData = _msgQueue.Take();
               processMessages(msgData);
            }
         }
     }

     private void processMessages(MessageBusReponse response)
     {
        _logger.LogInformation("mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm     In ProcessMessages with {ResponseType}", response.ResponseType.ToString());
        switch(response.ResponseType)
        {           
         case ResponseTypeEnums.GET_BALANCE_RESPONSE:
           var balances = JsonSerializer.Deserialize<ExchangeBalance[]>(response.Data, _jsonSerializerOptions);
           _inventoryManager.Update(response.FromVenue, balances);
            break;
         case ResponseTypeEnums.BALANCE_UPDATE:
            var balance = JsonSerializer.Deserialize<ExchangeBalance>(response.Data, _jsonSerializerOptions);
            _inventoryManager.Update(response.FromVenue, balance);
            break;
         case ResponseTypeEnums.PLACE_ORDER_RESPONSE:
            var orderIdData = JsonSerializer.Deserialize<OrderIdHolder>(response.Data);
            HandlePlaceOrderResponse(response.FromVenue, orderIdData);
            break;
         case ResponseTypeEnums.PLACE_ORDER_ERROR:
            var errorData = JsonSerializer.Deserialize<PlaceOrderErrorResponse>(response.Data);
            _orderProcessingModule.OrderPlacementUpdateError(response.FromVenue, errorData);
            break;
         case ResponseTypeEnums.NEW_ORDER:
         case ResponseTypeEnums.FILLED_ORDER:
         case ResponseTypeEnums.PARTIALLY_FILLED_ORDER:
         case ResponseTypeEnums.CANCELLED_ORDER:
         case ResponseTypeEnums.OWN_ORDER_UPDATE:
            var orderUpdate = JsonSerializer.Deserialize<OwnOrderChange>(response.Data, _jsonSerializerOptions);
            _orderProcessingModule.OrderUpdate(response.FromVenue, orderUpdate);
            break;
         
         case ResponseTypeEnums.GET_OPEN_ORDERS_RESPONSE:
            var openOrders = JsonSerializer.Deserialize<List<OrderQueryResponse>>(response.Data, _jsonSerializerOptions);
            _orderProcessingModule.OpenOrdersQueryResponse(response.FromVenue, openOrders);
            break;
         case ResponseTypeEnums.CANCEL_ORDERS_RESPONSE:
            var cancelledOrders = JsonSerializer.Deserialize<SingleCancelledOrderId>(response.Data, _jsonSerializerOptions);
            _orderProcessingModule.CancelOrderPlacementResponse(response.FromVenue, cancelledOrders);
            break;
         case ResponseTypeEnums.CANCEL_ORDER_ERROR:
           
            var cancelError =
               JsonSerializer.Deserialize<CancelOrderResponseError>(response.Data, _jsonSerializerOptions);
               _orderProcessingModule.CancelOrderError(response.FromVenue, cancelError);
            break;
         case ResponseTypeEnums.REFERENCE_DATA_RESPONSE:
            var tickerRefData = JsonSerializer.Deserialize<List<TickerReferenceData>>(response.Data, _jsonSerializerOptions);
            break;
         case ResponseTypeEnums.TRADE_UPDATE:
            var tradeData = JsonSerializer.Deserialize<TradeMsg>(response.Data, _jsonSerializerOptions);
            _orderProcessingModule.TradeUpdate(response.FromVenue, tradeData);
            break;
         case ResponseTypeEnums.PRIVATE_CLIENT_LOGIN_STATUS:
            var loginStatusData = JsonSerializer.Deserialize<PrivateClientLoginStatus>(response.Data, _jsonSerializerOptions);
            OnPrivateLoginStatus?.Invoke(response.FromVenue, loginStatusData);
            _orderProcessingModule.OnPrivateLoginStatus(response.FromVenue, loginStatusData);
            break;
         case ResponseTypeEnums.CONNECTOR_PING:
            OnConnectorPingStatus?.Invoke(response.FromVenue, true);
            break;
         
         case ResponseTypeEnums.STRATEGY_ALIVE_STATUS:
            var startegyAliveMsg = JsonSerializer.Deserialize<StrategyInstanceConnectionStatus>(response.Data, _jsonSerializerOptions);
            _realTimeUpdater.OnStrategyAlivePing(startegyAliveMsg);
               break;
         case ResponseTypeEnums.STRATEGY_CONTROL_RESPONSE:
            var strategyResponseMsg =
               JsonSerializer.Deserialize<StrategyControlResponse>(response.Data, _jsonSerializerOptions);
            _realTimeUpdater.OnStrategyControlResponse(strategyResponseMsg);
            break;

        }
     }

     private void HandlePlaceOrderResponse(string venue, OrderIdHolder orderData)
     {
        _orderProcessingModule.OrderPlacementUpdate(venue, orderData);
     }

     private void HandleGetBalanceResponse(ExchangeBalance[] balances)
     {

     }

      public IInventoryManager GetInventoryManager()
      {
         return _inventoryManager;
      }

      public IOrderAndTradeProcessing GetOrderAndTradeProcessingManager()
      {
         return _orderProcessingModule;
      }
   }
}
