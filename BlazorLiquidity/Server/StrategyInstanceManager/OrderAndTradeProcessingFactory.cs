using AccountBalanceManager;
using OrderAndTradeProcessing;

namespace BlazorLiquidity.Server.StrategyInstanceManager
{
   public interface IOrderAndTradeProcessingFactory
   {
      IOrderAndTradeProcessing CreateOrderAndTradeProcessingInstance();
   }

   public class OrderAndTradeProcessingFactory : IOrderAndTradeProcessingFactory
   {
      private readonly IServiceProvider _serviceProvider;
      private readonly ILogger<StrategyInstanceFactory> _logger;

      public OrderAndTradeProcessingFactory(IServiceProvider serviceProvider,
         ILoggerFactory loggerFactory)
      {
         _logger = loggerFactory.CreateLogger<StrategyInstanceFactory>();
         _serviceProvider = serviceProvider;
      }

      public IOrderAndTradeProcessing CreateOrderAndTradeProcessingInstance()
      {
         try
         {
            var instance = (IOrderAndTradeProcessing)_serviceProvider.GetService(typeof(IOrderAndTradeProcessing));
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
