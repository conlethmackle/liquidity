using AccountBalanceManager;
using ClientConnections;
using Common.Models;
using ConnectorStatus;
using DataStore;
using DynamicConfigHandling;
using FairValueProcessing;
using MessageBroker;
using Microsoft.Extensions.Logging;
using OrderAndTradeProcessing;
using OrderBookProcessing;
using Strategies.Common;
using StrategyMessageListener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategies
{
   public interface IMarketMakingStrategy
   { 

   }

   public class MarketMakingStrategy : BaseStrategy, IMarketMakingStrategy
   {
      public MarketMakingStrategy(ILoggerFactory loggerFactory, 
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
