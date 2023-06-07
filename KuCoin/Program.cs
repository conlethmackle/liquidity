using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Log = Serilog.Log;
using System.Net.Http;
using Polly;
using KuCoin.RestApi;
using KuCoin.Config;
using Microsoft.Extensions.DependencyInjection;
using KuCoin.WebSocket;
using MessageBroker.Configuration;
using System.Threading.Tasks;
using Amazon;
using MessageBroker;
using KuCoin.Clients;
using ConnectorCommon;
using DataStore;
using AutoMapper;
using Common;
using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using ConnectorStatus;

namespace KuCoin
{

   class Program
   {
      private static IConfigurationRoot Configuration { get; set; }
      private static string _environmentVariable;
      static async Task Main(string[] args)
      {
         try
         {
            Console.WriteLine("Starting kucoin connector process...");
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
            var mainProcessor = serviceProvider.GetService<IConnectionManager>();
            await mainProcessor.Init();
            await Task.Run(() => { while (true) { Thread.Sleep(300); } });
         }
         catch(Exception e)
         {
            Console.WriteLine($"Fatal Error - {e.Message}");
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
            .AddSingleton<ConnectorConfig>()
            .AddSingleton<IKuCoinRestApiClient, KuCoinRestApiClient>()
            .AddSingleton<IPublicClient, PublicClient>()
            .AddTransient<IPrivateClient, PrivateClient>()
            .AddSingleton<IPrivateClientFactory, PrivateClientFactory>()          
            .AddSingleton<IMessageBusProcessor, MessageBusProcessor>()         
            .AddTransient<IKuCoinWebSocket, KuCoinWebSocket>()
            .AddSingleton<IMessageBroker, RabbitMQBroker>()
            .AddSingleton<IConnectionManager, ConnectionManager>()
            .AddSingleton<IPortfolioRepository, PortfolioRepository>()
            .AddSingleton<IConnectorAliveHandler, ConnectorAliveHandler>()
            .AddSingleton<IConnectorClientStatus, ConnectorClientStatus>()
            .AddSingleton<IConfiguration>(Configuration)
            .AddScoped<LiquidityDbContext>()
            .AddDbContext<LiquidityDbContext>(
                  options => options.UseNpgsql(connString))
            .AddSingleton(mapper)
            .Configure<KuCoinConnectionConfig>(Configuration.GetSection(KuCoinConnectionConfig.PrivateConfig))
            .Configure<MessageBrokerConfig>(Configuration.GetSection(MessageBrokerConfig.MsgBrokerConfig))
            .Configure<SymbolDataConfig>(Configuration.GetSection(SymbolDataConfig.SymbolConfig))
            .Configure<ExchangeGenericConfig>(Configuration.GetSection(ExchangeGenericConfig.GenericConfig))
            
            .AddOptions()
            .AddHttpClient<IKuCoinRestApiClient, KuCoinRestApiClient>()
            .AddTransientHttpErrorPolicy(policy =>
               policy.WaitAndRetryAsync(new[] {
               TimeSpan.FromMilliseconds(200),
               TimeSpan.FromMilliseconds(500),
               TimeSpan.FromSeconds(1)
            }));         
      }    
   }
}
