
using AccountBalanceManager;
using SyncfusionLiquidity.Server.ErrorHandling;
using SyncfusionLiquidity.Server.OrdersAndTrades;
using Common;
using Common.Messages;
using Common.Models;
using DataStore;
using LastTradedPriceProcessing;
using MessageBroker;
using OrderAndTradeProcessing;
using OrderBookProcessing;
using ProtoBuf;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SyncfusionLiquidity.Server.Receiver
{
   public interface IPortFolioMessageReceiver
   {
      event Action<string, PrivateClientLoginStatus> OnPrivateLoginStatus;
      event Action<StrategyInstanceConnectionStatus> OnStrategyAlivePing;
      event Action<string, bool> OnConnectorPingStatus;
      Task Start(string spName, string configName);
   }
   public class MessageReceiver : IPortFolioMessageReceiver
   {
      public event Action<string, PrivateClientLoginStatus> OnPrivateLoginStatus;
      public event Action<string, bool> OnConnectorPingStatus;
      public event Action<StrategyInstanceConnectionStatus> OnStrategyAlivePing;
      private readonly ILogger<MessageReceiver> _logger;
      private readonly IMessageBroker _messageBroker;
      private readonly IInventoryManager _inventoryManager;
      private readonly IErrorHandler _errorHandler;
      private JsonSerializerOptions _jsonSerializerOptions { get; set; }
      private readonly IOrderAndTradeProcessing _orderProcessingModule;
      private readonly IOrderBookProcessor _orderBookProcessing;
      private readonly IPortfolioRepository _portfolioRepository;
      private readonly ILastTradedPriceHandler _lastTradedPriceHandler;
      private string _orderId { get; set; }
      protected  string _portfolioName;
      protected  string _configName;
      private bool _startCompleted = false;

      public MessageReceiver(ILoggerFactory loggerFactory,
                             IMessageBroker messageBroker,
                             IOrderAndTradeProcessing orderProcessingModule,
                             IOrderBookProcessor orderBookProcessing,
                             StrategyStartConfig startupConfig,
                             IPortfolioRepository repository,
                             IInventoryManager inventoryManager,
                             ILastTradedPriceHandler lastTradedPriceHandler,
                             IErrorHandler errorHandler
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
         _errorHandler = errorHandler;
        // //      _portfolioName = startupConfig.Account;
       //  _configName = "VetFairValueConfig";

      }
      public async Task Start(string spName, string configName)
      {
         if (!_startCompleted)
            _startCompleted = true;
         else
            return;
         _portfolioName = spName;
         _configName = configName;
         _messageBroker.SubscribeToTopicSubject("DashboardServer" + Constants.ORDERBOOK_TOPIC, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject(_portfolioName + "." + _configName + Constants.BALANCES_TOPIC, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject(_portfolioName + "." + _configName + Constants.STATUS_TOPIC, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject(_portfolioName + "." + _configName + Constants.ORDERS_TOPIC, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject(_portfolioName + "." + _configName + Constants.TRADES_TOPIC, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject( _configName + Constants.STRATEGY_CONTROL_RESPONSE, ProcessCommands);

         var configEntry = await _portfolioRepository.GetStrategyExchangeConfigEntry(configName);
         foreach (var exchange in configEntry.ExchangeDetails)
         {
            foreach (var symbol in exchange.Coinpairs)
            {
               var bindingKey = symbol + "." + exchange.Venue.VenueName + Constants.ORDERBOOK_TOPIC;
               _messageBroker.SubscribeToTopicSubject(bindingKey, ProcessCommands);

               var bindingKeyLastTrades = symbol + "." + exchange.Venue.VenueName + Constants.LAST_TRADED_PRICE_TOPIC;
               _messageBroker.SubscribeToTopicSubject(bindingKeyLastTrades, ProcessCommands);

               var bindingKeyForConnectorStatus = Constants.CONNECTOR_ALIVE_TOPIC + "." + exchange.Venue.VenueName;
               _messageBroker.SubscribeToTopicSubject(bindingKeyForConnectorStatus, ProcessCommands);
            }
         }
      }

      private void ProcessCommands(string subject, byte[] data)
      {
         using (var stream = new MemoryStream(data))
         {
            var response = Serializer.Deserialize<MessageBusReponse>(stream);
            var venue = response.FromVenue;
            switch (response.ResponseType)
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
               case ResponseTypeEnums.ORDERBOOK_UPDATE:
                  var orderbookUpdate = JsonSerializer.Deserialize<OrderBookChanged>(response.Data, _jsonSerializerOptions);
                   HandleOrderbookUpdate(venue, orderbookUpdate);
                  break;
               case ResponseTypeEnums.ORDERBOOK_SNAPSHOT:
                  var snapshot = JsonSerializer.Deserialize<OrderBookSnapshot>(response.Data, _jsonSerializerOptions);
                   HandleOrderBookSnapshot(venue, snapshot);
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
                  OnPrivateLoginStatus?.Invoke(response.FromVenue, loginStatusData);
                  _orderProcessingModule.OnPrivateLoginStatus(response.FromVenue, loginStatusData);
                  break;
               case ResponseTypeEnums.CONNECTOR_PING:
                  OnConnectorPingStatus?.Invoke(response.FromVenue, true);
                  break;
               case ResponseTypeEnums.LAST_TRADED_PRICE:
                  var lastTraded = JsonSerializer.Deserialize<LatestTrade>(response.Data, _jsonSerializerOptions);
                  _lastTradedPriceHandler.UpdateLastTraded(response.FromVenue, lastTraded);
                  break;
               case ResponseTypeEnums.STRATEGY_ALIVE_STATUS:
                  var startegyAliveMsg = JsonSerializer.Deserialize<StrategyInstanceConnectionStatus>(response.Data, _jsonSerializerOptions);
                  OnStrategyAlivePing?.Invoke(startegyAliveMsg);
                  break;
               case ResponseTypeEnums.GET_BALANCE_RESPONSE_ERROR:
                  break;

            }
         }
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
         _orderProcessingModule.OrderPlacementUpdate(venue, orderData);
      }

      private void HandleGetBalanceResponse(ExchangeBalance[] balances)
      {

      }

   }
}
