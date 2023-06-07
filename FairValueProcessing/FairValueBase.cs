using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Messages;
using Common.Models;
using DynamicConfigHandling;
using LastTradedPriceProcessing;
using Microsoft.Extensions.Logging;
using OrderBookProcessing;

namespace FairValueProcessing
{
   public abstract class FairValueBase : IFairValuePricing
   {
      public virtual event Action<string, string, decimal> OnFairValuePriceChange;
      private readonly ILogger<FairValueBase> _logger;
      private readonly ILastTradedPriceHandler _lastTradedPriceHandler;
      private readonly IOrderBookProcessor _orderBookProcessor;
      private readonly IDynamicConfigUpdater _dynamicConfigUpdater;

      protected Dictionary<string, Dictionary<string, FairValueHolder>> _fairValueBySymbol = new();
      protected object _fairValueLock = new object();
      private FairValueHolder _bestFairValue = new();

      public FairValueBase(IOrderBookProcessor orderBookProcessor, IDynamicConfigUpdater dynamicConfigUpdater, ILoggerFactory loggerFactory)
      {
         _logger = loggerFactory.CreateLogger<FairValueBase>();
         _orderBookProcessor = orderBookProcessor;
         _dynamicConfigUpdater = dynamicConfigUpdater;
         _dynamicConfigUpdater.OnStrategyConfigChange += OnStrategyConfigChange;
      }

      public async virtual Task OnStrategyConfigChange(StrategyConfigChangeData arg)
      {
         
      }

      public decimal GetMaxOfOthers(string venue, string symbol)
      {
         decimal highestOtherFairValue = 0;
         var highestOtherFairValueList = _fairValueBySymbol[symbol].Values.Where(x => !x.IsBest).ToList();
         if (highestOtherFairValueList.Any())
         {
            highestOtherFairValue = highestOtherFairValueList.Max(x => x.FairValue);
         }
            
         return highestOtherFairValue;
      }

      public virtual async Task Init(MarketType mode, bool isMaker)
      {
         
      }

      public virtual void OnBestAskChange(string venue, string symbol, decimal price, decimal quantity)
      {
         //   _logger.LogInformation("Best Ask Update from {Venue} for {Symbol} of {Price}",
         //                     venue, symbol, price);

         var fairValue = UpdateFairValue(venue, symbol, price, PriceToUpdate.Ask);
         if (fairValue.IsBest)
         {
            OnFairValuePriceChange?.Invoke(venue, symbol, fairValue.FairValue);
            // OnFairValuePriceChangeAsync?.Invoke(venue, symbol, fairValue.FairValue);
         }
      }

      public virtual void OnBestBidChange(string venue, string symbol, decimal price, decimal quantity)
      {
         //  _logger.LogInformation("Best Bid Update from {Venue} for {Symbol} of {Price}",
         //                        venue, symbol, price);
         var fairValue = UpdateFairValue(venue, symbol, price, PriceToUpdate.Bid);
         if (fairValue.IsBest)
         {
            OnFairValuePriceChange?.Invoke(venue, symbol, fairValue.FairValue);
            //OnFairValuePriceChangeAsync?.Invoke(venue, symbol, fairValue.FairValue);
         }
      }

      public FairValueHolder UpdateFairValue(string venue, string symbol, decimal price, PriceToUpdate which)
      {
         lock (_fairValueLock)
         {
            Dictionary<string, FairValueHolder> entry = null;
            if (_fairValueBySymbol.ContainsKey(symbol))
               entry = _fairValueBySymbol[symbol];
            else
            {
               entry = new Dictionary<string, FairValueHolder>();
               _fairValueBySymbol.Add(symbol, entry);
            }
            FairValueHolder fairValue;
            if (entry.ContainsKey(venue))
            {
               fairValue = entry[venue];
            }
            else
            {
               fairValue = new FairValueHolder();
               fairValue.Venue = venue;
               
               entry.Add(venue, fairValue);
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

            CalculateFairValue(fairValue);
            CalculateBestFairValue(symbol, venue, fairValue);
            // _logger.LogInformation("Fairvalue price is {FairValuePrice}", fairValue.FairValue);
            return fairValue;
         }
      }

      public virtual bool CalculateFairValue(FairValueHolder fairValue)
      {
         if (fairValue.IsBidValid && fairValue.IsAskValid && fairValue.IsLastTradeValid)
            fairValue.IsFairValueValid = true;
         fairValue.FairValue = (fairValue.BestBid + fairValue.BestAsk + fairValue.LastTrade) / 3;
         return fairValue.IsFairValueValid;
      }

      public virtual void CalculateBestFairValue(string symbol, string venue, FairValueHolder fairValue)
      {
         if (fairValue.IsFairValueValid)
         {
            if (fairValue.FairValue > _bestFairValue.FairValue)
            {
               _bestFairValue.IsBest = false;
               _bestFairValue = fairValue;
               _bestFairValue.IsBest = true;
            }
         }
      }

      public abstract void ChangeMakerTakerMode(StratgeyMode mode);
   }
}
