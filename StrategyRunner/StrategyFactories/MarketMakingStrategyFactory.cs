using Strategies;
using Strategies.Common;

namespace StrategyRunner.StrategyFactories
{
  
   public interface IMarketMakingStrategyFactory
   {
      IStrategy CreateStrategy();
   }

   public class MarketMakingStrategyFactory : IMarketMakingStrategyFactory
   {
      private readonly IServiceProvider _serviceProvider;

      public MarketMakingStrategyFactory(IServiceProvider serviceProvider)
      {
         _serviceProvider = serviceProvider;
      }

      public IStrategy CreateStrategy()
      {
         return (IStrategy)_serviceProvider.GetService(typeof(IMarketMakingStrategy));
      }
   }
}
