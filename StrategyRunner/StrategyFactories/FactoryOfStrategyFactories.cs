using Microsoft.Extensions.Logging;
using Strategies.Common;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyRunner.StrategyFactories
{
   public interface IFactoryOfStrategyFactories
   {
      IStrategy CreateStrategy(string strategyName);
   }

   public class FactoryOfStrategyFactories : IFactoryOfStrategyFactories
   {
      private readonly ILogger<FactoryOfStrategyFactories> _logger;
      private readonly IHedgingStrategyFactory _hedgingFactory;
      private readonly IMarketMakingStrategyFactory _marketMakingStrategyFactory;
      private readonly IFairValueLiquidationStrategyFactory _fairValueLiquidationStrategyFactory;
      private readonly IMultiFairValueLiquidationStrategyFactory _multiFairValueLiquidationStrategyFactory;
      private readonly ILiquidationPriceStrategyFactory _liquidationPriceStrategyFactory;
      public FactoryOfStrategyFactories(ILoggerFactory loggerFactory,
         IHedgingStrategyFactory hedgingFactory,
         IMarketMakingStrategyFactory marketMakingFactory,
         IFairValueLiquidationStrategyFactory fairValueLiquidationStrategyFactory,
         IMultiFairValueLiquidationStrategyFactory multiFairValueLiquidationStrategyFactory,
         ILiquidationPriceStrategyFactory liquidationPriceStrategy)
      {
         _logger = loggerFactory.CreateLogger<FactoryOfStrategyFactories>();
         _hedgingFactory = hedgingFactory;
         _marketMakingStrategyFactory = marketMakingFactory;
         _fairValueLiquidationStrategyFactory = fairValueLiquidationStrategyFactory;
         _multiFairValueLiquidationStrategyFactory = multiFairValueLiquidationStrategyFactory;
         _liquidationPriceStrategyFactory = liquidationPriceStrategy;
      }

      public IStrategy CreateStrategy(string strategyName)
      {
         switch(strategyName)
         {
            case StrategyNames.Hedging:
               return _hedgingFactory.CreateStrategy();               
            case StrategyNames.MarketMaking:
               return _marketMakingStrategyFactory.CreateStrategy();
            case StrategyNames.FairValueLiquidation:
               return _fairValueLiquidationStrategyFactory.CreateStrategy();
            case StrategyNames.MultiVenueFairValueLiquidation:
               return _multiFairValueLiquidationStrategyFactory.CreateStrategy();
            case StrategyNames.LiquidationPriceStrategy:
               return _liquidationPriceStrategyFactory.CreateStrategy();
            default:
               throw new ArgumentOutOfRangeException(nameof(strategyName));           
         }
      }
   }
}
