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

namespace TestSender
{
   class Program
   {
      private static IConfigurationRoot Configuration { get; set; }
      private static string _environmentVariable;
     

      static async Task Main(string[] args)
      {
         Console.WriteLine("Starting testsender process...");
         Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
         CultureInfo.DefaultThreadCurrentCulture = Thread.CurrentThread.CurrentCulture;
         _environmentVariable = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
         Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{_environmentVariable ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

         var serviceCollection = new ServiceCollection();
         ConfigureServices(serviceCollection);
         var serviceProvider = serviceCollection.BuildServiceProvider();
         var messageSender = serviceProvider.GetService<IMessageSender>();
         var messageReceiver = serviceProvider.GetService<IMessageReceiver>();
         messageReceiver.Start();
         messageSender.Run();
         await Task.Run(() => { while (true) { Thread.Sleep(300); } });
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
            .AddSingleton<IMessageSender, MessageSender>()
            .AddSingleton<IMessageReceiver, MessageReceiver>()            
            .AddSingleton<IOrderAndTradeProcessing, OrderTradeProcessing>()
            .AddSingleton<IOrderBookProcessor, OrderBookProcessor>()           
            .AddSingleton<IMessageBroker, RabbitMQBroker>()
            .AddSingleton<IOrderLedgerProcessor, OrderLedgerProcessor>()
            .AddSingleton<IPortfolioRepository, PortfolioRepository>()
            .AddScoped<LiquidityDbContext>()
            .AddDbContext<LiquidityDbContext>(
                  options => options.UseNpgsql(connString))
            .AddSingleton(mapper)
            .Configure<MessageBrokerConfig>(Configuration.GetSection(MessageBrokerConfig.MsgBrokerConfig))
            .AddOptions();
           
            
       
           
          
      }
   }
}
