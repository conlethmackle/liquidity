using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Messages;
using Common.Models;
using Common.Models.DTOs;
using DataStore;
using DynamicConfigHandling;
using Microsoft.Extensions.Logging;
using OrderBookProcessing;

namespace FairValueProcessing
{
   public class BidMakerTakerFairValue : FairValueBase
   {
      public override event Action<string, string, decimal> OnFairValuePriceChange;
      private readonly ILogger<BidMakerTakerFairValue> _logger;
      private readonly IOrderBookProcessor _orderBookProcessor;
      private readonly IPortfolioRepository _portfolioRepository;
      private Dictionary<string, MakerTakerFeeDTO> _makeTakerFees = new ();
      private bool _isMaker = false;
      private MarketType _mode;

      public BidMakerTakerFairValue(IOrderBookProcessor orderBookProcessor, IPortfolioRepository portfolioRepository, ILoggerFactory loggerFactory, IDynamicConfigUpdater dynamicConfigUpdater) : 
               base(orderBookProcessor, dynamicConfigUpdater, loggerFactory)
      {
         _logger = loggerFactory.CreateLogger<BidMakerTakerFairValue>();
         _portfolioRepository = portfolioRepository;
         _orderBookProcessor = orderBookProcessor;
         _orderBookProcessor.OnBestBidChange += OnBestBidChange;
         _orderBookProcessor.OnBestAskChange += OnBestAskChange;
      }

      public override async Task Init(MarketType mode, bool IsMaker=true)
      {
         _mode = mode;
         _isMaker = IsMaker;
         var allFees = await _portfolioRepository.GetMakerTakerFees();
         _makeTakerFees = allFees.Where(f => f.Mode == mode).ToDictionary(x => x.Venue.VenueName, x => x);
      }

      public override void OnBestAskChange(string venue, string symbol, decimal price, decimal quantity)
      {
        
      }

      public override void OnBestBidChange(string venue, string symbol, decimal price, decimal quantity)
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

      public override bool CalculateFairValue(FairValueHolder fairValue)
      {
         if (fairValue.IsBidValid)
            fairValue.IsFairValueValid = true;
         if (_makeTakerFees.ContainsKey(fairValue.Venue))
         {
            if (_isMaker)
               fairValue.FairValue = (fairValue.BestBid + fairValue.BestBid * _makeTakerFees[fairValue.Venue].MakerPercentage/100);
            else
               fairValue.FairValue = (fairValue.BestBid + fairValue.BestBid * _makeTakerFees[fairValue.Venue].TakerPercentage/100);
         }
         return fairValue.IsFairValueValid;
      }

      public async override Task OnStrategyConfigChange(StrategyConfigChangeData arg)
      {
         if (arg.StrategyConfigChangeType == StrategyConfigChangeType.FAIRVALUEMAKERTAKER)
         {
            await ReadConfig(_mode);
         }
      }

      private async Task ReadConfig(MarketType mode)
      {
         var allFees = await _portfolioRepository.GetMakerTakerFees();
         _makeTakerFees = allFees.Where(f => f.Mode == mode).ToDictionary(x => x.Venue.VenueName, x => x);
      }

      public override void ChangeMakerTakerMode(StratgeyMode mode)
      {
         if (mode == StratgeyMode.MAKER)
            _isMaker = true;
         else
            _isMaker = false;
      }
   }
}
