using Binance.Config;
using Binance.Extensions;
using Binance.Net.Clients;
using Binance.Net.Interfaces;
using Binance.Net.Objects;
using Binance.Net.Objects.Models;
using Binance.Net.Objects.Models.Spot.Socket;
using Binance.RestApi;
using Common;
using Common.Messages;
using Common.Models;
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
using Binance;
using Binance.Net.Objects.Models.Spot;
using ConnectorCommon;
using Constants = Common.Constants;

namespace Binance.Clients
{
  

   public class PublicClient : IPublicClient
   {
      private readonly IBinanceRestApiClient _restApiClient;
      private readonly ILogger<PrivateClient> _logger;
      private readonly IMessageBroker _messageBroker;
      private readonly string _websocketEndpoint;
      private readonly string _apiKey;
      private readonly string _secret;
      private readonly UInt64 _reconnectInterval;
      private readonly UInt64 _orderbookUpdateInterval;
      private readonly List<string> _symbols;
      private object _orderBookLock = new object();
      private readonly IConnectorClientStatus _clientStatus;
      private readonly IPortfolioRepository _portfolioRepository;

      private readonly List<string> _genericSymbols = new List<string>();
      private List<string> _exchangeSymbols = new List<string>();
      private Dictionary<string, string> _exchangeToGenericLookup = new Dictionary<string, string>();
      private Dictionary<string, string> _genericToExchangeLookup = new Dictionary<string, string>();

      private BinanceSocketClient _socketClient { get; set; }
      private Dictionary<string, OrderBook> _orderbook { get; set; } = new Dictionary<string, OrderBook>();

      public PublicClient(ILoggerFactory loggerFactory,
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
         _clientStatus = clientStatus;
         _portfolioRepository = repository;
         _websocketEndpoint = config.Value.WebSocketEndpoint;
         // TODO - ENCRYPT !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
         _apiKey = config.Value.ApiKey;
         _secret = config.Value.SecretKey;
         _reconnectInterval = config.Value.ReconnectIntervalMilliSecs;
         _orderbookUpdateInterval = config.Value.OrderbookUpdateInterval;
         _genericSymbols = symbolData.Value.CoinPairs.ToList();
         _clientStatus.SetExchange(Common.Constants.BINANCE);
      }

      public async Task DisconnectPublic()
      {
         try
         {
            // TODO - check that this actually does a disconnect
            await _socketClient.UnsubscribeAllAsync();
         }
         catch (Exception e)
         {
            // TODO - should we do  retry??
            _logger.LogError(e, "Error unsubscribing from  from all public streams {Error} {StackTrace}", e.Message, e.StackTrace);
            throw;
         }
      }

      public  void GetOrderBook(string instanceName, string genericSymbol)
      {
         try
         {
            var symbol = _genericToExchangeLookup[genericSymbol];
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.ORDERBOOK_SNAPSHOT,
               FromVenue = Common.Constants.BINANCE,
               AccountName = instanceName,
            };
            lock (_orderBookLock)
            {
               
               var topic = genericSymbol + "." + Common.Constants.BINANCE + Common.Constants.ORDERBOOK_TOPIC;
               _logger.LogInformation("Sending Orderbook snapshot for {Instance} symbol {Symbol} on topic {Topic}", instanceName, genericSymbol, topic);
               var book = _orderbook[symbol];
               var snapshot = new OrderBookSnapshot(genericSymbol, book);
               response.Data = JsonSerializer.Serialize(snapshot);
               PublishHelper.PublishToTopic(topic, response, _messageBroker);
              // _snapshotSent.Add(symbol, true);
            }
         }
         catch (Exception e)
         {
            _logger.LogError("Error cancelling Order {Error}", e.Message);
            throw;
         }
      }

      public Task GetReferenceData()
      {
         throw new NotImplementedException();
      }

      public async Task Init()
      {
         try
         {

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
            _socketClient = new BinanceSocketClient(new BinanceSocketClientOptions()
                  {
                     AutoReconnect = true,
                     ReconnectInterval = TimeSpan.FromMilliseconds(_reconnectInterval),
                     SpotStreamsOptions = new ApiClientOptions()
                     {
                        BaseAddress = _websocketEndpoint
                     },
                  });

            _exchangeSymbols.ForEach(async s =>
            {
               var res = await _restApiClient.GetOrderBook(s);
               if (res.Success)
               {
                  _clientStatus.UpdatePublicRestApiStatus(true, Constants.BINANCE,"Binance Rest Api is working");
                  var d = res.Value.Clone();
                  BuildInitialOrderbookUpdate(d);
               }
               else
               {
                  _clientStatus.UpdatePublicRestApiStatus(true, Constants.BINANCE, "Binance Rest Api is not working");
                  _logger.LogError("Error in getting Binance Orderbooks {symbol} with error {Error}", s, res.Error);
                  //throw new Exception(res.Error);
               }
            });
      
            var subscription = await _socketClient.SpotStreams.SubscribeToOrderBookUpdatesAsync(_exchangeSymbols, (int?)_orderbookUpdateInterval, onOrderbookUpdate);

            if (subscription == null)
            {
               _logger.LogCritical("*********************** FATAL ERROR - cannot subscribe to Binance Private Websocket ****************************");
               throw new Exception(
                  "*********************** FATAL ERROR - cannot subscribe to Binance public Websocket ****************************");
            }

            if (subscription.Success)
            {

               _exchangeSymbols.ForEach(e =>
               {
                  _socketClient.SpotStreams.SubscribeToTradeUpdatesAsync(e, onTradeUpdate);
               });

               subscription.Data.ConnectionLost += ConnectionDown;
               subscription.Data.ConnectionRestored += ConnectionRestored;
               _clientStatus.UpdatePublicWebSocketStatus(true, Constants.BINANCE,"Binance public websocket up");
            }
            else
            {
               var ret =subscription.GetResultOrError(out var errorData, out var error);
               var errorMsg = "";
               if (ret)
                  errorMsg = $"***********************  ERROR - cannot subscribe to Binance Public Websocket {error.Message}****************************";
               else
               {
                  errorMsg =
                     $"***********************  ERROR - cannot subscribe to Binance Public Websocket - Unknown Reason****************************";
               }
               _logger.LogCritical(errorMsg);
               _clientStatus.UpdatePublicWebSocketStatus(false, Constants.BINANCE,errorMsg);
               throw new Exception(errorMsg);

            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, e.Message);
            _clientStatus.UpdatePublicWebSocketStatus(false, Constants.BINANCE, e.Message);
            throw;
         }
      }

      private void onTradeUpdate(DataEvent<BinanceStreamTrade> tradeData)
      {
         var trades = tradeData.Data;
         //LAST_TRADED_PRICE_TOPIC
         var lastTrade = new LatestTrade
         {
            Symbol = _exchangeToGenericLookup[trades.Symbol],
            Price = trades.Price,
            Quantity = trades.Quantity,
            TradeTime = trades.TradeTime
         };

         var response = new MessageBusReponse()
         {
            ResponseType = ResponseTypeEnums.LAST_TRADED_PRICE,
            FromVenue = Common.Constants.BINANCE,
            AccountName = "",
            Data = JsonSerializer.Serialize(lastTrade)
         };
         var routingKey = _exchangeToGenericLookup[trades.Symbol] + "." + BinanceConnectorConstants.ConnectorName + Common.Constants.LAST_TRADED_PRICE_TOPIC;
         PublishHelper.PublishToTopic(routingKey, response, _messageBroker);
      }

      private void ConnectionRestored(TimeSpan span)
      {
         _logger.LogInformation("Binance Websocket connection restored {Time} time interval {Span} millisecs", DateTime.Now, span.TotalMilliseconds);
         _clientStatus.UpdatePublicWebSocketStatus(false, Constants.BINANCE, "Binance public websocket restored");
      }

      private void ConnectionDown()
      {
         _logger.LogError("************** Binance Websocket down {Time} **********", DateTime.Now);
         _clientStatus.UpdatePublicWebSocketStatus(false, Constants.BINANCE, "Binance public websocket down");
      }

      private void onOrderbookUpdate(DataEvent<IBinanceEventOrderBook> o)
      {         
         var oo = o.Data;
         var symbol = oo.Symbol;
         var genericSymbol = _exchangeToGenericLookup[symbol];
         var asks = oo.Asks.ToList();
         var bids = oo.Bids.ToList();
        // if (bids.Any())
   //      _logger.LogInformation("Number of bids received is {Size} {First} {Last}", bids.Count, bids[0].Price, bids[bids.Count-1].Price);
         OrderBook orderbook = null;
         if (_orderbook.ContainsKey(symbol))
         {
            orderbook = _orderbook[symbol];
         }
         else
         {
            orderbook = new OrderBook();
            _orderbook.Add(symbol, orderbook);
         }

         var externalOrderbookChanges = new OrderBookChanged();
         
         GetOrderBookChanges(bids, orderbook.Bid, externalOrderbookChanges.Data[0]);
         GetOrderBookChanges(asks, orderbook.Ask, externalOrderbookChanges.Data[1]);
         externalOrderbookChanges.Symbol = _exchangeToGenericLookup[symbol];
         var response = new MessageBusReponse()
         {
            ResponseType = ResponseTypeEnums.ORDERBOOK_UPDATE,
            FromVenue = Common.Constants.BINANCE,
            AccountName = "",            
            Data = JsonSerializer.Serialize(externalOrderbookChanges)
         };
         var routingKey = genericSymbol + "." + BinanceConnectorConstants.ConnectorName + Common.Constants.ORDERBOOK_TOPIC;
         PublishHelper.PublishToTopic(routingKey, response, _messageBroker);
         
         UpdateOrderBook(asks, orderbook.Ask, false);
         UpdateOrderBook(bids, orderbook.Bid, true);
      }

      private void BuildInitialOrderbookUpdate(OrderBookSnapshot snapshot)
      {
        // OrderBook orderbook = null;
         if (_orderbook.ContainsKey(snapshot.Symbol))
         {
            var orderbook = _orderbook[snapshot.Symbol];
            orderbook = snapshot.OrderBook; // Just write over it
         }
         else
         {
            _orderbook.Add(snapshot.Symbol, snapshot.OrderBook);           
         }
      }

      private void GetOrderBookChanges(List<BinanceOrderBookEntry> bidOrAsks, List<Level> orderbookSide, OrderBookSide externalOrderbookChanges)
      {
         foreach (var bidAsks in bidOrAsks)
         {
            
            // Check the price
            if (bidAsks.Price == 0)
            {
               continue;
            }
            else if (bidAsks.Quantity == 0) // Size is 0
            {
               var l = new Level(bidAsks.Price, 0.0m);
               var rem = orderbookSide.Where(x => x.Price == bidAsks.Price).FirstOrDefault();
               if (rem != null)
               {
                  orderbookSide.Remove(rem);
                  externalOrderbookChanges.Remove.Add(bidAsks.Price);
               }
            }
            else // Update/Replace or New
            {
               var anyLevel = orderbookSide.Where(p => p.Price == bidAsks.Price);
               if (anyLevel.Count() > 0)
               {
                  var loc = anyLevel.First();
                  loc.Quantity = bidAsks.Quantity;
                  var level = new LevelDetails(bidAsks.Price, bidAsks.Quantity);
                  externalOrderbookChanges.Update.Add(level);
               }
               else
               {
                  var l = new Level(bidAsks.Price, bidAsks.Quantity);
                  orderbookSide.Add(l);
                  var level = new LevelDetails(bidAsks.Price, bidAsks.Quantity);
                  externalOrderbookChanges.Add.Add(level);
               }
            }
         }
      }

      private void UpdateOrderBook(List<BinanceOrderBookEntry> bidOrAsks, List<Level> sideOfBook, bool isBuy)
      {
         foreach (var bidAsks in bidOrAsks)
         {
            if (bidAsks.Price == 0)
            {
               continue;
            }
            else if (bidAsks.Quantity == 0) // Size is 0
            {

               var l = new Level(bidAsks.Quantity, 0.0m);
               var rem = sideOfBook.Where(x => x.Price == bidAsks.Price).FirstOrDefault();
               sideOfBook.Remove(rem);
            }
            else // Update/Replace or New
            {
               var anyLevel = sideOfBook.Where(p => p.Price == bidAsks.Price);
               if (anyLevel.Count() > 0)
               {
                  var loc = anyLevel.First();
                  loc.Quantity = bidAsks.Quantity;
               }
               else
               {
                  var l = new Level(bidAsks.Price, bidAsks.Quantity);
                  sideOfBook.Add(l);
               }
            }
         }
         if (isBuy)
            sideOfBook = sideOfBook.OrderByDescending(p => p.Price).ToList();
         else
            sideOfBook = sideOfBook.OrderBy(p => p.Price).ToList();
      }

      public async Task GetLatestTrades(string instanceName, string symbol)
      {
         /*if (_genericToExchangeLookup.ContainsKey(symbol))
         {
            _logger.LogInformation("Received a GET LATEST TRADE REQUEST");
            var exchangeSymbol = _genericToExchangeLookup[symbol];
            var trades = await _restApiClient.GetTradeHistory(exchangeSymbol, 1);
            OnTradeUpdate(symbol, trades);
         } */
      }

      private void OnTradeUpdate(string symbol, IEnumerable<IBinanceRecentTrade> tradeData)
      {
       /*  try
         {
            _logger.LogInformation("*****************************************In OnTradeUpdate********************************************");
            var trades = tradeData.ToList();
            IBinanceRecentTrade lastBinanceTrade = null;
            if (trades.Any())
            {
               lastBinanceTrade = trades[0];
            }


            //LAST_TRADED_PRICE_TOPIC
            var lastTrade = new LatestTrade
            {
               Symbol = symbol,
               Price = lastBinanceTrade.Price,
               Quantity = lastBinanceTrade.BaseQuantity,
               TradeTime = lastBinanceTrade.TradeTime
            };

            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.LAST_TRADED_PRICE,
               FromVenue = Common.Constants.BITFINEX,
               AccountName = "",
               Data = JsonSerializer.Serialize(lastTrade)
            };
            _logger.LogInformation("*****************************************Sending a LAST_TRADED_PRICE Message********************************************");
            var routingKey = symbol + "." + Common.Constants.BITFINEX + Common.Constants.LAST_TRADED_PRICE_TOPIC;
            PublishHelper.PublishToTopic(routingKey, response, _messageBroker);
         }
         catch (Exception e)
         {
            _logger.LogInformation(e, "Error in OnTradeUpdate - {Error}", e.Message);

         } */

      }
   }
}
