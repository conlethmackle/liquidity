using AccountBalanceManager;
using Common;
using Common.Messages;
using Common.Models;
using DataStore;
using DynamicConfigHandling;
using LastTradedPriceProcessing;
using MessageBroker;
using Microsoft.Extensions.Logging;
using OrderAndTradeProcessing;
using OrderBookProcessing;
using ProtoBuf;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StrategyMessageListener
{
   public interface IMessageReceiver
   {
      event Action<string, PrivateClientLoginStatus> OnPrivateLoginStatus;
      event Action<string, bool> OnConnectorPingStatus;
      event Action<string, bool, string> OnConnectorPublicStatus;
      event Action<string, bool, string> OnConnectorPrivateStatus;
      event Action<StrategyInstanceConnectionStatus> OnStrategyAlivePing;
      event Action OnStartOfDay;
      Task Start();
      Task Start(string spName, string configName);
      Task Start(string spName, string configName, int configId);
      string GetOrderIdToCancel();
   }

   public class MessageReceiver : IMessageReceiver
   {
      public event Action<string, PrivateClientLoginStatus> OnPrivateLoginStatus;
      public event Action<string, bool> OnConnectorPingStatus;
      public event Action<string, bool, string> OnConnectorPublicStatus;
      public event Action<string, bool, string> OnConnectorPrivateStatus;
      public event Action<StrategyInstanceConnectionStatus> OnStrategyAlivePing;
      public event Action OnStartOfDay;

      // public event Action<>
      private readonly ILogger<MessageReceiver> _logger;
      private readonly IMessageBroker _messageBroker;
      private readonly IInventoryManager _inventoryManager;
      private JsonSerializerOptions _jsonSerializerOptions { get; set; }
      private readonly IOrderAndTradeProcessing _orderProcessingModule;
      private readonly IOrderBookProcessor _orderBookProcessing;
      private readonly IPortfolioRepository _portfolioRepository;
      private readonly ILastTradedPriceHandler _lastTradedPriceHandler;
      private readonly IDynamicConfigUpdater _dynamicConfigUpdater;
      private string _orderId { get; set; }
      protected readonly string _portfolioName;
      protected readonly string _configName;

      public MessageReceiver(ILoggerFactory loggerFactory,
                             IMessageBroker messageBroker,
                             IOrderAndTradeProcessing orderProcessingModule,
                             IOrderBookProcessor orderBookProcessing,
                             StrategyStartConfig startupConfig,
                             IPortfolioRepository repository,
                             IInventoryManager inventoryManager,
                             ILastTradedPriceHandler lastTradedPriceHandler,
                             IDynamicConfigUpdater dynamicConfigUpdater
                            )
      {
         _logger = loggerFactory.CreateLogger<MessageReceiver>();
         _messageBroker = messageBroker;
         _jsonSerializerOptions = new JsonSerializerOptions()
         {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
         };
         _orderProcessingModule = orderProcessingModule;
         _orderBookProcessing = orderBookProcessing;
         _portfolioRepository = repository;
         _inventoryManager = inventoryManager;
         _lastTradedPriceHandler = lastTradedPriceHandler;
         _dynamicConfigUpdater = dynamicConfigUpdater;
         _portfolioName = startupConfig.Account;
         _configName = startupConfig.ConfigName;        
      }
      public async Task Start()
      {
         _messageBroker.SubscribeToTopicSubject(_configName + Constants.ORDERBOOK_TOPIC, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject(_portfolioName + "." + _configName + Constants.BALANCES_TOPIC, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject(_portfolioName + "." + _configName + Constants.STATUS_TOPIC, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject(_portfolioName + "." + _configName + Constants.ORDERS_TOPIC, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject(_portfolioName + "." + _configName + Constants.TRADES_TOPIC, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject(Constants.CONFIG_UPDATE_TOPIC, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject(Constants.CONNECTOR_PUBLIC_CONNECTION_TOPIC, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject(Constants.SCHEDULE_UPDATE_TOPIC, ProcessCommands);

         var configEntry = await _portfolioRepository.GetStrategyExchangeConfigEntry(_configName);
         if (configEntry == null)
         {
            _logger.LogCritical("Fatal Error: No Strategy Instance {Instance}", _configName);
            throw new Exception($"Fatal Error: No Strategy Instance {_configName}");
         }
         foreach(var exchange in configEntry.ExchangeDetails)
         {
            var bindingKeyForConnectorStatus = Constants.CONNECTOR_ALIVE_TOPIC + "." + exchange.Venue.VenueName;
            _messageBroker.SubscribeToTopicSubject(bindingKeyForConnectorStatus, ProcessCommands);

            var pairs = exchange.CoinPairs.Split(",");
            foreach ( var symbol in pairs)
            {
               var bindingKey = symbol + "." + exchange.Venue.VenueName + Constants.ORDERBOOK_TOPIC;
               _messageBroker.SubscribeToTopicSubject(bindingKey, ProcessCommands);

               var bindingKeyLastTrades = symbol + "." + exchange.Venue.VenueName + Constants.LAST_TRADED_PRICE_TOPIC;
               _messageBroker.SubscribeToTopicSubject(bindingKeyLastTrades, ProcessCommands);
            }
         }
      }

      private void ProcessCommands(string subject, byte[] data)
      {
         using (var stream = new MemoryStream(data))
         {
            var response = Serializer.Deserialize<MessageBusReponse>(stream);
            var venue = response.FromVenue;
            //_logger.LogInformation("Received a message of type {MessageType}", response.ResponseType.ToString());
            switch (response.ResponseType)
            {
               case ResponseTypeEnums.GET_BALANCE_RESPONSE:
                  _logger.LogInformation("Received a message of type {MessageType}", response.ResponseType.ToString());
                  var balances = JsonSerializer.Deserialize<ExchangeBalance[]>(response.Data, _jsonSerializerOptions);
                  _inventoryManager.Update(response.FromVenue, balances);
                  break;
               case ResponseTypeEnums.BALANCE_UPDATE:
                  _logger.LogInformation("Received a message of type {MessageType}", response.ResponseType.ToString());
                  var balance = JsonSerializer.Deserialize<ExchangeBalance>(response.Data, _jsonSerializerOptions);
                  _inventoryManager.Update(response.FromVenue, balance);                
                  break;
               case ResponseTypeEnums.PLACE_ORDER_RESPONSE:
                  _logger.LogInformation("Received a message of type {MessageType}", response.ResponseType.ToString());
                  var orderIdData = JsonSerializer.Deserialize<OrderIdHolder>(response.Data);

                  HandlePlaceOrderResponse(response.FromVenue, orderIdData);
                  break;
               case ResponseTypeEnums.PLACE_ORDER_ERROR:
                  _logger.LogInformation("Received a message of type {MessageType}", response.ResponseType.ToString());
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
               case ResponseTypeEnums.ORDERBOOK_UPDATE:
                  var orderbookUpdate = JsonSerializer.Deserialize<OrderBookChanged>(response.Data, _jsonSerializerOptions);
                   HandleOrderbookUpdate(venue, orderbookUpdate);
                  break;
               case ResponseTypeEnums.ORDERBOOK_SNAPSHOT:
                  _logger.LogInformation("Received a message of type {MessageType}", response.ResponseType.ToString());
                  var snapshot = JsonSerializer.Deserialize<OrderBookSnapshot>(response.Data, _jsonSerializerOptions);
                   HandleOrderBookSnapshot(venue, snapshot);
                  break;
               case ResponseTypeEnums.GET_OPEN_ORDERS_RESPONSE:
                  _logger.LogInformation("Received a message of type {MessageType}", response.ResponseType.ToString());
                  var openOrders = JsonSerializer.Deserialize<List<OrderQueryResponse>>(response.Data, _jsonSerializerOptions);
                  _orderProcessingModule.OpenOrdersQueryResponse(response.FromVenue, openOrders);
                  break;
               case ResponseTypeEnums.OPEN_ORDERS_ERROR:
                  _logger.LogInformation("Received a message of type {MessageType}", response.ResponseType.ToString());
                  var errorRsp = JsonSerializer.Deserialize<OpenOrderErrorResponse>(response.Data, _jsonSerializerOptions);
                  _orderProcessingModule.OpenOrdersErrorResponse(response.FromVenue, errorRsp);
                  break;
               case ResponseTypeEnums.CANCEL_ORDERS_RESPONSE:
                 
                  var cancelledOrders = JsonSerializer.Deserialize<SingleCancelledOrderId>(response.Data, _jsonSerializerOptions);
                  _logger.LogInformation("Cancel Order Response {OrderId} and {ClientOid}", cancelledOrders.OrderId, cancelledOrders.ClientOrderId);
                 _orderProcessingModule.CancelOrderPlacementResponse(response.FromVenue, cancelledOrders);
                  break;
               case ResponseTypeEnums.CANCEL_ORDER_ERROR:
                  var cancelError =
                     JsonSerializer.Deserialize<CancelOrderResponseError>(response.Data, _jsonSerializerOptions);
                  var error = response.Data;
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
                  if (loginStatusData != null)
                  {
                     _logger.LogInformation("Received a login status message {Message} {Instance} {Account} {Status}", loginStatusData.Message,
                           loginStatusData.InstanceName, loginStatusData.AccountName, loginStatusData.IsLoggedIn);
                     OnPrivateLoginStatus?.Invoke(response.FromVenue, loginStatusData);
                     OnConnectorPrivateStatus?.Invoke(response.FromVenue, loginStatusData.IsLoggedIn, loginStatusData.Message);
                  }
                  break;
               case ResponseTypeEnums.CONNECTOR_PING:
                  OnConnectorPingStatus?.Invoke(response.FromVenue, true);
                  break;
               case ResponseTypeEnums.LAST_TRADED_PRICE:
                  var lastTraded = JsonSerializer.Deserialize<LatestTrade>(response.Data, _jsonSerializerOptions); 
                  _lastTradedPriceHandler.UpdateLastTraded(response.FromVenue, lastTraded);
                  break;
               case ResponseTypeEnums.CONFIG_UPDATE_STATUS:
                  var configData = JsonSerializer.Deserialize<ConfigChangeUpdate>(response.Data, _jsonSerializerOptions);
                  _dynamicConfigUpdater.UpdateConfig(configData);
                  break;
               case ResponseTypeEnums.CONNECTOR_STATUS_UPDATE:
                  _logger.LogInformation("Received a message of type {MessageType}", response.ResponseType.ToString());

                  var message = JsonSerializer.Deserialize<ConnectorStatusMsg>(response.Data, _jsonSerializerOptions);
                  if (message != null)
                  {
                     if (!response.IsPrivate)
                        OnConnectorPublicStatus?.Invoke(response.FromVenue, message.Public.IsConnected, message.Public.ErrorMsg);
                     else
                     {
                        _logger.LogInformation("");
                        OnConnectorPrivateStatus?.Invoke(response.FromVenue, message.Private.IsConnected,
                           message.Private.ErrorMsg);
                     }
                  }
                  else
                  {
                     _logger.LogError("Null message returned from CONNECTOR_STATUS_UPDATE");
                  }

                  break;
               case ResponseTypeEnums.GET_BALANCE_RESPONSE_ERROR:
                  var balanceError = JsonSerializer.Deserialize<PlaceOrderErrorResponse>(response.Data);
                  _logger.LogError("Unsuccessful balance request from {Venue} for instance {Instance} - Error - {Error} re-requesting", response.FromVenue, response.OriginatingSource, balanceError.ErrorMessage);
                  _inventoryManager.GetOpeningBalancesDirect(response.FromVenue, balanceError.Instance, balanceError.Account );
                  break;
               case ResponseTypeEnums.PRIVATE_CLIENT_ERROR:
                  var resp = JsonSerializer.Deserialize<PlaceOrderErrorResponse>(response.Data);
                  _logger.LogError("Eoor response received from {Venue} with message {Error}", response.FromVenue, resp.ErrorMessage);
                  break;
               case ResponseTypeEnums.START_OF_DAY:
                  OnStartOfDay?.Invoke();
                  break;
               
               default:
                  _logger.LogWarning("");
                     break;
            }
         }
      }

      private void HandleBalanceUpdate(ExchangeBalance balance)
      {

      }

      private void HandleOrderBookSnapshot(string venue, OrderBookSnapshot snapshot)
      {
          _orderBookProcessing.SnapshotOrderBook(venue, snapshot.Symbol, snapshot);
      }

      private void HandleOrderbookUpdate(string venue, OrderBookChanged orderbookUpdate)
      {
          _orderBookProcessing.UpdateOrderBook(venue, orderbookUpdate.Symbol, orderbookUpdate);
      }

      private void HandlePlaceOrderResponse(string venue, OrderIdHolder orderData)
      {
         _logger.LogInformation("HandlePlaceOrderResponse received order response");
         _orderProcessingModule.OrderPlacementUpdate(venue, orderData);
      }

      private void HandleGetBalanceResponse(ExchangeBalance[] balances)
      {

      }

      public string GetOrderIdToCancel()
      {
         return _orderId;
      }

      public Task Start(string spName, string configName)
      {
         throw new NotImplementedException();
      }

      public Task Start(string spName, string configName, int configId)
      {
         throw new NotImplementedException();
      }
   }

}
