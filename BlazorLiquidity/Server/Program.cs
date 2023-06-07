using Microsoft.AspNetCore.ResponseCompression;
using AutoMapper;
using Common;
using DataStore;
using MessageBroker;
using Microsoft.EntityFrameworkCore;
using BlazorLiquidity.Server.Hubs;
using Serilog;
using LastTradedPriceProcessing;
using FairValueProcessing;
using OrderBookProcessing;
using OrderAndTradeProcessing;
using StrategyRunner;
using StrategyMessageListener;
using BlazorLiquidity.Server.Receiver;
using Common.Models;
using StrategyRunner.StrategyFactories;
using Strategies;
using AccountBalanceManager;
using Amazon;
using ClientConnections;
using ConnectorStatus;
using BlazorLiquidity.Server.RealTime;
using BlazorLiquidity.Server.OrdersAndTrades;
using BlazorLiquidity.Server.ErrorHandling;
using BlazorLiquidity.Server.FairValueProcessing;
using BlazorLiquidity.Server.StrategyInstanceManager;
using Common.Utilities;
using CustomerIdAllocation;
using DynamicConfigHandling;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace BlazorLiquidity.Server
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
            .AddSecretsManager(region: RegionEndpoint.EUWest2,
               configurator: options =>
               {
                  options.SecretFilter = entry => entry.Name.Contains("Liquidity");
                  options.KeyGenerator = (_, s) => s
                     .Replace("__", ":");
                  options.PollingInterval = TimeSpan.FromHours(1);
               })
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
         KeyInitialiser.Initialize(Configuration);
         builder.Services.AddControllers();
         // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
         builder.Services.AddEndpointsApiExplorer();
         //builder.Services.AddSwaggerGen();
         builder.Services.AddSingleton(mapper);
         builder.Services.AddScoped<LiquidityDbContext>();
         builder.Services.AddSingleton<IPortfolioRepository, PortfolioRepository>();
         builder.Services.AddSingleton<StrategyStartConfig>();
         builder.Services.AddSingleton<IStrategyInstanceFactory, StrategyInstanceFactory>();
         builder.Services.AddSingleton<IInventoryFactory, InventoryFactory>();
         builder.Services.AddSingleton<IOrderAndTradeProcessingFactory, OrderAndTradeProcessingFactory>();
         builder.Services.AddSingleton<IRealTimerUpdaterFactory, RealTimerUpdaterFactory>();
         builder.Services.AddTransient<IInventoryManager, InventoryManager>();
         builder.Services.AddSingleton<IMessageReceiver, MultiStrategyMessageReceiver>();
         builder.Services.AddSingleton<IStrategyInstanceManager, StrategyInstanceManagerServer>();
         builder.Services.AddTransient<IOrderAndTradeProcessing, OrdersAndTradeForUI>();
         builder.Services.AddTransient<IOrderPlacementHolder, OrderPlacementHolderUI>();
         builder.Services.AddSingleton<IOrderBookProcessor, OrderBookProcessor>();
         builder.Services.AddSingleton<IFairValuePricingUI, FairValuePricingUI>();
         builder.Services.AddSingleton<ILastTradedPriceHandler, LastTradedPriceHandler>();
         builder.Services.AddSingleton<IMessageBroker, RabbitMQBroker>();
         builder.Services.AddTransient<IStrategyInstance, StrategyInstance>();
        
         builder.Services.AddSingleton<IOrderPlacementHolderFactory, OrderPlacementHolderFactory>();
         builder.Services.AddSingleton<IErrorHandler, ErrorHandler>();
         
         builder.Services.AddSingleton<IPrivateClientConnections, PrivateClientConnections>();
         builder.Services.AddSingleton<IPublicClientConnections, PublicClientConnections>();
         builder.Services.AddSingleton<IConnectorStatusListener, ConnectorStatusListener>();
         builder.Services.AddTransient<IRealTimeUpdater, RealTimeUpdater>();
         builder.Services.AddSingleton<IDynamicConfigUpdater, DynamicConfigUpdater>();
         builder.Services.AddSingleton<ICustomerIdAllocator, CustomerIdAllocator>();
         builder.Services.AddSignalR();
         builder.Services.AddControllersWithViews();
         builder.Services.AddRazorPages();
        
         builder.Services.AddLogging(x => x.AddSerilog(Log.Logger));
         var connString = Configuration.GetConnectionString("Liquidity");
         var audience = Configuration["Liquidity:Auth:Auth0:Audience"];
         var authority = $"https://{Configuration["Liquidity:Auth:Auth0:Domain"]}";
         var issuer = $"https://{Configuration["Liquidity:Auth:Auth0:Domain"]}";


         builder.Services.AddDbContext<LiquidityDbContext>(
                  options => options.UseNpgsql(connString));
         builder.Services.AddSingleton<IMessageBroker, RabbitMQBroker>();

         builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
            {
               c.Authority = authority;
               c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
               {
                  ValidAudience = audience,
                  ValidIssuer = issuer
               };
            });


         var app = builder.Build();

         // Configure the HTTP request pipeline.
        // if (app.Environment.IsDevelopment())
    //     {
      //      app.UseSwagger();
       //     app.UseSwaggerUI();
        // }

         app.UseHttpsRedirection();


         app.UseRouting();
         app.UseHttpsRedirection();
         app.UseBlazorFrameworkFiles();
         app.UseStaticFiles();

         app.UseAuthentication();
         app.UseAuthorization();

         app.UseEndpoints(endpoints =>
         {
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            endpoints.MapHub<PortfolioHub>("/portfoliohub");
            endpoints.MapFallbackToFile("index.html");
         });

         app.Run();
      }
   }
}

