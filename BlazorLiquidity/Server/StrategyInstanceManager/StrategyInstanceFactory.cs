using System.Collections.Concurrent;
using Common.Messages;

namespace BlazorLiquidity.Server.StrategyInstanceManager
{
    
   public interface IStrategyInstanceFactory
   {
      IStrategyInstance CreateStrategyInstance(string spName, string instanceName, int configId, BlockingCollection<MessageBusReponse> msgQueueData);
   }

   public class StrategyInstanceFactory : IStrategyInstanceFactory
   {
      private readonly IServiceProvider _serviceProvider;
      private readonly ILogger<StrategyInstanceFactory> _logger;
      public StrategyInstanceFactory(IServiceProvider serviceProvider,
         ILoggerFactory loggerFactory)
      {
         _logger = loggerFactory.CreateLogger<StrategyInstanceFactory>();
         _serviceProvider = serviceProvider;
      }

      public IStrategyInstance CreateStrategyInstance(string spName, string instanceName, int configId, BlockingCollection<MessageBusReponse> msgDataQueue)
      {
         var instance = (IStrategyInstance)_serviceProvider.GetService(typeof(IStrategyInstance));
         if (instance != null)
         {
           // await instance.Init(spName, instanceName, configId, msgDataQueue);
            return instance;
         }

         _logger.LogError("Unable to create strategy instance in server for {SPName} and {Instance}", 
            spName, instanceName);
         throw new ApplicationException($"Unable to create strategy instance in server for {spName} and {instanceName}");
      }
   }
}
