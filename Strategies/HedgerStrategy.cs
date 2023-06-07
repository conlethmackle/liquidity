using Strategies.Common;
using AccountBalanceManager;
using ClientConnections;
using DataStore;
using Microsoft.Extensions.Logging;
using OrderAndTradeProcessing;
using OrderBookProcessing;
using Common.Models;
using MessageBroker;
using StrategyMessageListener;
using ConnectorStatus;
using FairValueProcessing;
using DynamicConfigHandling;

namespace Strategies
{
   public interface IHedgerStrategy
   {

   }

   public class HedgerStrategy : BaseStrategy, IHedgerStrategy
   {
      public HedgerStrategy(ILoggerFactory loggerFactory,
                                  IInventoryManager inventoryManager,
                                  IOrderAndTradeProcessing orderTradeProcessing,
                                  IOrderBookProcessor orderBookProcessor,
                                  IFairValuePricing fairValuePricing,
                                  IPortfolioRepository repository,
                                  IPrivateClientConnections privateClientConnections,
                                  IPublicClientConnections publicClientConnections,
                                  IMessageBroker messageBroker,
                                  IMessageReceiver messageReceiver,
                                  IConnectorStatusListener connectionListener,
                                  StrategyStartConfig startupConfig,
                                  IDynamicConfigUpdater dynamicConfigUpdater) :
          base(loggerFactory,
               inventoryManager,
               orderTradeProcessing,
               orderBookProcessor,
               fairValuePricing,
               repository,
               privateClientConnections,
               publicClientConnections,
               messageBroker,
               messageReceiver,
               connectionListener,
               startupConfig,
               dynamicConfigUpdater)
      {
      
      }

      public override Task StrategyInit()
      {
         throw new NotImplementedException();
      }

      public override void OnCancelStrategyOrders()
      {

      }

      public override void OnCheckStaleOrdersPending()
      {

      }

      public override void OnFairValueUpdate()
      {

      }

      public override void OnOrderStatusUpdate()
      {

      }
   }
}
