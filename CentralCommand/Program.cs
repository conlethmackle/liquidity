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
using MessageBroker;
using System.Net.Http;
using Polly;

using Microsoft.Extensions.DependencyInjection;
using DataStore;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Common;

namespace CentralCommand
{
   internal class Program
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
         var subAccountInitiator = serviceProvider.GetService<ISubAccountInitiator>();
         subAccountInitiator.Init();
         var commandReceiver = serviceProvider.GetService<ICommandReceiver>();
         commandReceiver.Start();
     //    var messageReceiver = serviceProvider.GetService<IMessageReceiver>();
     //  messageReceiver.Start();
     //     messageSender.Run();
         await Task.Run(() => { while (true) { Thread.Sleep(300); } });
      }

      private static void ConfigureServices(IServiceCollection services)
      {
         var config = new MapperConfiguration(cfg =>
         {
            cfg.AddProfile(new AutoMapperProfile());
         });
         var mapper = config.CreateMapper();
         var connString = Configuration.GetConnectionString("DefaultConnection");
         Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
               .CreateLogger();
         services.AddLogging(x => x.AddSerilog(Log.Logger))
            .AddSingleton<ISubAccountInitiator, SubAccountInitiator>()
            .AddSingleton<IPortfolioRepository, PortfolioRepository>()
            .AddScoped<LiquidityDbContext>()
            .AddDbContext<LiquidityDbContext>(
                  options => options.UseNpgsql(connString))
            .AddSingleton(mapper)
            .AddSingleton<IMessageBroker, RabbitMQBroker>()
            .AddSingleton<ICommandReceiver, CommandReceiver>();
         

        
      }
   }
}