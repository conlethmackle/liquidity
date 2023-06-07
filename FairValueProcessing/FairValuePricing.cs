using Common.Messages;
using LastTradedPriceProcessing;
using Microsoft.Extensions.Logging;
using OrderBookProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models;
using DynamicConfigHandling;

namespace FairValueProcessing
{
   public interface IFairValuePricing
   {
      event Action<string, string, decimal> OnFairValuePriceChange;
      decimal GetMaxOfOthers(string venue, string symbol);
      Task Init(MarketType mode, bool isMaker);
      void ChangeMakerTakerMode(StratgeyMode mode);

      bool CalculateFairValue(FairValueHolder fairValue);

      void CalculateBestFairValue(string symbol, string venue, FairValueHolder fairValue);

      FairValueHolder UpdateFairValue(string venue, string symbol, decimal price, PriceToUpdate which);

      Task OnStrategyConfigChange(StrategyConfigChangeData arg);
      //event Func<string, string, decimal, Task> OnFairValuePriceChangeAsync;
   }

   public enum PriceToUpdate
   {
      Bid = 1,
      Ask = 2,
      Last = 3
   }

   public  class FairValuePricing : FairValueBase
   {
      public override event Action<string, string, decimal> OnFairValuePriceChange;
  //    public event Func<string, string, decimal, Task> OnFairValuePriceChangeAsync;
      private readonly ILogger<FairValuePricing> _logger;
      private readonly ILastTradedPriceHandler _lastTradedPriceHandler;
      private readonly IOrderBookProcessor _orderBookProcessor;
      private FairValueHolder _bestFairValue = new ();
      
      public FairValuePricing(ILastTradedPriceHandler lastTradeProcessing, IOrderBookProcessor orderBookProcessor, ILoggerFactory loggerFactory, IDynamicConfigUpdater dynamicConfigUpdater) : 
         base(orderBookProcessor,  dynamicConfigUpdater, loggerFactory)
      {
         _logger = loggerFactory.CreateLogger<FairValuePricing>();
         _lastTradedPriceHandler = lastTradeProcessing;
         _lastTradedPriceHandler.OnLatestTradeUpdate += OnLatestTradeUpdate;
         _orderBookProcessor = orderBookProcessor;
         _orderBookProcessor.OnBestBidChange += OnBestBidChange;
         _orderBookProcessor.OnBestAskChange += OnBestAskChange;
         _bestFairValue.FairValue = 0;
      }

      private void OnLatestTradeUpdate(string venue, LatestTrade latestTradeData)
      {
      //   _logger.LogInformation("Last Traded Price Update from {Venue} for {Symbol} of {Price}",
       //                          venue, latestTradeData.Symbol, latestTradeData.Price);
         var fairValue = UpdateFairValue(venue, latestTradeData.Symbol, latestTradeData.Price, PriceToUpdate.Last);
         if (fairValue.IsBest)
         {
            OnFairValuePriceChange?.Invoke(venue, latestTradeData.Symbol, fairValue.FairValue);
            //OnFairValuePriceChangeAsync?.Invoke(venue, latestTradeData.Symbol, fairValue.FairValue);
         }
      }

      public override void CalculateBestFairValue(string symbol, string venue, FairValueHolder fairValue)
      {
         if (fairValue.IsFairValueValid)
         {
            if (fairValue.FairValue > _bestFairValue.FairValue)
            {
               _bestFairValue.IsBest = false;
               _bestFairValue = fairValue;
               fairValue.IsBest = true;
            }
         }
      }

      public override void ChangeMakerTakerMode(StratgeyMode mode)
      {
        
      }
   }
}