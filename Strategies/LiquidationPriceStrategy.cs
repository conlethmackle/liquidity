using AccountBalanceManager;
using ClientConnections;
using Common.Models;
using ConnectorStatus;
using DataStore;
using DynamicConfigHandling;
using FairValueProcessing;
using MessageBroker;
using Microsoft.Extensions.Logging;
using OrderAndTradeProcessing;
using OrderBookProcessing;
using Strategies.Common;
using StrategyMessageListener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common;
using Common.Messages;
using Common.Models.DTOs;
using TelegramAlertsApi;

namespace Strategies
{
   public interface ILiquidationPriceStrategy
   {

   }

   
   public class LiquidationPriceStrategy : BaseStrategy, ILiquidationPriceStrategy
   {
      private readonly ITelegramAlertsApi _telegramAlerter;
      private decimal _subscriptionPrice { get; set; }
      private decimal _percentageSpreadFromFV { get; set; }
      private decimal _lowerThresholdPercentageSpread { get; set; }
      
      private decimal _quantitySize { get; set; }
      private FairValueData _currentFairValue { get; set; } = new FairValueData();
      private int _shortTimePeriod { get; set; }
      private int _longTimePeriod { get; set; }
      private int _cancelTimerPeriod { get; set; }
      private int _takerTimerPeriod { get; set; }
      private int _numberOfCycles { get; set; } = 0;
      private int _currentCycleNo { get; set; } = 0;
      private int _currentCancelTimerCycleNo { get; set; }
      private bool _cancelTimerInProgress { get; set; } = false;
      private List<int> _cancelOrderBatchNo { get; set; } = new List<int>();
      private List<decimal> _cancelOrderSizes { get; set; } = new List<decimal>();
      private int _batchSize { get; set; } = 0;
      private int _priceDecimals { get; set; } = 0;
      private int _amountDecimals { get; set; } = 0;
      private string _symbol { get; set; }
      private string _liquidatedCoin { get; set; }
      private string _stableCoin { get; set; }

      private int _previousNumOrders { get; set; } = 0;
      private bool _cancelAllInFlight { get; set; } = false;

      private decimal _totalCoinToBeLiquidated { get; set; } = 0;
      private decimal _totalCoinLiquidated { get; set; } = 0;
      private decimal _totalCoinLiquidatedToday { get; set; } = 0;
      private decimal _totalCoinToBeLiquidatedToday { get; set; } = 0;
      private decimal _startOfDayCoinBalance { get; set; } = 0;
      private bool _strategyLiquidationComplete { get; set; } = false;
      private bool _stopOnDailyTargetReached { get; set; } = false;
      private bool _liquidationFinishedForToday { get; set; } = false;
      private bool _tradingPaused { get; set; } = false;

      private bool _strategyReadyForAction = false;
      private PeriodicTimer _periodicTimer;
      private CancellationTokenSource _cts;
      //private bool _initialOpenOrdersReceived { get; set; } = false;
      private Dictionary<string, bool> _initialOpenOrdersReceived = new Dictionary<string, bool>();
      private decimal _scalingFactor = 50; // Stick in d/b config
      private StratgeyMode _makerTakerMode;
      private Dictionary<string, Dictionary<string, ExchangeBalance>> _openingBalances { get; set; } = new Dictionary<string, Dictionary<string, ExchangeBalance>>();
      private Dictionary<string, Dictionary<string, ExchangeBalance>> _latestBalances { get; set; } = new Dictionary<string, Dictionary<string, ExchangeBalance>>();
      private Dictionary<string, Dictionary<string, BestBidData>> _bestBidsTable = new();
      private LiquidationOrderLoadingConfigurationDTO _orderLoadingPolicy { get; set; }
      private List<LiquidationManualOrderLoadingDTO> _manualOrderLoading = new List<LiquidationManualOrderLoadingDTO>();
      private object _lockBestBid = new object();
      public LiquidationPriceStrategy(ILoggerFactory loggerFactory, 
         IInventoryManager inventoryManager, 
         IOrderAndTradeProcessing orderTradeProcessing, 
         IOrderBookProcessor orderBookProcessor, 
         IFairValuePricing fairValuePricing, 
         IPortfolioRepository repository, 
         IPrivateClientConnections privateClientConnections, 
         IPublicClientConnections publicClientConnections, 
         IMessageBroker messageBroker, 
         IMessageReceiver messageReceiver, 
         IConnectorStatusListener connectorStatusListener, 
         StrategyStartConfig startupConfig, 
         IDynamicConfigUpdater dynamicConfigUpdater,
         ITelegramAlertsApi telegramAlerter) : 
         base(loggerFactory, inventoryManager, orderTradeProcessing, orderBookProcessor, 
            fairValuePricing, repository, privateClientConnections, publicClientConnections, 
            messageBroker, messageReceiver, connectorStatusListener, startupConfig, dynamicConfigUpdater)
      {
         _telegramAlerter = telegramAlerter;
         inventoryManager.OnOpeningBalance += OnOpeningBalance;
         inventoryManager.OnBalanceUpdate += OnBalanceUpdate;
         _fairValuePricing.OnFairValuePriceChange += OnFairValuePriceChange;
         _orderAndTradeProcessing.OnCancelOrderFailure += OnCancelOrderFailure;
         _orderAndTradeProcessing.OnPlaceOrderFailure += OnPlaceOrderFailure;
         _orderAndTradeProcessing.OnCancelAllOrderSuccess += OnCancelAllOrderSuccess;
         _orderAndTradeProcessing.OnOpenOrdersResponse += OnOpenOrdersResponse;
         _orderAndTradeProcessing.OnNewTrade += OnNewTrade;
         _orderBookProcessor.OnBestBidChange += OnBestBidChange;
         _cts = new CancellationTokenSource();
         _messageReceiver.OnPrivateLoginStatus += PrivateClientLogonMsg;
         _messageReceiver.OnStartOfDay += OnStartOfDay;
         _dynamicConfigUpdater.OnStrategyConfigChange += OnStrategyConfigChange;
      }

      private void OnBestBidChange(string venue, string symbol, decimal price, decimal quantity)
      {
         lock (_lockBestBid)
         {
            Dictionary<string, BestBidData> symbolBestBidTable = null;
            if (_bestBidsTable.ContainsKey(venue))
            {
               symbolBestBidTable = _bestBidsTable[venue];
            }
            else
            {
               symbolBestBidTable = new Dictionary<string, BestBidData>();
               _bestBidsTable[venue] = symbolBestBidTable;
            }

            BestBidData bestBidData = null;
            if (symbolBestBidTable.ContainsKey(symbol))
            {
               bestBidData = symbolBestBidTable[symbol];
            }
            else
            {
               bestBidData = new BestBidData();
               symbolBestBidTable[symbol] = bestBidData;
            }

            bestBidData.BestBidPrice = price;
            bestBidData.BestBidQuantity = quantity;
         }
      }

      public void CancelTimer() => _cts.Cancel();

      public async override Task StrategyInit()
      {
         try
         {
            foreach (var exchange in _exchangeConfig.ExchangeDetails)
            {
               var venue = exchange.Venue.VenueName;
               var venueBalances = new Dictionary<string, ExchangeBalance>();
               _openingBalances.Add(venue, venueBalances);
               var coinPairs = exchange.CoinPairs.Split(",");
               foreach (var coinpair in coinPairs)
               {
                  var coins = coinpair.Split("/");
                  venueBalances[coins[0]] = new ExchangeBalance();
                  venueBalances[coins[1]] = new ExchangeBalance();
               }
            }

            await ReadStrategyConfig();
            await CalculateCurrentPosition();
            if (_makerTakerMode == StratgeyMode.MAKER)
               await _fairValuePricing.Init(MarketType.SPOT, true);
            else if (_makerTakerMode == StratgeyMode.TAKER)
               await _fairValuePricing.Init(MarketType.SPOT, false);
            _periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            await PeriodicTimerExpired();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error in initialising strategy {Error}", e.Message);
            throw;
         }
      }

      private async Task CalculateCurrentPosition()
      {
         try
         {
            var fills = await _portfolioRepository.GetFillsInfoForInstance(_configName);
            foreach (var venueFill in fills)
            {
               _totalCoinLiquidated += venueFill.TotalLiquidated;
               _totalCoinLiquidatedToday += venueFill.LiquidatedToday;
            }
          

            _logger.LogInformation("Calculating current position: Total Coin Liquidated = {TotalCoinLiquidated} TotalCoinLiquidated Today = {TotalCoinLiquidatedToday}",
               _totalCoinLiquidated, _totalCoinLiquidatedToday);
         }
         catch (Exception e)
         {
            _logger.LogError("Error thrown with GetLatestTrades - {Error}", e.Message);
         }
      }

      private async Task ReadStrategyConfig()
      {
         var fairValueConfig = await _portfolioRepository.GetOpeningLiquidationSubscriptionsForInstance(_exchangeConfig.ConfigName);
         if (fairValueConfig == null)
         {
            _logger.LogCritical("Fatal error: No Liquidation Configuration defined for {Instance}", _exchangeConfig.ConfigName);
            throw new Exception($"Fatal error: No Liquidation Configuration defined for {_exchangeConfig.ConfigName}");
         }
         _inventoryManager.InitCoins(fairValueConfig.CoinPair.Name);
         //    foreach(var exchange in _exchangeConfig.ExchangeDetails)
         //   {
         //      _inventoryManager.GetOpeningBalancesDirect(exchange.Venue.VenueName);
         //     _orderAndTradeProcessing.GetOpenOrdersDirect(exchange.Venue.VenueName);
         //    if (_initialOpenOrdersReceived.ContainsKey(exchange.Venue.VenueName))
         //       _initialOpenOrdersReceived[exchange.Venue.VenueName] = false;
         //  }
         _subscriptionPrice = fairValueConfig.SubscriptionPrice;
         
         _totalCoinToBeLiquidated = fairValueConfig.CoinAmount;
         _totalCoinToBeLiquidatedToday = fairValueConfig.DailyLiquidationTarget;
         _percentageSpreadFromFV = fairValueConfig.PercentageSpreadFromFV;
         _quantitySize = fairValueConfig.OrderSize;
         //_currentFairValue = _subscriptionPrice;

         _lowerThresholdPercentageSpread = fairValueConfig.PercentageSpreadLowerThreshold;
         _strategyReadyForAction = true;
         _shortTimePeriod = fairValueConfig.ShortTimeInterval;
         _longTimePeriod = fairValueConfig.LongTimeInterval;
         _numberOfCycles = _longTimePeriod / _shortTimePeriod;
         _cancelTimerPeriod = fairValueConfig.CancelTimerInterval;
         _takerTimerPeriod = fairValueConfig.TakerModeTimeInterval;
         _batchSize = fairValueConfig.BatchSize;
         _priceDecimals = fairValueConfig.PriceDecimals;
         _amountDecimals = fairValueConfig.AmountDecimals;
         _symbol = fairValueConfig.CoinPair.Name;
         var parts = _symbol.Split("/");
         if (parts.Length != 2)
         {
            _logger.LogError("The CoinPair name is not formatted correctly - should be in format BTC/USDT - see {CoinPair}", _symbol);
            throw new Exception($"The CoinPair name is not formatted correctly - should be in format BTC/USDT - see {_symbol}");
         }

         _liquidatedCoin = parts[0];
         _stableCoin = parts[1];

         _orderLoadingPolicy = fairValueConfig.LiquidationOrderLoadingConfiguration;
         if (_orderLoadingPolicy == null || _orderLoadingPolicy.Id == 0)
         {
            _logger.LogError("Error : No order loading policy defined");
            throw new Exception("Error : No order loading policy defined");
            // TODO : Send Telegram Alert
         }

         if (!_orderLoadingPolicy.IsAuto)
         {
            _manualOrderLoading =  await _portfolioRepository.GetLiquidationManualOrderLoadingForInstance(_instanceId);
            if (_manualOrderLoading.Count != _batchSize)
            {
               _logger.LogError("Number of manual Order sizes {ManualOrdersSize} !=  Batch Size {BatchSize}", 
                                                 _manualOrderLoading.Count, _batchSize);
               throw new Exception($"Number of manual Order sizes {_manualOrderLoading.Count} !=  Batch Size {_batchSize}");
            }
         }

         if (_makerTakerMode != fairValueConfig.MakerMode)
         {
            _makerTakerMode = fairValueConfig.MakerMode;
            _fairValuePricing.ChangeMakerTakerMode(_makerTakerMode);
         }

         if (_makerTakerMode == StratgeyMode.TAKER)
         {
            // Cancel existing orders
            var orders = _orderAndTradeProcessing.GetProcessedOpenOrders();
            if (orders != null)
            {
               foreach (var o in orders)
               {
                  _orderAndTradeProcessing.CancelOrder(o.Venue, _symbol, o.ClientOid.ToString(), o.OrderId);
               }
            }
         }
      }

      private async Task OnStrategyConfigChange(StrategyConfigChangeData configUpdate)
      {
         if (_configName.Equals(configUpdate.InstanceName))
         {
            await ReadStrategyConfig();
         }
      }

      private void OnBalanceUpdate(string venue, string coin, ExchangeBalance balance)
      {
         if (_latestBalances.ContainsKey(venue))
         {
            var venueBalances = _latestBalances[venue];
            if (venueBalances.ContainsKey(coin))
            {
               venueBalances[coin] = balance;
               _logger.LogInformation("Latest Balance for {Coin} from {Exchange} is {Balance}", coin, venue, balance.Available);
            }
            else
               venueBalances.Add(coin, balance);
         }
         else
         {
            var venueBalance = new Dictionary<string, ExchangeBalance>();
            _latestBalances[venue] = venueBalance;
            venueBalance.Add(coin, balance);
         }

        // if (coin.Equals(_liquidatedCoin))
        // {
         //   _totalCoinLiquidatedToday = _startOfDayCoinBalance - balance.Available;
        //    _totalCoinLiquidated = _totalCoinToBeLiquidated - balance.Available;
        // }
      }

      private void PrivateClientLogonMsg(string venue, PrivateClientLoginStatus statusMsg)
      {
         _logger.LogInformation("Received Login Status msg from {venue} with status {statusMsg.IsLoggedIn}", venue, statusMsg.IsLoggedIn);
         if (statusMsg.IsLoggedIn)
         {

            _inventoryManager.GetOpeningBalancesDirect(venue);
            _orderAndTradeProcessing.ConnectorLoginStatusChange(venue, true);
            var xxx = _exchangeDetailsTable[venue];
            var coinPairs = xxx.CoinPairs.Split(",");
            _orderAndTradeProcessing.GetOpenOrdersDirect(venue, coinPairs);
            if (_initialOpenOrdersReceived.ContainsKey(venue))
               _initialOpenOrdersReceived[venue] = false;
            SendStrategyAliveMessage(true);
            if (_loginStatusTable.ContainsKey(venue))
            {
               _loginStatusTable[venue] = true;
            }
            else
            {
               _loginStatusTable.TryAdd(venue, true);
            }
         }
         else
         {
            _logger.LogError("Login to {Venue} has failed - error message {Message}", venue, statusMsg.Message);
            SendStrategyAliveMessage(false, $"Login to {venue} has failed - error message {statusMsg.Message}");
            _orderAndTradeProcessing.ConnectorLoginStatusChange(venue, false);
            if (_loginStatusTable.ContainsKey(venue))
            {
               _loginStatusTable[venue] = false;
            }
            else
            {
               _loginStatusTable.TryAdd(venue, false);
            }
         }
      }

      private void SendStrategyAliveMessage(bool status, string? statusMsg = null)
      {
         try
         {
            var strategyStatus = new StrategyInstanceConnectionStatus()
            {
               InstanceName = _configName,
               Status = status,
               ErrorMsg = statusMsg
            };
            var response = new MessageBusReponse()
            {
               ResponseType = ResponseTypeEnums.STRATEGY_ALIVE_STATUS,
               OriginatingSource = _configName,
               Data = JsonSerializer.Serialize(strategyStatus)

            };

            PublishHelper.Publish(Constants.STRATEGY_ALIVE_TOPIC, response, _messageBroker);
         }
         catch (Exception e)
         {
            _logger.LogError("Error cancelling Order {Error}", e.Message);
            throw;
         }
      }

      private void OnOpenOrdersResponse(string venue, List<OwnOrderChange> openOrders) // A request direct to the exchange - on startup
      {
         _logger.LogInformation("********************* Received Open Orders Response from {Venue}", venue);
         if (!_initialOpenOrdersReceived.ContainsKey(venue)) // = true;
         {
            _initialOpenOrdersReceived.Add(venue, true);
         }
         else
            _initialOpenOrdersReceived[venue] = true;


         var orders = _orderAndTradeProcessing.GetPlacedOpenOrdersFromVenue(venue);
         _logger.LogInformation("*************************** {OrderCount} Open Orders to Cancel", orders.Count);
         if (orders.Count > 0)
         {
            _cancelAllInFlight = true;
            _orderAndTradeProcessing.CancelAllOrdersFromVenue(venue);
         }
      }

      private void OnNewTrade(string venue, TradeMsg trade)
      {
         try
         {
            if (trade != null)
            {
               if (!trade.IsBuy)
               {

                   if (trade.Quantity < 0)
                     trade.Quantity = trade.Quantity * -1;
                   _totalCoinLiquidated += trade.Quantity;
                   _totalCoinLiquidatedToday += trade.Quantity;
                   _orderAndTradeProcessing.ConnectorLoginStatusChange(venue, true);
                   CreateAndPlaceNewOrderBatch(_currentFairValue, 1);
               }
            }
         }
         catch (Exception e)
         {
           _logger.LogError("OnNewTrade - Error {Error}");
         }
      }

      private void OnCancelAllOrderSuccess(string venue)
      {
         // Just set that all is good to go
         _logger.LogInformation("********************* OnCancelAllOrderSuccess *******************************************");
         if (_cancelAllInFlight)
         {
            _logger.LogInformation("********************* All Orders successfully cancelled - creating new batch *******************************************");
            _orderAndTradeProcessing.ConnectorLoginStatusChange(venue, true);
      //      CreateAndPlaceNewOrderBatch(_currentFairValue, _batchSize);
            _cancelAllInFlight = false;
         }
      }

      private void OnPlaceOrderFailure(OwnOrderChange order, string clientOid, string venue)
      {
         // TODO - need to have a strategy - will at least send a telegram
         _logger.LogCritical("**************** ASK LEE or DAVE -  Unable to place to {Venue} - {ClientOid} and {ExchangeOrderId} ", venue, clientOid, order.OrderId);
      }

      private void OnCancelOrderFailure(OwnOrderChange order, string clientOid, string venue)
      {
         // TODO - need to have a strategy - will at least send a telegram
         _logger.LogCritical("**************** ASK LEE or DAVE - Unable to cancel to {Venue} - {ClientOid} and {ExchangeOrderId} ", venue, clientOid, order.OrderId);
      }

      private async Task PeriodicTimerExpired()
      {
         try
         {
            while (await _periodicTimer.WaitForNextTickAsync(_cts.Token))
            {
               HandleStrategyAlivePing();
               _currentCycleNo++;
               if (_makerTakerMode == StratgeyMode.MAKER)
               {
                  if (_currentCycleNo == _shortTimePeriod)
                  {
                     if (_orderAndTradeProcessing.GetProcessedOpenOrders().Count == 0)
                     {
                        CreateAndPlaceNewOrderBatch(_currentFairValue, _batchSize);
                     }
                     else
                     {
                        CheckExistingOrders();
                     }

                     _currentCycleNo = 0;
                  }
               }
               else if (_makerTakerMode == StratgeyMode.TAKER)
               {
                  if (_currentCycleNo == _takerTimerPeriod)
                  {
                     if (_orderAndTradeProcessing.GetOpenOrdersCount() == 0)
                     {
                        CreateAndPlaceNewOrderBatch(_currentFairValue, _batchSize);
                     }


                     _currentCycleNo = 0;
                  }
               }

               if (_cancelTimerInProgress)
               {
                  if (_currentCancelTimerCycleNo == _cancelTimerPeriod)
                  {
                     _cancelTimerInProgress = false;
                     CreateAndPlaceNewOrderBatch(_currentFairValue, _cancelOrderBatchNo.Count());
                  }
               }
            }
         }
         catch (Exception e)
         {
            //Handle the exception but don't propagate it
            _logger.LogError(e, "Error in Strategy periodic timer {Error}", e.Message);
         }
      }

      private void HandleStrategyAlivePing()
      {
         // TODO - could do with something better here
         SendStrategyAliveMessage(true, "");
      }

      private void OnFairValuePriceChange(string venue, string symbol, decimal fairValuePrice)
      {
         _currentFairValue.Venue = venue;
         _currentFairValue.CoinPair = symbol;
         _currentFairValue.FairValuePrice = fairValuePrice;
      }

      private void CheckExistingOrders()
      {
         try
         {
            int numOrdersCancelled = 0;
            var orders = _orderAndTradeProcessing.GetProcessedOpenOrders();
            _logger.LogInformation("The number of Open Orders is {NumOrders}", orders.Count);
            if (orders.Count != 0)
            {
               _cancelOrderBatchNo.Clear();
               _cancelOrderSizes.Clear();
               int batchNo = 0;
               foreach (var o in orders)
               {
                  if (o == null)
                  {
                     _logger.LogInformation("CheckExistingOrders has a null order");
                     continue;
                  }

                  if (!CheckIfPriceStillValid(o))
                  {
                     _cancelOrderBatchNo.Add(batchNo);
                     _cancelOrderSizes.Add(o.Quantity);
                     _logger.LogInformation("Cancelling order price {Price} as outside range ", o.Price);
                     _orderAndTradeProcessing.CancelOrder(o.Venue, _symbol, o.ClientOid.ToString(), o.OrderId);
                     numOrdersCancelled++;
                  }
               }

               if (numOrdersCancelled > 0)
               {
                  // Start the cancel timer process
                  _currentCancelTimerCycleNo = 0;
                  _cancelTimerInProgress = true;
                  // _logger.LogInformation("Replacing Cancelled orders with {SizeOfNewOrderBatch} new orders", numOrdersCancelled);
                  // CreateAndPlaceNewOrderBatch(_currentFairValue, numOrdersCancelled);
               }
               else
               {
                  _logger.LogInformation("Not Replacing order as all are valid ");
               }

               batchNo++;
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "CheckExistingOrders failure {Message}", e.Message);
         }
      }

      private bool CheckIfPriceStillValid(OwnOrderChange order)
      {
         lock (_lockBestBid)
         {
            if (_bestBidsTable.ContainsKey(_currentFairValue.Venue))
            {
               var bestBidVenue = _bestBidsTable[_currentFairValue.Venue];
               var bestBidPrice = bestBidVenue[_symbol].BestBidPrice;
               var maxOfOthers = _fairValuePricing.GetMaxOfOthers(_currentFairValue.Venue, _symbol);
               // Make sure the price is 
               if (order.Price > bestBidPrice + (_percentageSpreadFromFV * bestBidPrice * 2) / 100
                   || order.Price < _subscriptionPrice
                   || order.Price < maxOfOthers)
               {
                  return false;
               }
            }

            return true;
         }
      }

      private decimal[] CalculateQuantitiesInMakerMode()
      {
         if (_orderLoadingPolicy.IsAuto)
            return CalculateAutoOrderSizesInMakerMode();
         else
            return CalculateManualOrderSizesInMakerMode();
      }

      private decimal[] CalculateQuantitiesForCancelOrdersInMakerMode()
      {
         decimal[] quantityContainer = new decimal[_cancelOrderSizes.Count];
         int i = 0;
         foreach (var size in _cancelOrderSizes)
         {
            quantityContainer[i] = (decimal)size;
            if (size < 0)
            {
               _logger.LogInformation("Like wtf is going on");
               quantityContainer[i] = (decimal)size *-1;
            }

            i++;
         }
         return quantityContainer;
      }

      private decimal[] CalculateAutoOrderSizesInMakerMode()
      {
         var quantityContainer = new decimal[_batchSize];
         decimal remainingOrderSize = _quantitySize;
         var scalingFactor = _orderLoadingPolicy.ScalingFactor;

         for (int i = 0; i < _batchSize - 1; i++)
         {
            var size = remainingOrderSize * scalingFactor / 100;
            quantityContainer[i] = (decimal)size;
            if (size < 0)
            {
               _logger.LogInformation("Like wtf is going on");
               quantityContainer[i] = (decimal)size * -1;
            }

            remainingOrderSize -= size;
         }
         quantityContainer[_batchSize - 1] = remainingOrderSize;
         if (!_orderLoadingPolicy.IsHighestFirst)
            Array.Reverse(quantityContainer);
         RandomiseSizes(ref quantityContainer);
         return quantityContainer;
         
      }

      private decimal[] CalculateManualOrderSizesInMakerMode()
      {

         var quantityContainer = new decimal[_batchSize];
         var scalingFactor = _orderLoadingPolicy.ScalingFactor;

         foreach (var manualOrderSize in _manualOrderLoading)
         {
            quantityContainer[manualOrderSize.OrderNo - 1] = _quantitySize * manualOrderSize.Percentage / 100;
         }
         RandomiseSizes(ref quantityContainer);
         return quantityContainer;
      }

      private void RandomiseSizes(ref decimal[] sizes)
      {
         Random rnd = new Random();
         
         for (int i = 0; i < sizes.Length; i++)
         {
            var randomness = rnd.Next(80, 100);
            sizes[i] = Math.Round((randomness * sizes[i]) / 100, _amountDecimals, MidpointRounding.ToEven);
         }
      }

      private void CreateAndPlaceNewOrderBatch(FairValueData fairValue, int numOrders)
      {
         if (string.IsNullOrEmpty(fairValue.Venue))
         {
            _logger.LogWarning("Not placing orders as FairValue not valid");
            return;
         }

         _logger.LogInformation("In CreateAndPlaceNewOrderBatch ");

         if (!_initialOpenOrdersReceived.ContainsKey(fairValue.Venue))
         {
            _logger.LogInformation("Not received open orders response fromm {Venue} - not placing orders", fairValue.Venue);
            string[] coinPairs = new string[1];
            coinPairs[0] = fairValue.CoinPair;
            _orderAndTradeProcessing.GetOpenOrdersDirect(fairValue.Venue, coinPairs);
            return;
         }

         if (_cancelAllInFlight)
         {
            var orders = _orderAndTradeProcessing.GetOpenOrdersFromVenue(fairValue.Venue);
            if (orders.Count > 0)
            {

               _logger.LogInformation("Cancel All orders from {Venue} still valid - not placing orders", fairValue.Venue);
               return;
            }
            else
            {
               _cancelAllInFlight = false;
            }
         }

         string[] coins = _symbol.Split("/");
         bool orderPlaced = false;
         if (_makerTakerMode == StratgeyMode.MAKER)
         {
           
            _logger.LogInformation("calling PlaceOrdersInMakerMode");
            PlaceOrdersInMakerMode(fairValue, numOrders);
         }
         else if (_makerTakerMode == StratgeyMode.TAKER)
         {
            _logger.LogInformation("calling PlaceOrdersInTakerMode");
            PlaceOrdersInTakerMode(fairValue, numOrders);
         }
      }

      private void PlaceOrdersInTakerMode(FairValueData fairValue, int numOrders)
      {
         decimal price = 0;
         decimal quantity = 0;
         decimal availableBalance = 0.0m;
         string[] coins = _symbol.Split("/");
         bool orderPlaced = false;

         if (_latestBalances.ContainsKey(fairValue.Venue))
         {
            var venUeBalances = _latestBalances[fairValue.Venue];
            if (venUeBalances.ContainsKey(coins[0]))
            {
               var bal = venUeBalances[coins[0]];
               availableBalance = bal.Available;
            }
         }

         lock (_lockBestBid)
         {
            if (_bestBidsTable.ContainsKey(_currentFairValue.Venue))
            {
               var bb = _bestBidsTable[_currentFairValue.Venue];
               if (bb.ContainsKey(_symbol))

               {

                  price = bb[_symbol].BestBidPrice;
                  var availableQuantity = bb[_symbol].BestBidQuantity;
                  if (availableQuantity < _quantitySize)
                     quantity = availableQuantity;
                  else
                     quantity = _quantitySize;
               }

               if (CheckCriteriaForTrading(fairValue.Venue, availableBalance, quantity))
               {
                  _orderAndTradeProcessing.PlaceOrder(fairValue.Venue,
                        _symbol,
                        price,
                        quantity,
                        TimeInForceEnum.POST,
                        false,
                        OrderTypeEnum.LIMITMAKER);
                  orderPlaced = true;
               }
            }
         }

         if (!orderPlaced)
            _logger.LogInformation("************** No orders placed **************************");
         else
         {
            _logger.LogInformation("***************1 taker order placed at {Price} and {Quantity} at {Venue}", price, quantity, fairValue.Venue);
            _previousNumOrders = 1;
         }
      }

      private void PlaceOrdersInMakerMode(FairValueData fairValue, int numOrders)
      {
         bool orderPlaced = false;
         string[] coins = _symbol.Split("/");
         List<decimal> prices = new List<decimal>();
         prices = !_cancelOrderBatchNo.Any() ? 
            CalculatePricesForBatch(_currentFairValue.FairValuePrice, numOrders) :
            CalculatePricesForCancelReplacementBatch(_currentFairValue.FairValuePrice, numOrders);

         var sizes = _cancelOrderSizes.Any()
            ? CalculateQuantitiesForCancelOrdersInMakerMode() :
              CalculateQuantitiesInMakerMode();
         decimal availableBalance = 0.0m;
         int sizeCounter = 0;

         foreach (var price in prices)
         {
            if (_latestBalances.ContainsKey(fairValue.Venue))
            {
               var venUeBalances = _latestBalances[fairValue.Venue];
               if (venUeBalances.ContainsKey(coins[0]))
               {
                  var bal = venUeBalances[coins[0]];
                  availableBalance = bal.Available;
               }
            }

            _logger.LogInformation("Attempting to place order of price {Price} at {Venue}", price, fairValue.Venue);
            _logger.LogInformation("Available Balance of {Coin} is {AvailableBalance}", coins[0], availableBalance);
            _logger.LogInformation("Fair Value Price = {CurrentFairValue}", _currentFairValue.FairValuePrice);
            _logger.LogInformation("Quantity = {Quantity}", sizes[sizeCounter]);

            if (CheckCriteriaForTrading(fairValue.Venue, availableBalance, sizes[sizeCounter]))
            {
               if (price >= _currentFairValue.FairValuePrice)
               {
                  _orderAndTradeProcessing.PlaceOrder(fairValue.Venue,
                     _symbol,
                     price,
                     sizes[sizeCounter],
                     TimeInForceEnum.POST,
                     false,
                     OrderTypeEnum.LIMITMAKER);
                  sizeCounter++;
                  orderPlaced = true;
               }
               else
               {
                  _logger.LogInformation(
                     "**************************** Not placing order as price {Price} < {FairValue} {Available} of {Coin} at {Venue} - No order being placed",
                     price, _currentFairValue.FairValuePrice, availableBalance, coins[0], fairValue.Venue);
               }
            }
            else
            {
               _logger.LogInformation(
                  "**************************** Insufficient Balance {Available} of {Coin} at {Venue} - No order being placed",
                  availableBalance, coins[0], fairValue.Venue);
            }
         }

         if (!orderPlaced)
            _logger.LogInformation("************** No orders placed **************************");
         else
         {
            _logger.LogInformation("***************{OrderCount} orders placed", prices.Count);
            _previousNumOrders = prices.Count;
         }
      }

      private bool CheckCriteriaForTrading(string venue, decimal availableBalance, decimal quantity)
      {
         bool state = true;
         string reason = "";

         if (quantity > availableBalance)
         {
            state = false;
            reason =
               $"**************************** Insufficient Balance {availableBalance} of {_liquidatedCoin} at {venue} - No order being placed";
            _telegramAlerter.SendLiquidityAlert(ResponseTypeEnums.LIQUIDITY_PROGRESS_ALERT, 
                                                   LiquidityAlertTypes.INSUFFICIENT_BALANCE,
                                                            _portfolioName, _configName, reason, venue);
         }
         else if (_strategyLiquidationComplete)
         {
            state = false;
            reason =
               $"**************************** Strategy already Complete {_totalCoinToBeLiquidated} {_liquidatedCoin} liquidated";
            _telegramAlerter.SendLiquidityAlert(ResponseTypeEnums.LIQUIDITY_PROGRESS_ALERT,
               LiquidityAlertTypes.LIQUIDATION_COMPLETED,
               _portfolioName, _configName, reason, venue);
         }
         else if (_liquidationFinishedForToday)
         {
            reason =
               $"**************************** Strategy already Completed liquidation for today {_totalCoinToBeLiquidatedToday} {_liquidatedCoin} liquidated";
            state = false;
            _telegramAlerter.SendLiquidityAlert(ResponseTypeEnums.LIQUIDITY_PROGRESS_ALERT,
               LiquidityAlertTypes.DAILY_LIQUIDATION_TARGET_REACHED,
               _portfolioName, _configName, reason, venue);
         }
         else if (quantity + _totalCoinLiquidated > _totalCoinToBeLiquidated)
         {
            state = false;
            _strategyLiquidationComplete = true;
            reason =
               $"**************************** Strategy Complete {_totalCoinToBeLiquidated} {_liquidatedCoin} liquidated";
            _telegramAlerter.SendLiquidityAlert(ResponseTypeEnums.LIQUIDITY_PROGRESS_ALERT,
               LiquidityAlertTypes.LIQUIDATION_COMPLETED,
               _portfolioName, _configName, reason, venue);
         }
         else if (quantity + _totalCoinLiquidatedToday > _totalCoinToBeLiquidatedToday && _stopOnDailyTargetReached)
         {
            _liquidationFinishedForToday = true;
            state = false;
            reason =
               $"**************************** Strategy Completed liquidation for today {_totalCoinToBeLiquidatedToday} {_liquidatedCoin} liquidated";
            _telegramAlerter.SendLiquidityAlert(ResponseTypeEnums.LIQUIDITY_PROGRESS_ALERT,
               LiquidityAlertTypes.DAILY_LIQUIDATION_TARGET_REACHED,
               _portfolioName, _configName, reason, venue);
         }
         else if (_tradingPaused)
         {
            reason =
               $"**************************** Trading Paused currently ";
            state = false;
            _telegramAlerter.SendLiquidityAlert(ResponseTypeEnums.LIQUIDITY_PROGRESS_ALERT,
               LiquidityAlertTypes.DAILY_LIQUIDATION_TARGET_REACHED,
               _portfolioName, _configName, reason, venue);
         }
         return true;
      }

      private List<decimal> CalculatePricesForBatch(decimal fairValue, int numOrders)
      {
         List<decimal> prices = new List<decimal>();
         var start = _lowerThresholdPercentageSpread;
         var end = _percentageSpreadFromFV;
         decimal step = 0;
         if (_batchSize > 1)
            step = Math.Round((end - start) / (_batchSize - 1), _priceDecimals, MidpointRounding.ToEven);
         for (int i = 1; i <= numOrders; i++)
         {
            var price = fairValue + ((start + step * i) * fairValue) / 100;
            price = Math.Round(price, _priceDecimals, MidpointRounding.ToEven);
            if (price >= _subscriptionPrice)
            {
               prices.Add(price);
            }
            else
            {
               _logger.LogInformation("Price {Price} is less than the subscription price of {Subscription}", price, _subscriptionPrice);
            }
         }
         return prices;
      }

      private List<decimal> CalculatePricesForCancelReplacementBatch(decimal fairValue, int numOrders)
      {
         List<decimal> prices = new List<decimal>();
         var start = _lowerThresholdPercentageSpread / 100;
         var end = _percentageSpreadFromFV / 100;
         decimal step = 0;
         if (_batchSize > 1)
            step = Math.Round((end - start) / (_batchSize - 1), _priceDecimals, MidpointRounding.ToEven);
         foreach (var batchNo in _cancelOrderBatchNo)
         {
            var price = fairValue + ((start + step * batchNo) * fairValue) / 100;
            if (price >= _subscriptionPrice)
            {
               prices.Add(price);
            }
            else
            {
               _logger.LogInformation("Price {Price} is less than the subscription price of {Subscription}", price, _subscriptionPrice);
            }
            
         }
         
         return prices;
      }

      private void OnOpeningBalance(string venue, string coin, ExchangeBalance balance)
      {
         if (_openingBalances.ContainsKey(venue))
         {
            var venueBalances = _openingBalances[venue];
            if (venueBalances.ContainsKey(coin))
            {
               venueBalances[coin] = balance;
               _logger.LogInformation("Opening Balance for {Coin} from {Exchange} is {Balance}", coin, venue, balance.Available);
            }
            else
               venueBalances.Add(coin, balance);
         }
         else
         {
            var venueBalance = new Dictionary<string, ExchangeBalance>();
            _openingBalances[venue] = venueBalance;
            venueBalance.Add(coin, balance);
         }
      }

      private void OnStartOfDay()
      {
         _totalCoinLiquidatedToday = 0;
         _startOfDayCoinBalance = 0;
         // 1. Get the available balance for the coin. Use the last updated balance figure as this will
         // avoid the need to go to 
         foreach (var pair in _latestBalances)
         {
            var venueBalance = pair.Value;
            if (venueBalance.ContainsKey(_liquidatedCoin))
            {
               var bal = venueBalance[_liquidatedCoin];
               _startOfDayCoinBalance += bal.Available;
            }
         }
      }
   }
}
