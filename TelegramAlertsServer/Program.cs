using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Serilog;
using Microsoft.Extensions.Configuration;
using Log = Serilog.Log;
using Polly;
using System.Threading.Tasks;
using Amazon;
using Microsoft.Extensions.DependencyInjection;
using MessageBroker;
using MessageBroker.Configuration;
using Microsoft.Extensions.Logging;
using ConnectorCommon;
using DataStore;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Common;
using Common.Utilities;
using ConnectorStatus;
using TelegramAlertsServer.Router;
using TelegramAlertsServer.Services;

namespace TelegramAlertsServer
{
   internal class Program
   {
      private static IConfigurationRoot Configuration { get; set; }
      private static string _environmentVariable;

      static async Task Main(string[] args)
      {
         try
         {
            Console.WriteLine("Starting telegram Command  process...");
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

            KeyInitialiser.Initialize(Configuration);
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

           
         //   if (telegramRouter != null)
            {
               //await telegramRouter.Init();
               var mainProcessor = serviceProvider.GetService<ITelegramReceiver>();
               if (mainProcessor != null)
               {
                  mainProcessor.Start();
                  await Task.Run(() =>
                  {
                     while (true)
                     {
                        Thread.Sleep(300);
                     }
                  });
               }
            }
         }
         catch (Exception e)
         {
            Log.Logger.Fatal(e, "Fatal Error {Error} on TelegramCommandServer Start", e.Message);

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

         //  Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
         var connString = Configuration.GetConnectionString("Liquidity");
         services.AddLogging(x => x.AddSerilog(Log.Logger))
            .AddSingleton<IMessageBroker, RabbitMQBroker>()
            .AddSingleton<ITelegramAlertRouting, TelegramAlertRouting>()
            .AddSingleton<ITelegramReceiver, MQReceiver>()
            .AddSingleton<IPortfolioRepository, PortfolioRepository>()
            .AddScoped<LiquidityDbContext>()
            .AddSingleton(mapper)
            .AddSingleton<IConfiguration>(Configuration)
            .AddDbContext<LiquidityDbContext>(
                  options => options.UseNpgsql(connString))
           
            .Configure<MessageBrokerConfig>(Configuration.GetSection(MessageBrokerConfig.MsgBrokerConfig))
            .Configure<SymbolDataConfig>(Configuration.GetSection(SymbolDataConfig.SymbolConfig))
            .Configure<ExchangeGenericConfig>(Configuration.GetSection(ExchangeGenericConfig.GenericConfig))
            .AddOptions();

      }
   }
}