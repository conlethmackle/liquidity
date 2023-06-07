using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Sinks.SystemConsole;
using Log = Serilog.Log;
using System.Net.Http;
using Polly;

using Microsoft.Extensions.DependencyInjection;

using MessageBroker.Configuration;
using System.Threading.Tasks;
using MessageBroker;
using OrderAndTradeProcessing;
using OrderLedger;
using OrderBookProcessing;
using DataStore;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Common;
using StrategyRunner.StrategyFactories;
using Strategies.Common;
using AccountBalanceManager;
using Amazon;
using ClientConnections;
using Common.Models;
using Common.Utilities;
using Strategies;
using ConnectorStatus;
using CustomerIdAllocation;
using StrategyMessageListener;
using FairValueProcessing;
using LastTradedPriceProcessing;
using DynamicConfigHandling;

using TelegramAlertsApi;

namespace StrategyRunner
{
   internal class Program
   {
      private static IConfigurationRoot Configuration { get; set; }
      private static string _environmentVariable;
      private static Dictionary<string, string> _cmdLineArgs = new Dictionary<string, string>();

      static async Task Main(string[] args)
      {
         try
         {            
            string configName = "";
            bool showhelp = false;
          //  Console.WriteLine("Starting Strategy process...");
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = Thread.CurrentThread.CurrentCulture;
            _environmentVariable = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)           
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .AddSecretsManager(region: RegionEndpoint.EUWest2,
               configurator: options =>
               {
                  options.SecretFilter = entry => entry.Name.Contains("Liquidity");
                  options.KeyGenerator = (_, s) => s
                     .Replace("__", ":");
                  options.PollingInterval = TimeSpan.FromHours(1);
               })
            .Build();

            KeyInitialiser.Initialize(Configuration);
            CheckForLoggingAndStuff(args);
            //Console.WriteLine("Starting Strategy process...2");

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Log.Logger.Information("**************************** The Log directory is {Dir} **********************************", Configuration["LogDirectory"]);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var strategyManager = serviceProvider.GetService<IStrategyManager>();
            var messageReceiver = serviceProvider.GetService<IMessageReceiver>();
            strategyManager.InitialiseWithStrategy();
            await messageReceiver.Start();
           
            await Task.Run(() => { while (true) { Thread.Sleep(300); } });
         }
         catch(Exception e)
         {
            Console.WriteLine($"Fatal Error {e.StackTrace}");
            Log.Logger.Error(e, "Fatal Error has occurred {StackTrace}", e.StackTrace);
         }
      }

      private static void CheckForLoggingAndStuff(string[] args)
      {
         foreach (var arg in args)
         {
            var parts = arg.Split("=");
            if (parts.Length != 2)
            {
               throw new Exception("Incorrectly formatted command line arguments");
            }
            _cmdLineArgs.Add(parts[0], parts[1]);
         }

         if (!_cmdLineArgs.ContainsKey("Strategy"))
         {
            throw new Exception("No Strategy argument specified");
         }

         if (!_cmdLineArgs.ContainsKey("Account"))
         {
            throw new Exception("No Account argument specified");
         }

         if (!_cmdLineArgs.ContainsKey("ConfigName"))
         {
            throw new Exception("No ConfigName argument specified");
         }
      }

      private static void ConfigureServices(IServiceCollection services)
      {
         var config = new MapperConfiguration(cfg =>
         {
            cfg.AddProfile(new AutoMapperProfile());
         });
         var mapper = config.CreateMapper();
         bool fileLogging = false;
         bool consoleLogging = true;

         if (_cmdLineArgs.ContainsKey("FileLogging"))
         {
            var val = _cmdLineArgs["FileLogging"];
            if (val.Equals("On"))
               fileLogging = true;
         }

         if (_cmdLineArgs.ContainsKey("ConsoleLogging"))
         {
            var val = _cmdLineArgs["ConsoleLogging"];
            if (val.Equals("Off"))
               consoleLogging = false;
         }

         if (fileLogging && consoleLogging)
         {
            Log.Logger = new LoggerConfiguration()
               .WriteTo.Console()
               .WriteTo.File(Configuration["LogDirectory"] + "log_" + _cmdLineArgs["ConfigName"] + ".txt",
                  rollingInterval: RollingInterval.Hour,
                  flushToDiskInterval: TimeSpan.FromSeconds(5))
               .CreateLogger();
         }
         else if (fileLogging && !consoleLogging)
         {
            Log.Logger = new LoggerConfiguration()
               
               .WriteTo.File(Configuration["LogDirectory"] + "log_" + _cmdLineArgs["ConfigName"] + ".txt",
                  rollingInterval: RollingInterval.Hour,
                  flushToDiskInterval: TimeSpan.FromSeconds(5))
               .CreateLogger();
         }
         else
         {
            Log.Logger = new LoggerConfiguration()
               .WriteTo.Console()
               .CreateLogger();
         }

         var connString = Configuration.GetConnectionString("Liquidity");

         services.AddLogging(x => x.AddSerilog(Log.Logger))
            .AddSingleton<StrategyStartConfig>()
            .AddSingleton<IFactoryOfStrategyFactories, FactoryOfStrategyFactories>()
            .AddSingleton<IHedgingStrategyFactory, HedgingStrategyFactory>()
            .AddSingleton<IMarketMakingStrategyFactory, MarketMakingStrategyFactory>()
            .AddSingleton<IFairValueLiquidationStrategyFactory, FairValueLiquidationStrategyFactory>()
            .AddSingleton<IMultiFairValueLiquidationStrategyFactory, MultiFairValueLiquidationStrategyFactory>()
            .AddSingleton<ILiquidationPriceStrategyFactory, LiquidationPriceStrategyFactory>()
            .AddSingleton<IHedgerStrategy, HedgerStrategy>()
            .AddSingleton<IMarketMakingStrategy, MarketMakingStrategy>()
            .AddSingleton<IFairValueLiquidationStrategy, FairValueLiquidationStrategy>()
            .AddSingleton<IMultiVenueFairValueLiquidation, MultiVenueFairValueLiquidation>()
            .AddSingleton<ILiquidationPriceStrategy, LiquidationPriceStrategy>()
            .AddSingleton<IMessageReceiver, MessageReceiver>()
            .AddSingleton<IStrategyManager, StrategyManager>()
            .AddSingleton<IOrderAndTradeProcessing, OrderTradeProcessing>()
            .AddTransient<IOrderPlacementHolder, OrderPlacementHolder>()
            .AddSingleton<IOrderPlacementHolderFactory, OrderPlacementHolderFactory>()
            .AddSingleton<IOrderBookProcessor, OrderBookProcessor>()
            .AddSingleton<IFairValuePricing, BidMakerTakerFairValue>()
            .AddSingleton<ILastTradedPriceHandler, LastTradedPriceHandler>()            
            .AddSingleton<IMessageBroker, RabbitMQBroker>()
            .AddSingleton<IOrderLedgerProcessor, OrderLedgerProcessor>()
            .AddSingleton<IPortfolioRepository, PortfolioRepository>()
            .AddSingleton<IInventoryManager, InventoryManager>()
            .AddSingleton<IPrivateClientConnections, PrivateClientConnections>()
            .AddSingleton<IPublicClientConnections, PublicClientConnections>()
            .AddSingleton<IConnectorStatusListener, ConnectorStatusListener>()
            .AddSingleton<IDynamicConfigUpdater, DynamicConfigUpdater>()
            .AddSingleton<ICustomerIdAllocator, CustomerIdAllocator>()
            .AddSingleton<ITelegramAlertsApi, TelegramAlertsApi.TelegramAlertsApi>()
            .AddSingleton<IConfiguration>(Configuration)
            .AddScoped<LiquidityDbContext>()
            .AddDbContext<LiquidityDbContext>(
                  options => options.UseNpgsql(connString))
            .AddSingleton(mapper)
            .Configure<MessageBrokerConfig>(Configuration.GetSection(MessageBrokerConfig.MsgBrokerConfig))
            .AddOptions();





      }
   }
}