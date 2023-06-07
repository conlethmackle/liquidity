using DataStore;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Log = Serilog.Log;
using Common.Models.Entities;

namespace DatabaseConsole
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
         var dbcontext = serviceProvider.GetRequiredService<LiquidityDbContext>();
       
        
            var apiKey = new ApiKey()
            {
               DateCreated = DateTime.Now,
               Key = "ABCDEFG",
               Secret = "ASecret",
               PassPhrase = "pass"
            };

         //   liquidContext.Add(apiKey);

            var exchange = new ExchangeDetails()
            {
              
               IsEnabled = true,
               Name = "KuCoin",
               ApiKey = new ApiKey()
               {
                  DateCreated = DateTime.UtcNow,
                  Key = "ABCDEFG",
                  Secret = "ASecret",
                  PassPhrase = "pass"
               },
               DateCreated = DateTime.UtcNow
            };

         dbcontext.Add(exchange);
            await dbcontext.SaveChangesAsync();
         
         await Task.Run(() => { while (true) { Thread.Sleep(300); } });
      }

      private static void ConfigureServices(IServiceCollection services)
      {
         Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
               .CreateLogger();
         services.AddLogging(x => x.AddSerilog(Log.Logger));

         var connString = Configuration.GetConnectionString("DefaultConnection");
         services.AddDbContext<LiquidityDbContext>(
                  options => options.UseNpgsql(connString));
      }
      /*using (var liquidContext = new LiquidityDbContext())
      {
         liquidContext.Database.EnsureCreated();

         var sp = new SP()
         {
            Name = "SP 2"
         };

         liquidContext.Add(sp);
         await liquidContext.SaveChangesAsync(); */
   }
}