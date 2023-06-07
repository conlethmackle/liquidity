using AccountBalanceManager;
using BlazorLiquidity.Server.ErrorHandling;
using BlazorLiquidity.Server.OrdersAndTrades;
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
using BlazorLiquidity.Server.StrategyInstanceManager;
using StrategyMessageListener;

namespace BlazorLiquidity.Server.Receiver
{
   public class MultiStrategyMessageReceiver : IMessageReceiver
   {
      public event Action<string, PrivateClientLoginStatus> OnPrivateLoginStatus;
      public event Action<string, bool> OnConnectorPingStatus;
      public event Action<StrategyInstanceConnectionStatus> OnStrategyAlivePing;
      public event Action OnStartOfDay;
      public event Action<string, bool, string> OnConnectorPublicStatus;
      public event Action<string, bool, string> OnConnectorPrivateStatus;

      private readonly ILogger<MultiStrategyMessageReceiver> _logger;
      private readonly IMessageBroker _messageBroker;
      private readonly IPortfolioRepository _portfolioRepository;
      private readonly IOrderBookProcessor _orderBookProcessing;
      private readonly ILastTradedPriceHandler _lastTradedPriceHandler;
      private readonly Dictionary<string, bool> _startStatus = new Dictionary<string, bool>();
      private JsonSerializerOptions _jsonSerializerOptions { get; set; }
      public IStrategyInstanceManager _strategyInstanceManager { get; set; }

      public MultiStrategyMessageReceiver(ILoggerFactory loggerFactory,
         IMessageBroker messageBroker,
         IStrategyInstanceManager strategyManager,
         IPortfolioRepository repository,
         IOrderBookProcessor orderBookProcessing,
         ILastTradedPriceHandler lastTradedPriceHandler)
      {
         _logger = loggerFactory.CreateLogger<MultiStrategyMessageReceiver>();
         _jsonSerializerOptions = new JsonSerializerOptions()
         {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
         };
         _messageBroker = messageBroker;
         _strategyInstanceManager = strategyManager;
         _portfolioRepository = repository;
         _orderBookProcessing = orderBookProcessing;
         _lastTradedPriceHandler = lastTradedPriceHandler;
         _messageBroker.SubscribeToTopicSubject(Constants.SCHEDULE_UPDATE_TOPIC, ProcessOrderBookTopic);

      }

      public string GetOrderIdToCancel()
      {
         throw new NotImplementedException();
      }

      public Task Start()
      {
         throw new NotImplementedException();
      }

      public async Task Start(string spName, string configName, int configId)
      {
        // _strategyInstanceManager.CreateStrategyInstance(spName, configName, configId);
         if (!_startStatus.ContainsKey(configName))
         {
            _startStatus [configName] = true;
         }
         else
         {
            return;
         }
         //DashboardServer.VetFairValueConfig.orderbooks
         _messageBroker.SubscribeToTopicSubject("DashboardServer" +  Constants.ORDERBOOK_TOPIC, ProcessOrderBookTopic);
         _messageBroker.SubscribeToTopicSubject(spName + "." + configName +  Constants.BALANCES_TOPIC, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject(spName + "." + configName +  Constants.STATUS_TOPIC, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject(spName + "." + configName +  Constants.ORDERS_TOPIC, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject(spName + "." + configName +  Constants.TRADES_TOPIC, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject(spName + "." + configName +  Constants.STRATEGY_CONTROL_RESPONSE, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject(spName + "." + configName +  Constants.STRATEGY_ALIVE_TOPIC, ProcessCommands);
         _messageBroker.SubscribeToTopicSubject(Constants.CONNECTOR_PUBLIC_CONNECTION_TOPIC, ProcessOrderBookTopic);

         var configEntry = await _portfolioRepository.GetStrategyExchangeConfigEntry(configName);
         foreach (var exchange in configEntry.ExchangeDetails)
         {
            var coinPairs = exchange.CoinPairs.Split(",");

            foreach (var symbol in coinPairs)
            {
               var bindingKey = symbol + "." + exchange.Venue.VenueName + Constants.ORDERBOOK_TOPIC;
               _messageBroker.SubscribeToTopicSubject(bindingKey, ProcessOrderBookTopic);

               var bindingKeyLastTrades = symbol + "." + exchange.Venue.VenueName + Constants.LAST_TRADED_PRICE_TOPIC;
               _messageBroker.SubscribeToTopicSubject(bindingKeyLastTrades, ProcessOrderBookTopic);

            }
            var bindingKeyForConnectorStatus = Constants.CONNECTOR_ALIVE_TOPIC + "." + exchange.Venue.VenueName;
            _messageBroker.SubscribeToTopicSubject(bindingKeyForConnectorStatus, ProcessOrderBookTopic);
         }
      }

      private void ProcessCommands(string subject, byte[] data)
      {
         var configName = GetInstance(subject);
         if (configName == null)
         {
            throw new ArgumentException("");
         }

         using (var stream = new MemoryStream(data))
         {
            var response = Serializer.Deserialize<MessageBusReponse>(stream);
         //   _logger.LogInformation("**************** Received a response of type {Type}", response.ResponseType.ToString());
            if (response.ResponseType == ResponseTypeEnums.PRIVATE_CLIENT_LOGIN_STATUS)
            {
               var loginReply = JsonSerializer.Deserialize<PrivateClientLoginStatus>(response.Data, _jsonSerializerOptions);
               OnPrivateLoginStatus?.Invoke(response.FromVenue, loginReply);
            }
            _strategyInstanceManager.ForwardMessage(response, configName);
         }
      }

      private void ProcessOrderBookTopic(string subject, byte[] data)
      {
         using (var stream = new MemoryStream(data))
         {
            var response = Serializer.Deserialize<MessageBusReponse>(stream);
            if (response != null)
            {
               var venue = response.FromVenue;
               switch (response.ResponseType)
               {
                  case ResponseTypeEnums.ORDERBOOK_UPDATE:
                     var orderbookUpdate =
                        JsonSerializer.Deserialize<OrderBookChanged>(response.Data, _jsonSerializerOptions);
                     HandleOrderbookUpdate(venue, orderbookUpdate);
                     break;
                  case ResponseTypeEnums.ORDERBOOK_SNAPSHOT:
                     //           _logger.LogInformation("Received OrderBook Snapshot");
                     var snapshot =
                        JsonSerializer.Deserialize<OrderBookSnapshot>(response.Data, _jsonSerializerOptions);
                     HandleOrderBookSnapshot(venue, snapshot);
                     break;
                  case ResponseTypeEnums.LAST_TRADED_PRICE:
                     //       _logger.LogInformation("Received latest Trade Price");
                     var lastTraded = JsonSerializer.Deserialize<LatestTrade>(response.Data, _jsonSerializerOptions);
                     _lastTradedPriceHandler.UpdateLastTraded(response.FromVenue, lastTraded);
                     break;
                  case ResponseTypeEnums.CONNECTOR_PING:
                     OnConnectorPingStatus?.Invoke(response.FromVenue, true);
                     break;
                  case ResponseTypeEnums.CONNECTOR_STATUS_UPDATE:
                  //   _logger.LogInformation("Received a CONNECTOR_STATUS_UPDATE message");
                     var message =
                        JsonSerializer.Deserialize<ConnectorStatusMsg>(response.Data, _jsonSerializerOptions);
                     if (message != null)
                     {
                        if (!response.IsPrivate)
                           OnConnectorPublicStatus?.Invoke(response.FromVenue, message.Public.IsConnected,
                              message.Public.ErrorMsg);
                        else
                           OnConnectorPrivateStatus?.Invoke(response.FromVenue, message.Private.IsConnected,
                              message.Private.ErrorMsg);
                     }
                     else
                        _logger.LogError("Message of type ResponseTypeEnums.CONNECTOR_STATUS_UPDATE has null content");

                     break;
                  case ResponseTypeEnums.START_OF_DAY:
                     OnStartOfDay?.Invoke();
                     break;
               }
            }
         }
      }

      private void HandleOrderBookSnapshot(string venue, OrderBookSnapshot snapshot)
      {
         _orderBookProcessing.SnapshotOrderBook(venue, snapshot.Symbol, snapshot);
      }

      private void HandleOrderbookUpdate(string venue, OrderBookChanged orderbookUpdate)
      {
         //_logger.LogInformation("HandleOrderbookUpdate for {Venue}", venue);
         _orderBookProcessing.UpdateOrderBook(venue, orderbookUpdate.Symbol, orderbookUpdate);
      }

      private string GetInstance(string subject)
      {
         if (string.IsNullOrEmpty(subject))
         {
            _logger.LogError("GetInstance RabbitMQ subject  is empty", subject);
            return null;
         }

         var parts = subject.Split(".");
         if (parts.Length > 1)
         {
            return parts[1];
         }
         _logger.LogError("RabbitMQ subject {Subject} is empty", subject);
         return null;
      }

      public Task Start(string spName, string configName)
      {
         throw new NotImplementedException();
      }
   }
}
