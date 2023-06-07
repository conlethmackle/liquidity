using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Strategies;
using Strategies.Common;

namespace StrategyRunner.StrategyFactories
{
   public interface IMultiFairValueLiquidationStrategyFactory
   {
      IStrategy CreateStrategy();
   }

   public class MultiFairValueLiquidationStrategyFactory : IMultiFairValueLiquidationStrategyFactory
   {
      private readonly IServiceProvider _serviceProvider;

      public MultiFairValueLiquidationStrategyFactory(IServiceProvider serviceProvider)
      {
         _serviceProvider = serviceProvider;
      }

      public IStrategy CreateStrategy()
      {
         return (IStrategy)_serviceProvider.GetService(typeof(IMultiVenueFairValueLiquidation));
      }
   }
}
