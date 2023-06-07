using System;
using System.Collections.Generic;
using System.Linq;
using Common.Messages;
using Common.Models;
using Common.Models.DTOs;
using DataStore;
using FairValueProcessing;
using LastTradedPriceProcessing;
using OrderBookProcessing;
using Timer = System.Timers.Timer;

namespace BlazorLiquidity.Server.FairValueProcessing
{
   public interface IFairValuePricingUI : IFairValuePricing
   {
      Task Init();
   }
   
   class FairValuePriceTimer : Timer
   {
      public string _venue { get; set; }
      public string _coinPair { get; set; }
    
      public FairValuePriceTimer(string venue, string coinPair)
      {
         _venue = venue;
         _coinPair = coinPair;
      }
   }
   public class FairValuePricingUI : IFairValuePricingUI 
   {
      public event Action<string, string, decimal> OnFairValuePriceChange;
      
      //    public event Func<string, string, decimal, Task> OnFairValuePriceChangeAsync;
      private readonly ILogger<FairValuePricing> _logger;
      private readonly ILastTradedPriceHandler _lastTradedPriceHandler;
      private readonly IOrderBookProcessor _orderBookProcessor;
      private readonly IPortfolioRepository _portfolioRepository;
      private Dictionary<string, Dictionary<string, FairValueHolder>> _masterFairValues { get; set; } = new();
      private Dictionary<string, FairValueHolder> _fairValues = new Dictionary<string, FairValueHolder>();
      private object _fairValueLock = new object();
      private Dictionary<Tuple<string, string>, FairValueConfigForUiDTO> _fairValueConfigTable = new();
      private Dictionary<Tuple<string, string>, FairValueHolder> _fairValueDataTable = new();

      public FairValuePricingUI(ILastTradedPriceHandler lastTradeProcessing, IOrderBookProcessor orderBookProcessor, ILoggerFactory loggerFactory, IPortfolioRepository repository)
      {
         _logger = loggerFactory.CreateLogger<FairValuePricing>();
         _lastTradedPriceHandler = lastTradeProcessing;
         _lastTradedPriceHandler.OnLatestTradeUpdate += OnLatestTradeUpdate;
         _orderBookProcessor = orderBookProcessor;
         _portfolioRepository = repository;
         _orderBookProcessor.OnBestBidChange += OnBestBidChange;
         _orderBookProcessor.OnBestAskChange += OnBestAskChange;
      }

      public async Task Init()
      {
         var configs = await _portfolioRepository.GetFairValueConfigForUI();
         configs.ForEach(c =>
         {
            var pair = Tuple.Create(c.Venue.VenueName, c.CoinPair.Name);
            // Kick off a timer for this - as this will be used for high liquidity pairs only
            _fairValueDataTable.Add(pair, new FairValueHolder());
            var timer = new FairValuePriceTimer(c.Venue.VenueName, c.CoinPair.Name);
            timer.Elapsed += FairValueTimerExpired;
            timer.Interval = c.UpdateIntervalSecs * 1000;
            timer.Start();
         });
         _fairValueConfigTable = configs.ToDictionary(c => new Tuple<string, string>(c.Venue.VenueName, c.CoinPair.Name), c => c);
      }

      private void FairValueTimerExpired(object sender, System.Timers.ElapsedEventArgs e)
      {
         var data = (FairValuePriceTimer)sender;
         data.Stop();
         var pair = Tuple.Create(data._venue, data._coinPair);
         if (_fairValueDataTable.ContainsKey(pair))
         {
            var fairValueData = _fairValueDataTable[pair];
            OnFairValuePriceChange?.Invoke(data._venue, data._coinPair, fairValueData.FairValue);
         }
         data.Start();
      }

      private void OnBestAskChange(string venue, string symbol, decimal price, decimal quantity)
      {
      //   _logger.LogInformation("Best Ask Update from {Venue} for {Symbol} of {Price}",
            //                     venue, symbol, price);

         var fairValue = UpdateFairValue(venue, symbol, price, PriceToUpdate.Ask);
         var pair = Tuple.Create(venue, symbol);
         if (_fairValueDataTable.ContainsKey(pair))
         {
            _fairValueDataTable[pair] = fairValue;
            return;
         }
         else if (fairValue.IsFairValueValid)
         {
            OnFairValuePriceChange?.Invoke(venue, symbol, fairValue.FairValue);
         }
      }

      private void OnBestBidChange(string venue, string symbol, decimal price, decimal quantity)
      {
     //    _logger.LogInformation("Best Bid Update from {Venue} for {Symbol} of {Price}",
     //                            venue, symbol, price);
         var fairValue = UpdateFairValue(venue, symbol, price, PriceToUpdate.Bid);
         var pair = Tuple.Create(venue, symbol);
         if (_fairValueDataTable.ContainsKey(pair))
         {
            _fairValueDataTable[pair] = fairValue;
            return;
         }
         else if (fairValue.IsFairValueValid)
         {
            OnFairValuePriceChange?.Invoke(venue, symbol, fairValue.FairValue);
         }
      }

      private void OnLatestTradeUpdate(string venue, LatestTrade latestTradeData)
      {
  //       _logger.LogInformation("Last Traded Price Update from {Venue} for {Symbol} of {Price}",
      //                           venue, latestTradeData.Symbol, latestTradeData.Price);
         var fairValue = UpdateFairValue(venue, latestTradeData.Symbol, latestTradeData.Price, PriceToUpdate.Last);
         var pair = Tuple.Create(venue, latestTradeData.Symbol);
         if (_fairValueDataTable.ContainsKey(pair))
         {
            _fairValueDataTable[pair] = fairValue;
            return;
         }
         else if (fairValue.IsFairValueValid)
         {
            OnFairValuePriceChange?.Invoke(venue, latestTradeData.Symbol, fairValue.FairValue);
         }
      }

      private FairValueHolder UpdateFairValue(string venue, string symbol, decimal price, PriceToUpdate which)
      {
         lock (_fairValueLock)
         {
            Dictionary<string, FairValueHolder> entry = null;
            if (_masterFairValues.ContainsKey(venue))
               entry = _masterFairValues[venue];
            else
            {
               entry = new Dictionary<string, FairValueHolder>();
               _masterFairValues.Add(venue, entry);
            }
            FairValueHolder fairValue;
            if (entry.ContainsKey(symbol))
            {
               fairValue = entry[symbol];
            }
            else
            {
               fairValue = new FairValueHolder();
               entry.Add(symbol, fairValue);
            }

            switch (which)
            {
               case PriceToUpdate.Ask:
                  fairValue.BestAsk = price;
                  fairValue.IsAskValid = true;
                  break;
               case PriceToUpdate.Bid:
                  fairValue.BestBid = price;
                  fairValue.IsBidValid = true;
                  break;
               case PriceToUpdate.Last:
                  fairValue.LastTrade = price;
                  fairValue.IsLastTradeValid = true;
                  break;
            }
            fairValue.CalculateFairValue();
            // _logger.LogInformation("Fairvalue price is {FairValuePrice}", fairValue.FairValue);
            return fairValue;
         }
      }

      public decimal GetMaxOfOthers(string venue, string symbol)
      {
         throw new NotImplementedException();
      }

      public Task Init(MarketType mode)
      {
         throw new NotImplementedException();
      }

      public bool CalculateFairValue(FairValueHolder fairValue)
      {
         throw new NotImplementedException();
      }

      public void CalculateBestFairValue(string symbol, string venue, FairValueHolder fairValue)
      {
         throw new NotImplementedException();
      }

      FairValueHolder IFairValuePricing.UpdateFairValue(string venue, string symbol, decimal price, PriceToUpdate which)
      {
         throw new NotImplementedException();
      }

      public Task OnStrategyConfigChange(StrategyConfigChangeData arg)
      {
         throw new NotImplementedException();
      }

      public Task Init(MarketType mode, bool isMaker)
      {
         throw new NotImplementedException();
      }

      public void ChangeMakerTakerMode(StratgeyMode mode)
      {
         throw new NotImplementedException();
      }
   }
}
