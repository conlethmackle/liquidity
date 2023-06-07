using AccountBalanceManager;
using AutoMapper;
using Common;
using ConnectorStatus;
using DataStore;
using FairValueProcessing;
using MessageBroker;
using Microsoft.EntityFrameworkCore;
using OrderAndTradeProcessing;
using OrderBookProcessing;
using PortfolioManagementAPI.Hubs;
using PortfolioManagementAPI.MessageReceiver;
using Serilog;

namespace PortfolioManagementAPI
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
         builder.Services.AddSwaggerGen();
         builder.Services.AddSingleton(mapper);
         builder.Services.AddScoped<LiquidityDbContext>();
         builder.Services.AddSingleton<IPortfolioRepository, PortfolioRepository>();
         builder.Services.AddLogging(x => x.AddSerilog(Log.Logger));
         var connString = Configuration.GetConnectionString("DefaultConnection");
         builder.Services.AddDbContext<LiquidityDbContext>(
                  options => options.UseNpgsql(connString));
         builder.Services.AddSingleton<IMessageBroker, RabbitMQBroker>();
         var app = builder.Build();

         // Configure the HTTP request pipeline.
         if (app.Environment.IsDevelopment())
         {
            app.UseSwagger();
            app.UseSwaggerUI();
         }

         app.UseHttpsRedirection();

         app.UseAuthorization();

         app.MapControllers();

         app.Run();
      }
   }
}