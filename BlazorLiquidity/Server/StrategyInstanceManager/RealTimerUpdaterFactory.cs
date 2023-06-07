using System.Collections.Concurrent;
using AccountBalanceManager;
using BlazorLiquidity.Server.RealTime;
using Common.Messages;
using OrderAndTradeProcessing;

namespace BlazorLiquidity.Server.StrategyInstanceManager
{
   public interface IRealTimerUpdaterFactory
   {
      IRealTimeUpdater CreateRealTimeUpdaterInstance(IInventoryManager accountInventoryManager, IOrderAndTradeProcessing orderAndTradeProcessing);
   }

   public class RealTimerUpdaterFactory : IRealTimerUpdaterFactory
   {
      private readonly IServiceProvider _serviceProvider;
      private readonly ILogger<StrategyInstanceFactory> _logger;
      public RealTimerUpdaterFactory(IServiceProvider serviceProvider,
         ILoggerFactory loggerFactory)
      {
         _logger = loggerFactory.CreateLogger<StrategyInstanceFactory>();
         _serviceProvider = serviceProvider;
      }

      public  IRealTimeUpdater CreateRealTimeUpdaterInstance(IInventoryManager accountInventoryManager, IOrderAndTradeProcessing orderAndTradeProcessing)
      {
         try
         {
            var instance = (IRealTimeUpdater)_serviceProvider.GetService(typeof(IRealTimeUpdater));
            if (instance != null)
            {
               instance.Init(accountInventoryManager, orderAndTradeProcessing);
               return instance;
            }
            return null;
         }
         catch (Exception e)
         {
            _logger.LogError("Unable to create real time updater instance in server ");
            throw;
         }
      }
   }
}
