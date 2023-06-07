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
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Timers;

namespace KuCoin.Clients
{
   
   public class PublicClient : IPublicClient
   {     
      private readonly ILogger<PublicClient> _logger;
      private readonly IKuCoinWebSocket _webSocketHandler;
      private readonly IKuCoinRestApiClient _restClient;
      private readonly IMessageBroker _messageBroker;

      private  string _publicEndpoint { get; set; }
      private readonly string _url;
      private JsonSerializerOptions _jsonSerializerOptions { get; set; }
      private List<WebsocketServerDetails> _publicServers { get; set; }
      private WebsocketServerDetails _currentPublicServer { get; set; }
     
      private Timer _publicPingTimer { get; set; }
      private Timer _publicPongTimer { get; set; }
      private Timer _restartSocketTimer { get; set; }
      private string _token { get; set; }
      private string _connectId { get; set; }
      private string _connection { get; set; }

      private Dictionary<string, OrderBookState> _orderBooksState = new Dictionary<string, OrderBookState>();
      private Dictionary<string, bool> _snapshotSent = new Dictionary<string, bool>();
      private Dictionary<string, List<Level2Changes>> _cachedOrderBooks = new Dictionary<string, List<Level2Changes>>();
      private Dictionary<string, string> _orderBookSubscriptionAcks = new Dictionary<string, string>();
      private Dictionary<string, Int64> _snapshotSequenceNos = new Dictionary<string, long>();
      private Dictionary<string, OrderBook> _orderbook { get; set; } = new Dictionary<string, OrderBook>();
      private readonly string[] _genericSymbols;
      private List<string> _exchangeSymbols = new List<string>();
      private Dictionary<string, string> _exchangeToGenericLookup = new Dictionary<string, string>();
      private Dictionary<string, string> _genericToExchangeLookup = new Dictionary<string, string>();

      private object _orderBookLock = new object();
      private Queue<Tuple<string, string>> _pushOrderBook = new Queue<Tuple<string, string>>();
      private WebsocketConnectState _connectionState = WebsocketConnectState.NOT_CONNECTED;
      private readonly IConnectorClientStatus _clientStatus;
      private readonly IPortfolioRepository _portfolioRepository;

      public  PublicClient(ILoggerFactory loggerFactory, 
                          IOptions<KuCoinConnectionConfig> connectionConfig,
                          IOptions<SymbolDataConfig> symbolConfig,
                          IKuCoinWebSocket webSocketHandler, 
                          IKuCoinRestApiClient restClient, 
                          IMessageBroker messageBroker,
                          IConnectorClientStatus clientStatus,
                          IPortfolioRepository repository)
      {
         _logger = loggerFactory.CreateLogger<PublicClient>();
         _webSocketHandler = webSocketHandler;
         _restClient = restClient;
         _messageBroker = messageBroker;
         _clientStatus = clientStatus;
         _portfolioRepository = repository;

         var config = connectionConfig.Value;
         _url = config.Url;
         _publicEndpoint = config.PublicWebSocketEndpoint;
         _jsonSerializerOptions = new JsonSerializerOptions()
         {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
         };
         _genericSymbols = symbolConfig.Value.CoinPairs;
         _clientStatus.SetExchange(Common.Constants.KUCOIN);
      }

      public async Task Init()
      {
         try
         {
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
               _exchangeSymbols.Add(exchangeSymbol.ExchangeCoinpairName);
               _genericToExchangeLookup.Add(symbol, exchangeSymbol.ExchangeCoinpairName);
               _exchangeToGenericLookup.Add(exchangeSymbol.ExchangeCoinpairName, symbol);
            }
         
            await SetupPublicWebsocket();
            await _webSocketHandler.ConnectPublic();
            _webSocketHandler.RunPublic(ProcessPublicResponse);
         }
         catch (Exception e)
         {
            _logger.LogCritical("{Error}", e.Message);
            throw;
         }
      }

      public void GetOrderBook(string instanceName, string genericSymbol)
      {
         try
         {
            var symbol = _genericToExchangeLookup[genericSymbol];
            // What is the state of the orderbook
            if (_orderBooksState.ContainsKey(symbol))
            {
               var state = _orderBooksState[symbol];
               if (state == OrderBookState.REALTIME)
               {
                  if (_orderbook.ContainsKey(symbol))
                  {
                     PublishInitialOrderBook(instanceName, symbol);                    
                  }
               }
               else
               {
                  var t = new Tuple<string, string>(instanceName, symbol);
                  _pushOrderBook.Enqueue(t);
               }
            }              
         }
         catch (Exception e)
         {
            _logger.LogError("Error in Get Orderbook {Error}", e.Message);
            throw;
         }
      } 

      public async Task GetReferenceData()
      {
         try
         {
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.REFERENCE_DATA_RESPONSE,
               FromVenue = Constants.KUCOIN,
               AccountName = ""
            };
            var result = await _restClient.GetReferenceData();
            if (result.Success)
            {
               var kuCoinrefData = result.Value.ToList();
               var normalisedRefData = new List<TickerReferenceData>();
               kuCoinrefData.ForEach(b =>
                  {
                     normalisedRefData.Add(b.Clone());
                  }
               );
               // Send out on rabbitmq
               response.Data = JsonSerializer.Serialize(normalisedRefData);
            }
            else
            {
               response.Success = false;
               response.Data = result.Error;
            }
                       
            PublishHelper.Publish(Constants.TESTER, response, _messageBroker);
         }
         catch (Exception e)
         {
            _logger.LogError("Error in GetReferenceData {Error}", e.Message);
            throw;
         }
      }

      private async Task SetupPublicWebsocket()
      {
         var result = await _restClient.GetPublicWebsocketInfo();
         if (result.Success)
         {
            var websocketTokenAndServer = result.Value;
            _publicServers = websocketTokenAndServer.Servers;
            _currentPublicServer = websocketTokenAndServer.Servers[0];
            _publicEndpoint = websocketTokenAndServer.Servers[0].Endpoint;
            InitPublicTimers(_currentPublicServer);
            var pingInterval = websocketTokenAndServer.Servers[0].PingInterval;
            _token = websocketTokenAndServer.Token;
            _connectId = RandomString(16);
            _connection = _publicEndpoint + "?" + "token=" + _token + "&[connectId=" + _connectId + "]";
            _webSocketHandler.InitPublic(_publicEndpoint, _token, _connectId);
            //await _webSocketHandler.ConnectPublic();
         }
         else
         {
            _logger.LogError("******************* Error connecting to the public endpoint {Error} ******************* ", result.Error);
            throw new Exception($"******************* Error connecting to the public endpoint {result.Error} ************************* ");
         }        
      }

      private void InitPublicTimers(WebsocketServerDetails publicServer)
      {
         _publicPingTimer = new Timer();
         _publicPingTimer.Elapsed += PublicSendPingTimerCallback;
         _publicPingTimer.Interval = publicServer.PingInterval;
         _publicPingTimer.Enabled = true;

         _publicPongTimer = new Timer();
         _publicPongTimer.Elapsed += PublicPongTimerCallback;
         _publicPongTimer.Interval = publicServer.PingTimeout;
         _publicPongTimer.Enabled = true;

         _restartSocketTimer = new Timer();
         _restartSocketTimer.Elapsed += PublicRestartWebsocketTimer;
         _restartSocketTimer.Interval = 1000; // TODO
      }

      private void PrivateSendPingTimerCallback(object o, ElapsedEventArgs e)
      {
         _publicPingTimer.Enabled = false;
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
         _publicPongTimer.Enabled = true;
         if (_connectionState == WebsocketConnectState.CONNECTED)
            _publicPingTimer.Enabled = true;
      }

      private void PrivatePongTimerCallback(object o, ElapsedEventArgs e)
      {
         _publicPingTimer.Enabled = false;
         if (_webSocketHandler.IsPublicConnected())
         {
            // This indicates that the socket is down
            if (_connectionState == WebsocketConnectState.CONNECTED)
            {
               _logger.LogError("***************** KuCoin Pong timer for public has expired Websocket down");
               _clientStatus.UpdatePublicWebSocketStatus(false, "Pong Timer has fired - private websocket down");    //(_accountName, false, "Pong Timer has fired - private websocket down");
               _restartSocketTimer.Enabled = true;
               _connectionState = WebsocketConnectState.NOT_CONNECTED;

            }
         }
      }

      private async void PublicRestartWebsocketTimer(object sender, ElapsedEventArgs e)
      {
         // TODO - need to cycle round the list of available servers. Should have a restart module I think
         // that can do both private and public clients
         _restartSocketTimer.Enabled = false;
         _connectionState = WebsocketConnectState.CONNECTING;
         await _webSocketHandler.DisconnectPublic("Restarting the public websocket connection");
         await SetupPublicWebsocket();
         _restartSocketTimer.Enabled = true;
      }


      public async Task Start(Func<string, Task> privateAction, Func<string, Task> publicAction)
      {
         await Restart(privateAction, publicAction);
         //_websocketPart.Run(action);
      }

      public async Task Restart(Func<string, Task> privateAction, Func<string, Task> publicAction)
      {
         try
         {
            RestartPublic(publicAction);
         }
         catch (Exception e)
         {
            _logger.LogError($"Error connecting to websocket {e.Message}");
            throw;
         }
      }

      private async Task RestartPublic(Func<string, Task> publicAction)
      {
         try
         {
            await _webSocketHandler.ConnectPublic();
            _webSocketHandler.RunPublic(publicAction);
         }
         catch (Exception e)
         {
            _logger.LogError($"Error connecting to websocket {e.Message}");
            throw;
         }
      }
      private void PublicSendPingTimerCallback(object o, ElapsedEventArgs e)
      {
         _publicPingTimer.Enabled = false;
         if (_webSocketHandler.IsPublicConnected())
         {
            var pingData = new PingPongData()
            {
               Id = _connectId,
               Type = "ping"
            };
            var ping = JsonSerializer.Serialize(pingData);
            _webSocketHandler.SendPublic(ping);
         }
         _publicPongTimer.Enabled = true;
         if (_connectionState == WebsocketConnectState.CONNECTED)
            _publicPingTimer.Enabled = true;         
      }

      private void PublicPongTimerCallback(object o, ElapsedEventArgs e)
      {
         _publicPongTimer.Enabled = false;
         if (_webSocketHandler.IsPublicConnected())
         {
            // This indicates that the socket is down
            if (_connectionState == WebsocketConnectState.CONNECTED)
            {
               _logger.LogError("***************** KuCoin Pong timer for public has expired Websocket down");
               _clientStatus.UpdatePublicWebSocketStatus( false, "Pong Timer has fired - public websocket down");
               _restartSocketTimer.Enabled = true;
               _connectionState = WebsocketConnectState.NOT_CONNECTED;
            }
         }
      }

      public static string RandomString(int length)
      {
         var random = new Random();
         const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
         return new string(Enumerable.Repeat(chars, length)
             .Select(s => s[random.Next(s.Length)]).ToArray());
      }

      public async Task ProcessPublicResponse(string reply)
      {
         try
         {
            //   _logger.LogInformation(reply);
            //   Console.WriteLine(reply);
            if (reply.Equals("CONNECTION DOWN"))
            {

               await RestartPublic(ProcessPublicResponse);
            }
            var response = JsonSerializer.Deserialize<BasicResponseData>(reply);
            var type = response.Type;
            switch (type)
            {
               case "pong":
                  _publicPongTimer.Enabled = false;
                  _restartSocketTimer.Enabled = false;
                  if (_connectionState != WebsocketConnectState.CONNECTED)
                  {
                     _connectionState = WebsocketConnectState.CONNECTED;
                     _clientStatus.UpdatePublicWebSocketStatus(true, Constants.BINANCE);
                     _logger.LogInformation("Public Websocket connection is UP");
                  }
                  break;
               case "welcome":
                  // Successful login
                  await SubscribeForOrderBooks();
                  await SubscribeForSymbolTicker();
                  break;
               case "message":
                  // {"type":"message","topic":"/market/level2:KCS-USDT","subject":"trade.l2update","data":{"sequenceStart":1636696609333,"symbol":"KCS-USDT","changes":{"asks":[],"bids":[["20.484","0.04","1636696609333"]]},"sequenceEnd":1636696609333}}
                  await HandlePublicMessage(reply, response);
                  break;
               case "ack":
                  if (_orderBookSubscriptionAcks.ContainsKey(response.Id))
                  {
                     var symbol = _orderBookSubscriptionAcks[response.Id];
                     _orderBookSubscriptionAcks.Remove(response.Id);
                  }
                  break;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Public Side processing error when processing {Reply} {Error}", reply, e.Message);
            throw;
         }
      }

      private async Task SubscribeForOrderBooks()
      {
         foreach (var symbol in _exchangeSymbols)
         {
            Subscription sub = new Subscription()
            {
               Id = RandomString(16),
               Topic = $"/market/level2:{symbol}",
               Type = "subscribe",
               Response = true,
               PrivateChannel = false
            };
            _orderBookSubscriptionAcks.Add(sub.Id, symbol);
            var msg = JsonSerializer.Serialize(sub);
            await _webSocketHandler.SendPublic(msg);
         }
      }

      private async Task SubscribeForSymbolTicker()
      {
         foreach (var symbol in _exchangeSymbols)
         {
            Subscription sub = new Subscription()
            {
               Id = RandomString(16),
               Topic = $"/market/ticker:{symbol}",
               Type = "subscribe",
               Response = true,
               PrivateChannel = false
            };
            _orderBookSubscriptionAcks.Add(sub.Id, symbol);
            var msg = JsonSerializer.Serialize(sub);
            await _webSocketHandler.SendPublic(msg);
         }
      }

      private async Task HandlePublicMessage(string reply, BasicResponseData response)
      {
         var topicData = response.Topic.Split(":");
         if (topicData.Count() == 0)
         {
            _logger.LogError("Received incorrectly formed message/topic from exchange {Topic}", response.Topic);
            return; // Maybe do a throw??
         }

         var topic = topicData[0];
         var symbols = topicData[1];

         switch (topic)
         {
            case "/market/level2":
               if (response.Subject.Equals("trade.l2update"))
               {
                  await HandleL2Update(reply);
               }
               break;
            case "/market/ticker":
               HandleTickerUpdate(symbols, reply);
               break;
         }
      }

      private async Task HandleL2Update(string response)
      {
         var data = JsonSerializer.Deserialize<ResponseData<Level2Changes>>(response);
         var l2Data = data.Data;
         var state = GetOrderBookSnapshotState(l2Data.Symbol);
         switch (state)
         {
            case OrderBookState.INITIAL:
               await HandleInitialL2Update(l2Data);
               break;
            case OrderBookState.REALTIME:
               HandleRealTimeL2Update(l2Data);
               break;
            case OrderBookState.CACHING:
               HandleCachingL2Update(l2Data);
               break;
            case OrderBookState.REPLAYING_CACHE:
               HandleReplayingL2Update(l2Data);
               break;
            default:
               _logger.LogError("Unhandled OrderBook State {L2State}", state);
               break;
         }
      }

      private void HandleTickerUpdate(string symbol, string reply)
      {
         var data = JsonSerializer.Deserialize<ResponseData<TradeTicker>>(reply);
         var tickerData = data.Data;
         
         TimeSpan time = TimeSpan.FromMilliseconds(tickerData.Time);
         DateTime startdate = new DateTime(1970, 1, 1) + time;
         var lastTrade = new LatestTrade
         {
            Symbol = _exchangeToGenericLookup[symbol],
            Price = Decimal.Parse(tickerData.Price),
            Quantity = Decimal.Parse(tickerData.Quantity),
            TradeTime = new DateTime(1970, 1, 1) + time
         };

         var response = new MessageBusReponse()
         {
            ResponseType = ResponseTypeEnums.LAST_TRADED_PRICE,
            FromVenue = Common.Constants.BINANCE,
            AccountName = "",
            Data = JsonSerializer.Serialize(lastTrade)
         };
         var routingKey = _exchangeToGenericLookup[symbol] + "." + KucoinConnectorConstants.ConnectorName + Common.Constants.LAST_TRADED_PRICE_TOPIC;
         PublishHelper.PublishToTopic(routingKey, response, _messageBroker);

      }

      private async Task HandleInitialL2Update(Level2Changes l2Data)
      {
         SetOrderBookSnapshotState(l2Data.Symbol, OrderBookState.CACHING);
         HandleCachingL2Update(l2Data);
         await GetFullOrderBookSnapshot(l2Data.Symbol);
      }

      private void HandleReplayingL2Update(Level2Changes l2Data)
      {
         
      }

      private void HandleCachingL2Update(Level2Changes l2Data)
      {
        
         Console.WriteLine($"HandleCachingL2Update Sequence start {l2Data.SequenceStart} Sequence End {l2Data.SequenceEnd}");
         List<Level2Changes> _cachedList = null;
         if (!_cachedOrderBooks.ContainsKey(l2Data.Symbol))
         {
            _cachedList = new List<Level2Changes>();
         }
         else
            _cachedList = _cachedOrderBooks[l2Data.Symbol];
         _cachedList.Add(l2Data);
      }

      private void HandleRealTimeL2Update(Level2Changes l2Data)
      {
       //  Console.WriteLine("In HandleRealTimeL2Update");
         var symbol = l2Data.Symbol;
         var genericSymbol = _exchangeToGenericLookup[symbol];
         OrderBook orderbook = null;
         if (_orderbook.ContainsKey(symbol))
         {
            orderbook = _orderbook[symbol];
         }
         else
         {
            _logger.LogError("No Orderbook for {Symbol}", symbol);
            throw new Exception($"No Orderbook for {symbol}");
         }

         var externalOrderbookChanges = new OrderBookChanged();
        
         GetOrderBookChanges(l2Data.Changes.Bids, orderbook.Bid, externalOrderbookChanges.Data[0], true);
         GetOrderBookChanges(l2Data.Changes.Asks, orderbook.Ask, externalOrderbookChanges.Data[1], false);
         externalOrderbookChanges.Symbol = genericSymbol;

         if (_snapshotSent.ContainsKey(symbol))
         {
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.ORDERBOOK_UPDATE,
               FromVenue = Constants.KUCOIN,
               AccountName = "",
               Data = JsonSerializer.Serialize(externalOrderbookChanges)
            };
            var routingKey = genericSymbol +  "." + KucoinConnectorConstants.ConnectorName + Constants.ORDERBOOK_TOPIC;
            
            PublishHelper.PublishToTopic(routingKey, response, _messageBroker);
         }

         UpdateOrderBook(l2Data.Changes.Asks, orderbook.Ask, 0, false);
         UpdateOrderBook(l2Data.Changes.Bids, orderbook.Bid, 0, true);
      }

      private void GetOrderBookChanges(List<List<string>> bidOrAsks, List<Level> orderbookSide, OrderBookSide externalOrderbookChanges, bool isBuy)
      {
         foreach (var bidAsks in bidOrAsks)
         {
            // Check the price
            if (Decimal.Parse(bidAsks[0]) == 0)
            {
               continue;
            }
            else if (Decimal.Parse(bidAsks[1]) == 0) // Size is 0
            {
               var l = new Level(Decimal.Parse(bidAsks[0]), 0.0m);
               var rem = orderbookSide.Where(x => x.Price == Decimal.Parse(bidAsks[0])).FirstOrDefault();
               orderbookSide.Remove(rem);
               externalOrderbookChanges.Remove.Add(Decimal.Parse(bidAsks[0]));
            }
            else // Update/Replace or New
            {
               var anyLevel = orderbookSide.Where(p => p.Price == Decimal.Parse(bidAsks[0]));
               if (anyLevel.Count() > 0)
               {
                  var loc = anyLevel.First();
                  loc.Quantity = Decimal.Parse(bidAsks[1]);
                  var level = new LevelDetails(Decimal.Parse(bidAsks[0]), Decimal.Parse(bidAsks[1]));
                  externalOrderbookChanges.Update.Add(level);
               }
               else
               {
                  var l = new Level(Decimal.Parse(bidAsks[0]), Decimal.Parse(bidAsks[1]));
                  orderbookSide.Add(l);
                  var level = new LevelDetails(Decimal.Parse(bidAsks[0]), Decimal.Parse(bidAsks[1]));
                  externalOrderbookChanges.Add.Add(level);
               }
            }
         }
      }

      private OrderBookState GetOrderBookSnapshotState(string symbol)
      {
         if (!_orderBooksState.ContainsKey(symbol))
         {
            // Effectively no state - go straight to INITIAL
            var state = OrderBookState.INITIAL;
            _orderBooksState.Add(symbol, state);
            return state;
         }
         return _orderBooksState[symbol];
      }

      private void SetOrderBookSnapshotState(string symbol, OrderBookState state)
      {
         if (!_orderBooksState.ContainsKey(symbol))
         {
            _orderBooksState.Add(symbol, state);
         }
         _orderBooksState[symbol] = state;
      }

      private void UpdateOrderBook(List<List<string>> bidOrAsks, List<Level> sideOfBook, Int64 currentSequenceNumber, bool isBuy)
      {
         foreach (var bidAsks in bidOrAsks)
         {
            if (Int64.Parse(bidAsks[2]) > currentSequenceNumber)
            {
               // Check the price
               if (Decimal.Parse(bidAsks[0]) == 0)
               {
                  currentSequenceNumber++;
                  continue;
               }
               else if (Decimal.Parse(bidAsks[0]) == 0) // Size is 0
               {
                  currentSequenceNumber++;
                  var l = new Level(Decimal.Parse(bidAsks[1]), 0.0m);

                  var rem = sideOfBook.Where(x => x.Price == Decimal.Parse(bidAsks[0])).FirstOrDefault();
                  sideOfBook.Remove(rem);
               }
               else // Update/Replace or New
               {
                  var anyLevel = sideOfBook.Where(p => p.Price == Decimal.Parse(bidAsks[0]));
                  if (anyLevel.Count() > 0)
                  {
                     var loc = anyLevel.First();
                     loc.Quantity = Decimal.Parse(bidAsks[1]);
                  }
                  else
                  {
                     var l = new Level(Decimal.Parse(bidAsks[0]), Decimal.Parse(bidAsks[1]));
                     sideOfBook.Add(l);
                  }
               }
            }
         }
         if (isBuy)
            sideOfBook = sideOfBook.OrderByDescending(p => p.Price).ToList();
         else
            sideOfBook = sideOfBook.OrderBy(p => p.Price).ToList();
      }

      private async Task GetFullOrderBookSnapshot(string symbol)
      {
         var result = await _restClient.GetFullOrderBookSnapshot(symbol);
         if (result.Success)
         {
            var snapshot = result.Value;
            BuildOrderBook(symbol, snapshot);
         }
      }

      private void BuildOrderBook(string symbol, OrderBookSnapShotContainer snapshotContainer)
      {
         var snapshot = snapshotContainer.Data;
         if (_orderbook.ContainsKey(symbol))
         {
            _orderbook.Remove(symbol);
         }
         if (_snapshotSequenceNos.ContainsKey(symbol))
         {
            _snapshotSequenceNos[symbol] = Int64.Parse(snapshot.Sequence);
         }
         else
            _snapshotSequenceNos.Add(symbol, Int64.Parse(snapshot.Sequence));

         OrderBook orderBook = new OrderBook();
         _orderbook.Add(symbol, orderBook);

         foreach (var bid in snapshot.Bids)
         {
            var price = Decimal.Parse(bid[0]);
            var quantity = Decimal.Parse(bid[1]);
            var l = new Level(Decimal.Parse(bid[0]), Decimal.Parse(bid[1]));
            orderBook.Bid.Add(l);
         }

         foreach (var ask in snapshot.Asks)
         {
            var price = Decimal.Parse(ask[0]);
            var quantity = Decimal.Parse(ask[1]);
            var l = new Level(price, quantity);
            orderBook.Ask.Add(l);
         }
         SetOrderBookSnapshotState(symbol, OrderBookState.REPLAYING_CACHE);
         ReplayL2UpdatesAfterSnapshot(symbol);
      }

      private void ReplayL2UpdatesAfterSnapshot(string symbol)
      {
         // get the orderbook
         //if 
         OrderBook orderbook = null;
         if (_orderbook.ContainsKey(symbol))
         {
            orderbook = _orderbook[symbol];
         }
         else
         {
            _logger.LogError("No Orderbook for {Symbol}", symbol);
            throw new Exception($"No Orderbook for {symbol}");
         }

         if (_cachedOrderBooks.ContainsKey(symbol))
         {
            Int64 currentSequenceNumber = 0;
            if (_snapshotSequenceNos.ContainsKey(symbol))
               currentSequenceNumber = _snapshotSequenceNos[symbol];
            var cachedOrderBook = _cachedOrderBooks[symbol];
            if (cachedOrderBook.Count > 0)
            {
               foreach (var level2Change in cachedOrderBook)
               {
                  if (level2Change.SequenceEnd <= currentSequenceNumber)
                     continue;
                  UpdateOrderBook(level2Change.Changes.Bids, orderbook.Bid, currentSequenceNumber, true);
                  UpdateOrderBook(level2Change.Changes.Asks, orderbook.Ask, currentSequenceNumber, false);
               }
            }
         }
         
         // Can now publish an initial snapshot of the orderbook
         if(_pushOrderBook.TryDequeue(out var pushedSymbol))
            PublishInitialOrderBook(pushedSymbol.Item1, pushedSymbol.Item2);
         SetOrderBookSnapshotState(symbol, OrderBookState.REALTIME);
      }

      private void PublishInitialOrderBook(string instanceName, string symbol)
      {
         var response = new MessageBusReponse()
         {
            ResponseType = ResponseTypeEnums.ORDERBOOK_SNAPSHOT,
            FromVenue = Constants.KUCOIN,
            AccountName = "",
         };

         lock (_orderBookLock)
         {
            var routingKey = symbol + "." + Common.Constants.KUCOIN + Common.Constants.ORDERBOOK_TOPIC;
            var book = _orderbook[symbol];
            var snapshot = new OrderBookSnapshot(symbol, book);
            snapshot.Symbol = _exchangeToGenericLookup[symbol];
            response.Data = JsonSerializer.Serialize(snapshot);
            PublishHelper.PublishToTopic(routingKey, response, _messageBroker);
            if (!_snapshotSent.ContainsKey(symbol))
               _snapshotSent.Add(symbol, true);          
         }        
      }

      public Task DisconnectPublic()
      {
         throw new NotImplementedException();
      }

      public async Task GetLatestTrades(string instanceName, string symbol)
      {
         
      }
   }
}
