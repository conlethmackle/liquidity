using AccountBalanceManager;
using BlazorLiquidity.Server.RealTime;
using OrderAndTradeProcessing;

namespace BlazorLiquidity.Server.StrategyInstanceManager
{
   public interface IInventoryFactory
   {
      IInventoryManager CreateInventoryInstance();
   }

   public class InventoryFactory : IInventoryFactory
   {
      private readonly IServiceProvider _serviceProvider;
      private readonly ILogger<StrategyInstanceFactory> _logger;

      public InventoryFactory(IServiceProvider serviceProvider,
                              ILoggerFactory loggerFactory)
      {
         _logger = loggerFactory.CreateLogger<StrategyInstanceFactory>();
         _serviceProvider = serviceProvider;
      }

      public IInventoryManager CreateInventoryInstance()
      {
         try
         {
            var instance = (IInventoryManager)_serviceProvider.GetService(typeof(IInventoryManager));
            if (instance != null)
            {
               return instance;
            }
            return null;
         }
         catch (Exception e)
         {
            _logger.LogError("Unable to create inventory instance in server ");
            throw;
         }
      }
   }
}
