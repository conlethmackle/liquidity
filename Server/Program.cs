using Microsoft.AspNetCore.ResponseCompression;
using AutoMapper;
using Common;
using DataStore;
using MessageBroker;
using Microsoft.EntityFrameworkCore;
using Serilog;
using LastTradedPriceProcessing;
using FairValueProcessing;
using OrderBookProcessing;
using OrderAndTradeProcessing;
using StrategyRunner;
using StrategyMessageListener;
using SyncfusionLiquidity.Server.Receiver;
using Common.Models;
using StrategyRunner.StrategyFactories;
using Strategies;
using AccountBalanceManager;
using ClientConnections;
using ConnectorStatus;
using SyncfusionLiquidity.Server.RealTime;
using SyncfusionLiquidity.Server.OrdersAndTrades;
using SyncfusionLiquidity.Server.ErrorHandling;
using DynamicConfigHandling;
using SyncfusionLiquidity.Server.Hubs;

namespace SyncfusionLiquidity.Server
{
   public class Program
   {

      private static IConfigurationRoot Configuration { get; set; }
      private static string _environmentVariable;

      public static void Main(string[] args)
      {
         Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{_environmentVariable ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
         var builder = WebApplication.CreateBuilder(args);
         Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();


         // Add services to the container.
         var config = new MapperConfiguration(cfg =>
         {
            cfg.AddProfile(new AutoMapperProfile());
         });
         var mapper = config.CreateMapper();
         builder.Services.AddControllers();
         // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
         builder.Services.AddEndpointsApiExplorer();
         //builder.Services.AddSwaggerGen();
         builder.Services.AddSingleton(mapper);
         builder.Services.AddScoped<LiquidityDbContext>();
         builder.Services.AddSingleton<IPortfolioRepository, PortfolioRepository>();
         builder.Services.AddSingleton<IInventoryManager, InventoryManager>();
         builder.Services.AddSingleton<IPortFolioMessageReceiver, Receiver.MessageReceiver>();
         //     builder.Services.AddSingleton<IStrategyManager, StrategyManager>();
         builder.Services.AddSingleton<IOrderAndTradeProcessing, OrdersAndTradeForUI>();
         builder.Services.AddTransient<IOrderPlacementHolder, OrderPlacementHolderUI>();
         builder.Services.AddSingleton<IOrderBookProcessor, OrderBookProcessor>();
         builder.Services.AddSingleton<IFairValuePricing, FairValuePricing>();
         builder.Services.AddSingleton<ILastTradedPriceHandler, LastTradedPriceHandler>();
         builder.Services.AddSingleton<IMessageBroker, RabbitMQBroker>();
         builder.Services.AddSingleton<StrategyStartConfig>();
         builder.Services.AddSingleton<IOrderPlacementHolderFactory, OrderPlacementHolderFactory>();
         builder.Services.AddSingleton<IErrorHandler, ErrorHandler>();
         // builder.Services.AddSingleton<IFactoryOfStrategyFactories, FactoryOfStrategyFactories>();
         //  builder.Services.AddSingleton<IHedgingStrategyFactory, HedgingStrategyFactory>();
         //  builder.Services.AddSingleton<IMarketMakingStrategyFactory, MarketMakingStrategyFactory>();
         //  builder.Services.AddSingleton<IFairValueLiquidationStrategyFactory, FairValueLiquidationStrategyFactory>();
         //  builder.Services.AddSingleton<IHedgerStrategy, HedgerStrategy>();
         //  builder.Services.AddSingleton<IMarketMakingStrategy, MarketMakingStrategy>();
         //  builder.Services.AddSingleton<IFairValueLiquidationStrategy, FairValueLiquidationStrategy>();
         builder.Services.AddSingleton<IMessageReceiver, StrategyMessageListener.MessageReceiver>();
         builder.Services.AddSingleton<IPrivateClientConnections, PrivateClientConnections>();
         builder.Services.AddSingleton<IPublicClientConnections, PublicClientConnections>();
         builder.Services.AddSingleton<IConnectorStatusListener, ConnectorStatusListener>();
         builder.Services.AddSingleton<IRealTimeUpdater, RealTimeUpdater>();
         builder.Services.AddSingleton<IDynamicConfigUpdater, DynamicConfigUpdater>();
         builder.Services.AddSignalR();
         builder.Services.AddControllersWithViews();
         builder.Services.AddRazorPages();
         //builder.Services.UseRou
         // builder.Services.AddSingleton<IPortfolioHub, PortfolioHub>();
         builder.Services.AddLogging(x => x.AddSerilog(Log.Logger));
         var connString = Configuration.GetConnectionString("DefaultConnection");
         builder.Services.AddDbContext<LiquidityDbContext>(
                  options => options.UseNpgsql(connString));
         builder.Services.AddSingleton<IMessageBroker, RabbitMQBroker>();

         // Add services to the container.

         builder.Services.AddControllersWithViews();
         builder.Services.AddRazorPages();

         var app = builder.Build();

// Configure the HTTP request pipeline.
         if (app.Environment.IsDevelopment())
         {
            app.UseWebAssemblyDebugging();
         }
         else
         {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
         }

         app.UseHttpsRedirection();

         app.UseBlazorFrameworkFiles();
         app.UseStaticFiles();

         app.UseRouting();
         app.UseAuthorization();


         app.UseEndpoints(endpoints =>
         {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            endpoints.MapHub<PortfolioHub>("/portfoliohub");
            endpoints.MapFallbackToFile("index.html");
         });

         app.MapRazorPages();
         app.MapControllers();
         app.MapFallbackToFile("index.html");

         app.Run();
      }
   }
}
