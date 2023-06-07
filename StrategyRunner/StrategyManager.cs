using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StrategyRunner.StrategyFactories;
using Strategies.Common;
using Common.Models;

namespace StrategyRunner
{
   public interface IStrategyManager
   {
      void InitialiseWithStrategy();
   }

   public class StrategyManager : IStrategyManager
   {          
      private IStrategy _strategy { get; set; }
      private readonly ILogger<StrategyManager> _logger;
      private readonly IFactoryOfStrategyFactories _factoryOfStrategyFactories;
      private readonly string _account;
      private readonly string _strategyName;
      private readonly string _configName;

      public StrategyManager(ILoggerFactory loggerFactory,
         IFactoryOfStrategyFactories factoryOfFactories, StrategyStartConfig config)
      {
         _logger = loggerFactory.CreateLogger<StrategyManager>();  
         _factoryOfStrategyFactories = factoryOfFactories;
         _account = config.Account;
         _strategyName = config.Strategy;
         _configName = config.ConfigName;
         _logger.LogInformation("In StrategyManager ...");       
      }

      public void InitialiseWithStrategy()
      {
         try
         {
            _logger.LogInformation("In StrategyManager.InitialiseWithStrategy ...");
            _strategy = _factoryOfStrategyFactories.CreateStrategy(_strategyName);
            _strategy.Initialise();
         }
         catch(Exception e)
         {
            _logger.LogError(e, "Error creating Strategy {Strategy} for {Account} with config {Config} {Error}", _strategyName, _account, _configName, e.Message);
            throw;
         }
      }
   }
}
