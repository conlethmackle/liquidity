using Strategies;
using Strategies.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyRunner.StrategyFactories
{
   

   public interface IFairValueLiquidationStrategyFactory
   {
      IStrategy CreateStrategy();
   }

   public class FairValueLiquidationStrategyFactory : IFairValueLiquidationStrategyFactory
   {
      private readonly IServiceProvider _serviceProvider;

      public FairValueLiquidationStrategyFactory(IServiceProvider serviceProvider)
      {
         _serviceProvider = serviceProvider;
      }

      public IStrategy CreateStrategy()
      {
         return (IStrategy)_serviceProvider.GetService(typeof(IFairValueLiquidationStrategy));
      }
   }
}
