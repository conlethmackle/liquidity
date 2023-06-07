using System;
using System.Collections.Generic;
using System.Linq;
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
using Bitfinex;
using Bitfinex.Config;
using Bitfinex.Net.Clients;
using Bitfinex.Net.Enums;
using Bitfinex.Net.Objects;
using Bitfinex.RestApi;
using Bitfinex.Net.SymbolOrderBooks;

using ConnectorCommon;
using Bitfinex.Net.Objects.Models;
using Constants = Common.Constants;
using CryptoExchange.Net.Interfaces;

namespace Bitfinex.Clients
{
   public class PublicClient : IPublicClient
   {

      private readonly IBitfinexRestApiClient _restApiClient;
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

      private BitfinexSocketClient _socketClient { get; set; }
      private Dictionary<string, OrderBook> _orderbook { get; set; } = new Dictionary<string, OrderBook>();
      private PrivateConnectionConfig _config { get; set; }

      public PublicClient(ILoggerFactory loggerFactory,
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
         _clientStatus = clientStatus;
         _portfolioRepository = repository;
         _websocketEndpoint = config.Value.PublicWSEndpoint;
         // TODO - ENCRYPT !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
         _config = config.Value;
         _reconnectInterval = config.Value.ReconnectIntervalMilliSecs;
         _orderbookUpdateInterval = config.Value.OrderbookUpdateInterval;
         _genericSymbols = symbolData.Value.CoinPairs.ToList();
         _clientStatus.SetExchange(Common.Constants.BITFINEX);
      }

      public async Task Init()
      {
         try
         {
            var symbols = _restApiClient.GetSymbols();
            if (symbols != null)
            {
               _clientStatus.UpdatePublicRestApiStatus(true, Constants.BITFINEX,"Binance Rest Api is working");
            }
            else
            {
               _clientStatus.UpdatePublicRestApiStatus(false, Constants.BITFINEX, "Binance Rest Api is not working");
            }
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
            }
            _socketClient = new BitfinexSocketClient(new BitfinexSocketClientOptions()
            {
               LogLevel = LogLevel.Debug,
              
               SpotStreamsOptions = new SocketApiClientOptions()
               {
                  BaseAddress = _websocketEndpoint,
                  AutoReconnect = true,
                  ReconnectInterval = TimeSpan.FromMilliseconds(_reconnectInterval)
               },
            });

            _exchangeSymbols.ForEach(async s =>
            {
               var book = new BitfinexSymbolOrderBook(s);

               var orderBook = new OrderBook();
               if (!_orderbook.ContainsKey(s))
               {
                  _orderbook.Add(s, orderBook);
               }
               else
               {
                  _orderbook[s] = orderBook;
               }

               var startResult = await book.StartAsync();
               var books = book.Book;
               foreach (var bid in books.bids)
               {
                  orderBook.Bid.Add(new Level(bid.Price, bid.Quantity));
               }

               foreach (var ask in books.asks)
               {
                  orderBook.Ask.Add(new Level(ask.Price, ask.Quantity));
               }


               if (startResult.Success)
               {
                  book.OnStatusChange += (oldStatus, newStatus) =>
                  {
                     OnOrderBookStatusChange(oldStatus, newStatus, s);
                  };

                  book.OnBestOffersChanged += (bestOffer) =>
                  {
                     OnBestOffersChanged(bestOffer, s);
                  };

                  book.OnOrderBookUpdate += (bidsAsks) =>
                  {
                     OnOrderBookUpdate(bidsAsks, s);
                  };
               }


               await _socketClient.SpotStreams.SubscribeToTradeUpdatesAsync(s, (trades) =>
               {
                  OnTradeUpdate(s, trades);
               });
               _clientStatus.UpdatePublicWebSocketStatus(true, Constants.BITFINEX, "All Good With Bitfinex Websocket");
            });


         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error connecting to Bitfinex Websocket {Error}", e.Message);
            _clientStatus.UpdatePublicWebSocketStatus(false, Constants.BITFINEX, $"Error connecting to Bitfinex Websocket {e.Message}");
            throw;
         }
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

      public void GetOrderBook(string instanceName, string genericSymbol)
      {
         try
         {
            var symbol = _genericToExchangeLookup[genericSymbol];
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.ORDERBOOK_SNAPSHOT,
               FromVenue = Common.Constants.BITFINEX,
               AccountName = instanceName,
            };
            lock (_orderBookLock)
            {
               var topic = genericSymbol + "." + Common.Constants.BITFINEX + Common.Constants.ORDERBOOK_TOPIC;
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
      public async Task GetLatestTrades(string instanceName, string symbol)
      {
         if (_genericToExchangeLookup.ContainsKey(symbol))
         {
            _logger.LogInformation("Received a GET LATEST TRADE REQUEST");
            var exchangeSymbol = _genericToExchangeLookup[symbol];
            var trades = await _restApiClient.GetTradeHistory(exchangeSymbol, 1);
            if (trades != null)
            {
               _clientStatus.UpdatePublicRestApiStatus(true, Constants.BITFINEX, "Bitfinex Rest Api is working");
               OnTradeUpdate(symbol, trades);
            }
            else
            {
               _clientStatus.UpdatePublicRestApiStatus(false, Constants.BITFINEX,"Bitfinex Rest Api is not working");
            }
         }
      }

      private void OnTradeUpdate(string symbol, IEnumerable<BitfinexTradeSimple> tradeData)
      {
         try
         {
            _logger.LogInformation("*****************************************In OnTradeUpdate********************************************");
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
               Symbol = symbol,
               Price = lastBitfinexTrade.Price,
               Quantity = lastBitfinexTrade.Quantity,
               TradeTime = lastBitfinexTrade.Timestamp
            };

            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.LAST_TRADED_PRICE,
               FromVenue = Common.Constants.BITFINEX,
               AccountName = "",
               Data = JsonSerializer.Serialize(lastTrade)
            };
            _logger.LogInformation("*****************************************Sending a LAST_TRADED_PRICE Message********************************************");
            var routingKey = symbol + "." + Constants.BITFINEX + Common.Constants.LAST_TRADED_PRICE_TOPIC;
            PublishHelper.PublishToTopic(routingKey, response, _messageBroker);
         }
         catch (Exception e)
         {
            _logger.LogInformation(e, "Error in OnTradeUpdate - {Error}", e.Message);
            
         }
         
      }

      public Task GetReferenceData()
      {
         throw new NotImplementedException();
      }

      private void OnTradeUpdate(string symbol, DataEvent<IEnumerable<BitfinexTradeSimple>> tradeData)
      {
         
         var trades = tradeData.Data.ToList();
         //_logger.LogInformation("OnTradeUpdate {Symbol} price {Price}", symbol, trades[0].Price);
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
            TradeTime = lastBitfinexTrade.Timestamp
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

      private void OnOrderBookUpdate((IEnumerable<ISymbolOrderBookEntry> Bids, IEnumerable<ISymbolOrderBookEntry> Asks) bidsAsks, string symbol)
      {
         var asks = bidsAsks.Asks.ToList();
         var bids = bidsAsks.Bids.ToList();
         var genericSymbol = _exchangeToGenericLookup[symbol];

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
            FromVenue = Common.Constants.BITFINEX,
            AccountName = "",
            Data = JsonSerializer.Serialize(externalOrderbookChanges)
         };
         var routingKey = genericSymbol + "." + Constants.BITFINEX + Common.Constants.ORDERBOOK_TOPIC;
         PublishHelper.PublishToTopic(routingKey, response, _messageBroker);

         UpdateOrderBook(asks, orderbook.Ask, false);
         UpdateOrderBook(bids, orderbook.Bid, true);
   //      Console.WriteLine(
    //        $"Order book changed for Pair {symbol}: {bidsAsks.Asks.Count()} asks, {bidsAsks.Bids.Count()} bids");
      }

      private void OnBestOffersChanged((ISymbolOrderBookEntry BestBid, ISymbolOrderBookEntry BestAsk) bestBidAsk, string symbol)
      {
         
      }

      private void OnOrderBookStatusChange(OrderBookStatus oldBookStatus, OrderBookStatus newBookStatus, string symbol)
      {
         _logger.LogInformation("OrderBook Status Change from {OldStatus} to {NewStatus}", oldBookStatus, newBookStatus);
      }

      private void ConnectionClosed()
      {
         _logger.LogWarning("Bitfinex public Websocket connection closed at {Time} time interval ", DateTime.UtcNow);
         _clientStatus.UpdatePublicWebSocketStatus(false, Constants.BITFINEX,
            $"Bitfinex Websocket closed at {DateTime.UtcNow}");
      }

      private void ConnectionRestored(TimeSpan obj)
      {
         _logger.LogInformation("Bitfinex public Websocket connection restored {Time} time interval {Span} millisecs", DateTime.Now, obj.TotalMilliseconds);
         _clientStatus.UpdatePublicWebSocketStatus( true, Constants.BITFINEX,
            $"Bitfinex Websocket up at {DateTime.UtcNow}");
      }

      private void ConnectionDown()
      {
         _logger.LogWarning("Bitfinex public Websocket connection down at {Time} time interval", DateTime.UtcNow);
         _clientStatus.UpdatePublicWebSocketStatus(false, Constants.BITFINEX,
            $"Bitfinex Websocket down at {DateTime.UtcNow}");
      }

      private void OnOrderbookUpdate((IEnumerable<ISymbolOrderBookEntry> Bids, IEnumerable<ISymbolOrderBookEntry> Asks) obj)
      {
         var symbol = "";
         var genericSymbol = "";
         var asks = obj.Asks.ToList();
         var bids = obj.Bids.ToList();
        // var oo = o.Data;
      //   var symbol = oo.Symbol;
       //  var genericSymbol = _exchangeToGenericLookup[symbol];
       //  var asks = oo.Asks.ToList();
       //  var bids = oo.Bids.ToList();
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
            FromVenue = Common.Constants.BITFINEX,
            AccountName = "",
            Data = JsonSerializer.Serialize(externalOrderbookChanges)
         };
         var routingKey = genericSymbol + "." +Constants.BITFINEX + Common.Constants.ORDERBOOK_TOPIC;
         PublishHelper.PublishToTopic(routingKey, response, _messageBroker);

         UpdateOrderBook(asks, orderbook.Ask, false);
         UpdateOrderBook(bids, orderbook.Bid, true);
      }

      private void GetOrderBookChanges(List<ISymbolOrderBookEntry> bidOrAsks, List<Level> orderbookSide, OrderBookSide externalOrderbookChanges)
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

      private void UpdateOrderBook(List<ISymbolOrderBookEntry> bidOrAsks, List<Level> sideOfBook, bool isBuy)
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
   }
}
