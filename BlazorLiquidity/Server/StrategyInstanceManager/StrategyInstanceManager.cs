using BlazorLiquidity.Server.Hubs;
using BlazorLiquidity.Server.Receiver;
using ClientConnections;
using Common.Messages;
using ConnectorStatus;
using DataStore;
using FairValueProcessing;
using MessageBroker;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Data.Entity.Core.Mapping;

namespace BlazorLiquidity.Server.StrategyInstanceManager
{
   public interface IStrategyInstanceManager
   {
      //  void CreateStrategyInstance(string InstanceName, Queue<InstanceQueueData> processorQueue);
      Tuple<IStrategyInstance, BlockingCollection<MessageBusReponse>> CreateStrategyInstance(string spName, string InstanceName, int configId);
      void ForwardMessage(MessageBusReponse message, string instanceName);
      Task StartRealTimeUpdaterForInstance(string instanceName, int configId);
      IStrategyInstance GetStrategyInstance(string instanceName);
   }

   
   public class StrategyInstanceManagerServer : IStrategyInstanceManager
   {
      private readonly ILogger<StrategyInstanceManagerServer> _logger;
    //  private readonly IInventoryFactory _inventoryFactory;
   //   private readonly IOrderAndTradeProcessingFactory _orderAndTradeProcessingFactory;
    //  private readonly IRealtimeUpdaterFactory _realtimeUpdaterFactory;
      private readonly IStrategyInstanceFactory _strategyInstanceFactory;
      private ConcurrentDictionary<string, Tuple<IStrategyInstance, BlockingCollection<MessageBusReponse>>> _strategyInstances = new ConcurrentDictionary<string, Tuple<IStrategyInstance, BlockingCollection<MessageBusReponse>>>();
      private object _lock = new object();
      public StrategyInstanceManagerServer(ILoggerFactory loggerFactory, 
                                     IStrategyInstanceFactory strategyInstanceFactory
                                  //   IInventoryFactory inventoryFactory, 
                                 //    IOrderAndTradeProcessingFactory orderAndTradeProcessingFactory, 
                                //     IRealtimeUpdaterFactory realtimeUpdaterFactory
                                     )
      {
         _logger = loggerFactory.CreateLogger<StrategyInstanceManagerServer>();
         _strategyInstanceFactory = strategyInstanceFactory;
        // _inventoryFactory = inventoryFactory;
       //  _orderAndTradeProcessingFactory = orderAndTradeProcessingFactory;
       //  _realtimeUpdaterFactory = realtimeUpdaterFactory;
      }

      

      public Tuple<IStrategyInstance, BlockingCollection<MessageBusReponse>> CreateStrategyInstance(string spName, string instanceName, int configId)
      {
         try
         {
            var blockingQueue = new BlockingCollection<MessageBusReponse>();
            var strategyInstance =
                _strategyInstanceFactory.CreateStrategyInstance(spName, instanceName, configId, blockingQueue);

            if (!_strategyInstances.ContainsKey(instanceName))
            {
               var tuple = new Tuple<IStrategyInstance, BlockingCollection<MessageBusReponse>>(strategyInstance,
                  blockingQueue);
               _strategyInstances.TryAdd(instanceName, tuple);
               return tuple;
               // strategyInstance.Run();
            }
            else
            {
               return _strategyInstances[instanceName];
            }
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Error Creating StrategyInstance {Error}", e.Message);
            throw;
         }

      }

      public IStrategyInstance GetStrategyInstance(string instanceName)
      {
         lock (_lock)
         {
            if (_strategyInstances.ContainsKey(instanceName))
            {

               return _strategyInstances[instanceName].Item1;
            }

            _logger.LogError("No Strategy Instance for {Instance}", instanceName);
            return null;
         }
      }

      public void ForwardMessage(MessageBusReponse message, string instanceName)
      {
         if (_strategyInstances.ContainsKey(instanceName))
         {
            var strategyInstanceAndQueue = _strategyInstances[instanceName];
            var queue = strategyInstanceAndQueue.Item2;
            queue.Add(message);
         }
      }

      public async Task StartRealTimeUpdaterForInstance(string instanceName, int configId)
      {
         if (!_strategyInstances.ContainsKey(instanceName))
         {

            var strategyInstanceAndQueue = _strategyInstances[instanceName];
            var strategyInstance = strategyInstanceAndQueue.Item1;
           // await strategyInstance.InitRealTimeUpdater(configId);
         }
      }
   }
}
