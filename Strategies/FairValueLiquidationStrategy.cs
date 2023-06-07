using Strategies.Common;
using AccountBalanceManager;
using ClientConnections;
using Common.Models;
using Common;
using ConnectorStatus;
using DataStore;
using MessageBroker;
using Microsoft.Extensions.Logging;
using OrderAndTradeProcessing;
using OrderBookProcessing;
using StrategyMessageListener;
using LastTradedPriceProcessing;
using FairValueProcessing;
using Common.Messages;
using System.Text.Json;
using DynamicConfigHandling;
using CustomerIdAllocation;

namespace Strategies
{
   public interface IFairValueLiquidationStrategy
   {

   }
   public class FairValueLiquidationStrategy : BaseStrategy, IFairValueLiquidationStrategy
   {
      private decimal _subscriptionPrice { get; set; }
      private decimal _initialNumCoins { get; set; }
      private decimal _percentageSpreadFromFV { get; set; }
      private decimal _lowerThresholdPercentageSpread { get; set; }
      private string _venue { get; set; }
      private CancellationPolicyOnStart _cancellationPolicyOnStart { get; set; }
      private decimal _quantitySize { get; set; }
      private decimal _currentFairValue { get; set; }
      private int _shortTimePeriod { get; set; }
      private int _longTimePeriod { get; set; }
      private int _strategyAliveIntervalSecs { get; set; }
      private int _currentCycleTime { get; set; } = 0;
      private int _currentStrategyAliveTime { get; set; } = 0;
      private int _numberOfCycles { get; set; } = 0;
      private int _currentCycleNo { get; set; } = 0;
      private int _batchSize { get; set; } = 0;
      private int _priceDecimals { get; set; } = 0;
      private int _amountDecimals { get; set; } = 0;
      private string _symbol { get; set; }
      private int _previousFillCount { get; set; } = 0;
      private int _previousNumOrders { get; set; } = 0;
      private bool _cancelAllInFlight { get; set; } = false;

      private bool _strategyReadyForAction = false;
      private PeriodicTimer _periodicTimer;
      private PeriodicTimer _periodicStrategyAliveTimer;
      private CancellationTokenSource _cts;
      //private bool _initialOpenOrdersReceived { get; set; } = false;
      private Dictionary<string, bool> _initialOpenOrdersReceived = new Dictionary<string, bool>();

      private Dictionary<string, Dictionary<string, ExchangeBalance>> _openingBalances { get; set; } = new Dictionary<string, Dictionary<string, ExchangeBalance>>();
      private Dictionary<string, Dictionary<string, ExchangeBalance>> _latestBalances { get; set; } = new Dictionary<string, Dictionary<string, ExchangeBalance>>();
      public FairValueLiquidationStrategy(ILoggerFactory loggerFactory,
                                  IInventoryManager inventoryManager,
                                  IOrderAndTradeProcessing orderTradeProcessing,
                                  IOrderBookProcessor orderBookProcessor,
                                  IFairValuePricing fairValuePricing,
                                  IPortfolioRepository repository,
                                  IPrivateClientConnections privateClientConnections,
                                  IPublicClientConnections publicClientConnections,
                                  IMessageBroker messageBroker,
                                  IMessageReceiver messageReceiver,
                                  IConnectorStatusListener connectionListener,
                                  StrategyStartConfig startupConfig,
                                  IDynamicConfigUpdater dynamicConfigUpdater
                                  ) :
          base(loggerFactory,
               inventoryManager,
               orderTradeProcessing,
               orderBookProcessor,
               fairValuePricing,
               repository,
               privateClientConnections,
               publicClientConnections,
               messageBroker,
               messageReceiver,
               connectionListener,
               startupConfig,
               dynamicConfigUpdater)
      {
         inventoryManager.OnOpeningBalance += OnOpeningBalance;
         inventoryManager.OnBalanceUpdate += OnBalanceUpdate;
         _fairValuePricing.OnFairValuePriceChange += OnFairValuePriceChange;
         _orderAndTradeProcessing.OnCancelOrderFailure += OnCancelOrderFailure;
         _orderAndTradeProcessing.OnPlaceOrderFailure += OnPlaceOrderFailure;
         _orderAndTradeProcessing.OnCancelAllOrderSuccess += OnCancelAllOrderSuccess;
         _orderAndTradeProcessing.OnOpenOrdersResponse += OnOpenOrdersResponse;
         _cts = new CancellationTokenSource();
         _messageReceiver.OnPrivateLoginStatus += PrivateClientLogonMsg;
         _dynamicConfigUpdater.OnStrategyConfigChange += OnStrategyConfigChange;
         _strategyAliveIntervalSecs = 5; // Maybe configure
         _currentStrategyAliveTime = _strategyAliveIntervalSecs;
         _currentCycleTime = _shortTimePeriod;
      }

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

            _periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            await PeriodicTimerExpired();
        
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error in initialising strategy {Error}", e.Message);
            throw;
         }
      }

   private async Task OnStrategyConfigChange(StrategyConfigChangeData configUpdate)
   {
      if (_configName.Equals(configUpdate.InstanceName))
      {
         await ReadStrategyConfig();
      }
   }

   private void PrivateClientLogonMsg(string venue, PrivateClientLoginStatus statusMsg)
   {
      _logger.LogInformation("Received Login Status msg from {venue} with status {statusMsg.IsLoggedIn}", venue, statusMsg.IsLoggedIn);
      if (statusMsg.IsLoggedIn)
      {
       
         _inventoryManager.GetOpeningBalancesDirect(venue);
         _orderAndTradeProcessing.ConnectorLoginStatusChange(venue, true);
         if (_exchangeDetailsTable.ContainsKey(venue))
         {
            var exchangeConfig = _exchangeDetailsTable[venue];
            var coinpairs = exchangeConfig.CoinPairs.Split(",");
            _orderAndTradeProcessing.GetOpenOrdersDirect(venue, coinpairs);
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
            _logger.LogError("Attempt to login to venue {Venue} has failed, as venue not known", venue);
            throw new Exception($"Attempt to login to venue {venue} has failed, as venue not known");
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

   private void SendStrategyAliveMessage(bool status, string? statusMsg=null)
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
            ResponseType  = ResponseTypeEnums.STRATEGY_ALIVE_STATUS,
            OriginatingSource = _configName,
            Data = JsonSerializer.Serialize(strategyStatus)
         
         };
        
        // _logger.LogInformation("***************************** Sending StrategyAlive ping for {ConfigName} ******************", _configName);
         PublishHelper.Publish(Constants.STRATEGY_ALIVE_TOPIC, response, _messageBroker);
      }
      catch (Exception e)
      {
         _logger.LogError("Error cancelling Order {Error}", e.Message);
         throw;
      }
   }

   private void HandleStrategyAlivePing()
   {
      // TODO - could do with something better here
      SendStrategyAliveMessage(true, "");
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
   }

   public void CancelTimer() => _cts.Cancel();

   private async Task ReadStrategyConfig()
   {
      var fairValueConfig = await _portfolioRepository.GetOpeningLiquidationSubscriptionsForInstance(_exchangeConfig.ConfigName);
     // var fairValueConfig = await _portfolioRepository.GetLiquidationStrategyConfigByStrategyConfigId(_exchangeConfig.StrategySPSubscriptionConfigId);
      _inventoryManager.InitCoins(fairValueConfig.CoinPair.Name);
      //    foreach(var exchange in _exchangeConfig.ExchangeDetails)
      //   {
      //      _inventoryManager.GetOpeningBalancesDirect(exchange.Venue.VenueName);
      //     _orderAndTradeProcessing.GetOpenOrdersDirect(exchange.Venue.VenueName);
      //    if (_initialOpenOrdersReceived.ContainsKey(exchange.Venue.VenueName))
      //       _initialOpenOrdersReceived[exchange.Venue.VenueName] = false;
      //  }
      _subscriptionPrice = fairValueConfig.SubscriptionPrice;
      _initialNumCoins = fairValueConfig.CoinAmount;
      _percentageSpreadFromFV = fairValueConfig.PercentageSpreadFromFV;
      _quantitySize = fairValueConfig.OrderSize;
      _currentFairValue = _subscriptionPrice;
      _venue = fairValueConfig.StrategySPSubscriptionConfig.ExchangeDetails[0].Venue.VenueName;
    //  _cancellationPolicyOnStart = (CancellationPolicyOnStart)fairValueConfig.CancellationPolicyOnStart;
      _lowerThresholdPercentageSpread = fairValueConfig.PercentageSpreadLowerThreshold;
      _strategyReadyForAction = true;
      _shortTimePeriod = fairValueConfig.ShortTimeInterval;
      _longTimePeriod = fairValueConfig.LongTimeInterval;
      _numberOfCycles = _longTimePeriod / _shortTimePeriod;
      _batchSize = fairValueConfig.BatchSize;
      _priceDecimals = fairValueConfig.PriceDecimals;
      _amountDecimals = fairValueConfig.AmountDecimals;
      _symbol = fairValueConfig.CoinPair.Name;
      _currentStrategyAliveTime = _strategyAliveIntervalSecs;
      _currentCycleTime = _shortTimePeriod;

         // Validate the above

         if (_numberOfCycles < 1)
      {
         _logger.LogError("************ Time periods for short and long cycles incorrectly defined");
         throw new Exception("************ Time periods for short and long cycles incorrectly defined");
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

   private void OnCancelAllOrderSuccess(string venue)
   {
      // Just set that all is good to go
      _logger.LogInformation("********************* OnCancelAllOrderSuccess *******************************************");
      if (_cancelAllInFlight)
      {
         _logger.LogInformation("********************* All Orders successfully cancelled - creating new batch *******************************************");
         _orderAndTradeProcessing.ConnectorLoginStatusChange(venue, true);
         CreateAndPlaceNewOrderBatch(_currentFairValue, _batchSize);
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
            HandleCycles();
            HandleStrategyAliveTimer();
         }
      }
      catch (Exception e)
      {
         //Handle the exception but don't propagate it
         _logger.LogError(e, "Error in Strategy periodic timer {Error}", e.Message);
      }
   }

   private void HandleCycles()
   {
      _currentCycleTime--;
      if (_currentCycleTime == 0)
      {
         // private int _currentStrategyAliveTime { get; set; } = 0;
         _currentCycleNo++;
         if (_currentCycleNo != _numberOfCycles)
         {
            HandleShortCycle();
         }
         else
         {
            HandleLongCycle();
            _currentCycleNo = 0;
         }
         _currentCycleTime = _shortTimePeriod;
      }
   }

   private void HandleLongCycle()
   {
      _logger.LogInformation("**********************In HandleLongCycle *******************************");
      var totalNumFills = _orderAndTradeProcessing.GetFilledOrdersFromVenue(_venue).Count();
      if (totalNumFills == _previousFillCount)
      {
         var orderCount = _orderAndTradeProcessing.GetPlacedOpenOrdersFromVenue(_venue).Count();
         if (orderCount > 0)
         {
            _logger.LogInformation("Cancelling All orders as order count is {OrderCount}", orderCount);
            _orderAndTradeProcessing.CancelAllOrdersFromVenue(_venue);
            // Need to add a new batch now, but will wait until all the current ones
            // have been successfully
            _cancelAllInFlight = true;
         }
      }
   }

   private void HandleShortCycle()
   {
      _logger.LogInformation("**********************In HandleShortCycle *******************************");
      // Has there be any fills?
      var totalNumFills = _orderAndTradeProcessing.GetFilledOrdersFromVenue(_venue).Count();
      if (totalNumFills > _previousFillCount)
      {
         var numFillsInCycle = totalNumFills - _previousFillCount;
         _previousFillCount = totalNumFills;
         // Could be a partial fill
         var openOrdersCount = _orderAndTradeProcessing.GetOpenOrdersFromVenue(_venue).Count();
         if (_previousNumOrders == openOrdersCount)
            _logger.LogInformation("************** Partial fill in previous period ***********************");

         // If the number of open orders is < _batchSize - try and add more
         if (openOrdersCount < _batchSize)
            CreateAndPlaceNewOrderBatch(_currentFairValue, _batchSize - openOrdersCount);
      }
   }

   private void HandleStrategyAliveTimer()
   {
      _currentStrategyAliveTime--;
      if (_currentStrategyAliveTime == 0)
      {
         SendStrategyAliveMessage(true);
         _currentStrategyAliveTime = _strategyAliveIntervalSecs;

      }
   }

   private void OnFairValuePriceChange(string venue, string symbol, decimal fairValuePrice)
   {
      if (!_strategyReadyForAction) return;
      // Update for multiple symbols
      _currentFairValue = fairValuePrice;
      if (_orderAndTradeProcessing.GetOpenOrdersFromVenue(_venue).Count() == 0)
         CreateAndPlaceNewOrderBatch(fairValuePrice, _batchSize);
      else      
         CheckExistingOrders(symbol, fairValuePrice);       
   }

   private void CheckExistingOrders(string symbol, decimal fairValuePrice)
   {
      try
      {
         if (!_loginStatusTable[_venue])
         {
            _logger.LogInformation("No Orders being placed to {Exchange} as not logged in", _venue);
            OnConnectorIsUp(_venue); // Try login again
            return;
         }
            var orders = _orderAndTradeProcessing.GetProcessedOpenOrdersFromVenue(_venue);
         if (orders != null)
         {
            foreach (var o in orders)               
            {
               if (o == null)
               {
                  _logger.LogInformation("CheckExistingOrders has a null order");
                  continue;
               }
            // Make sure the price is 
               if (o.Price < fairValuePrice || o.Price < _subscriptionPrice)
               {
                  _logger.LogInformation("Cancelling order as price {Price} is below fairvalue {FairValue} or Subscription price {SubscriptionPrice}", o.Price, fairValuePrice, _subscriptionPrice);
                  _orderAndTradeProcessing.CancelOrder(_venue, o.Symbol, o.ClientOid.ToString(), o.OrderId);
               }
            }
         }
      }
      catch(Exception e)
      {
         _logger.LogError(e, "CheckExistingOrders failure {Message}", e.Message);
      }
   }


   private void CreateAndPlaceNewOrderBatch(decimal fairValue, int numOrders)
   {
      if (!_initialOpenOrdersReceived.ContainsKey(_venue))
      {
         _logger.LogInformation("Not received open orders response fromm {Venue} - not placing orders", _venue);
        // _orderAndTradeProcessing.GetOpenOrdersDirect(_venue);
         return;
      }
      if (_cancelAllInFlight)
      {
         var orders = _orderAndTradeProcessing.GetOpenOrdersFromVenue(_venue);
         if (orders.Count > 0)
         {

            _logger.LogInformation("Cancel All orders from {Venue} still valid - not placing orders", _venue);
            return;
         }
         else
         {
            _cancelAllInFlight = false;
         }
      }

      string[] coins = _symbol.Split("/");
      bool orderPlaced = false;
      var prices = CalculatePricesForBatch(fairValue, numOrders);
      decimal availableBalance = 0.0m;
      if (!_loginStatusTable[_venue])
      {
         _logger.LogInformation("No Orders being placed to {Exchange} as not logged in", _venue);
         OnConnectorIsUp(_venue); // Try login again
         return;
      }
      foreach(var price in prices)
      {
         if (_latestBalances.ContainsKey(_venue))
         {
            var venUeBalances = _latestBalances[_venue];
            if (venUeBalances.ContainsKey(coins[0]))
            {
               var bal = venUeBalances[coins[0]];
               availableBalance = bal.Available;
            }
         }
         _logger.LogInformation("Attempting to place order of price {Price}", price);
         _logger.LogInformation("Available Balance of {Coin} is {AvailableBalance}", coins[0], availableBalance);
         _logger.LogInformation("Lowest Price = {CurrentFairValue}", _currentFairValue);
         if (_quantitySize <= availableBalance)
         {
            if (price >= _currentFairValue)
            {
               if (_orderAndTradeProcessing.PlaceOrder(_venue,
                      _symbol,
                      price,
                      _quantitySize,
                      TimeInForceEnum.POST,
                      false,
                      OrderTypeEnum.LIMITMAKER) == -1)
               {
                  _logger.LogError("Order to {Exchange} has not been placed", _venue);
               }
               orderPlaced = true;
            }
         }
         else
         {
           _logger.LogInformation("**************************** Insufficient Balance {Available} of {Coin} - No order being placed", availableBalance, coins[0]);
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

   private List<decimal> CalculatePricesForBatch(decimal fairValue, int numOrders)
   {
      List<decimal> prices = new List<decimal>();
      var start = _lowerThresholdPercentageSpread;
      var end = _percentageSpreadFromFV;
      decimal step = 0;
      if (_batchSize > 1)
          step = Math.Round((end - start)/(_batchSize -1), _priceDecimals, MidpointRounding.ToEven);
      for(int i = 1; i <= numOrders; i++)
      {
         var price = fairValue + ((start + step * i) * fairValue)/100;
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
  }
}
