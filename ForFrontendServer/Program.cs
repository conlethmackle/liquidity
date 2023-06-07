// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Globalization;

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
            .Build();

         var serviceCollection = new ServiceCollection();
         ConfigureServices(serviceCollection);
         var serviceProvider = serviceCollection.BuildServiceProvider();
         var mainProcessor = serviceProvider.GetService<IConnectionManager>();
         await mainProcessor.Init();
         await Task.Run(() => { while (true) { Thread.Sleep(300); } });
      }
      catch (Exception e)
      {
         Console.WriteLine($"Fatal Error - {e.Message}");
      }
   }

   private static void ConfigureServices(IServiceCollection services)
   {
      Log.Logger = new LoggerConfiguration()

            .WriteTo.Console().CreateLogger();
      services.AddLogging(x => x.AddSerilog(Log.Logger))
         .AddSingleton;
         
       
       
       
   }
}