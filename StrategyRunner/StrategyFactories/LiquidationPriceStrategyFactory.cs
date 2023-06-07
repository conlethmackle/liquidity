using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Strategies;
using Strategies.Common;

namespace StrategyRunner.StrategyFactories
{

   public interface ILiquidationPriceStrategyFactory
   {
      IStrategy CreateStrategy();
   }

   public class LiquidationPriceStrategyFactory : ILiquidationPriceStrategyFactory
   {
      private readonly IServiceProvider _serviceProvider;

      public LiquidationPriceStrategyFactory(IServiceProvider serviceProvider)
      {
         _serviceProvider = serviceProvider;
      }

      public IStrategy CreateStrategy()
      {
         return (IStrategy)_serviceProvider.GetService(typeof(ILiquidationPriceStrategy));
      }
   }
}
