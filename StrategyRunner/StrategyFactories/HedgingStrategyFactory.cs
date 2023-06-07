using Strategies;
using Strategies.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyRunner.StrategyFactories
{
   public interface IHedgingStrategyFactory
   {
      IStrategy CreateStrategy();
   }

   public class HedgingStrategyFactory : IHedgingStrategyFactory
   {
      private readonly IServiceProvider _serviceProvider;

      public HedgingStrategyFactory(IServiceProvider serviceProvider)
      {
         _serviceProvider = serviceProvider;
      }

      public IStrategy CreateStrategy()
      {
         return (IStrategy)_serviceProvider.GetService(typeof(IHedgerStrategy));
      }
   }
}
