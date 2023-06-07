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
using Amazon;
using Polly;
using Microsoft.Extensions.DependencyInjection;
using DataStore;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Common;
using Common.Utilities;
using MessageBroker.Configuration;
using MessageBroker;

namespace MultipleStrategyManager
{
   internal class Program
   {
      private static IConfigurationRoot Configuration { get; set; }
      private static string _environmentVariable;
      static async Task Main(string[] args)
      {
         try
         {
            string configName = "";
            bool showhelp = false;
            Console.WriteLine("Starting Strategy process...");
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = Thread.CurrentThread.CurrentCulture;
            _environmentVariable = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{_environmentVariable ?? "Production"}.json", optional: true)
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

            KeyInitialiser.Initialize(Configuration); ;

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var strategyManager = serviceProvider.GetService<IStrategyMonitor>();
            var messageReceiver = serviceProvider.GetService<IStrategyMessageReceiver>();
           
            messageReceiver.Start();

            await Task.Run(() => { while (true) { Thread.Sleep(300); } });
         }
         catch (Exception e)
         {
            Log.Logger.Error(e, "Error in startup - Error - {Error}", e.Message);
         }
      }

      private static void ConfigureServices(IServiceCollection services)
      {
         var config = new MapperConfiguration(cfg =>
         {
            cfg.AddProfile(new AutoMapperProfile());
         });

         var mapper = config.CreateMapper();

         Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
               .CreateLogger();

         var connString = Configuration.GetConnectionString("DefaultConnection");

         services.AddLogging(x => x.AddSerilog(Log.Logger))
          
            .AddSingleton<IConfiguration>(Configuration)
            .AddSingleton<IStrategyMonitor, StrategyMonitor>()
            .AddSingleton<IStrategyMessageReceiver, StrategyMessageReceiver>()
            .AddSingleton<IPortfolioRepository, PortfolioRepository>()
            .AddSingleton<IMessageBroker, RabbitMQBroker>()
            .AddScoped<LiquidityDbContext>()
            .AddDbContext<LiquidityDbContext>(
                  options => options.UseNpgsql(connString))
            .AddSingleton(mapper)
            .Configure<MessageBrokerConfig>(Configuration.GetSection(MessageBrokerConfig.MsgBrokerConfig))
            .AddOptions();
      }
   }
}