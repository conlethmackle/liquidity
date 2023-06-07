

using System.Globalization;
using AccountBalanceManager;
using Amazon;
using Amazon.Runtime;
using AutoMapper;
using Common;
using Common.Models;
using ConnectorCommon;
using ConnectorStatus;
using DataStore;
using MessageBroker;
using MessageBroker.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TelegramCommandServer.Services;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Common.Models.Configuration;
using Common.Utilities;
using Kralizek.Extensions.Configuration;

namespace TelegramCommandServer
{
   internal class Program
   {
      private static IConfigurationRoot Configuration { get; set; }
      private static string _environmentVariable;
      static async Task Main(string[] args)
      {
         try
         {
            Console.WriteLine("Starting TelegramCommandServer process...");
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = Thread.CurrentThread.CurrentCulture;
            _environmentVariable = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddJsonFile($"appsettings.{_environmentVariable ?? "Production"}.json", optional: true)
               .AddEnvironmentVariables()
               .AddSecretsManager(region: RegionEndpoint.EUWest2,
                  configurator: options =>
                  {
                     options.SecretFilter = entry => entry.Name.Contains("Liquidity");
                     options.KeyGenerator = (_, s) => s
                        .Replace("__", ":");
                     options.PollingInterval = TimeSpan.FromHours(1);
                  })
               .AddCommandLine(args)
               
               .Build();
            KeyInitialiser.Initialize(Configuration);
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var botCreator = serviceProvider.GetService<IBotCreator>();
            var messageReceiver = serviceProvider.GetService<ITelegramMessageReceiver>();

            messageReceiver?.Start();
            await botCreator?.Init();
            await Task.Run(() => { while (true) { Thread.Sleep(300); } });
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

         KeyInitialiser.Initialize(Configuration);

         //  Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
         var connString = Configuration.GetConnectionString("Liquidity");
         
         services.AddLogging(x => x.AddSerilog(Log.Logger))
            // .AddTransient<IBinanceRestApiClient, BinanceRestApiClient>()
            .AddSingleton<ConnectorConfig>()
            .AddSingleton<IMessageBroker, RabbitMQBroker>()
            .AddSingleton<IBotCreator, BotCreator>()
            .AddSingleton<ITelegramCommandListenerFactory, TelegramCommandListenerFactory>()
            .AddSingleton<IPortfolioRepository, PortfolioRepository>()
            .AddSingleton<ITelegramMessageReceiver, TelegramMessageReceiver>()
            .AddTransient<ITelegramCommandListener, TelegramCommandListener>()
            .AddSingleton<IInventoryManager, InventoryManager>()
            .AddSingleton<StrategyStartConfig>()
            .AddScoped<LiquidityDbContext>()
            .AddSingleton(mapper)
            .AddSingleton<IConfiguration>(Configuration)
            .AddDbContext<LiquidityDbContext>(
               options => options.UseNpgsql(connString))
            .AddOptions()
            .Configure<Signing>(Configuration.GetSection(Signing.SectionName));

      }
   }
}