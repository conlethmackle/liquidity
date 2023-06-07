using DataStore;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Sinks.SystemConsole;
using Log = Serilog.Log;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl.Calendar;
using MessageBroker;
using AutoMapper;
using Common;
using System.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Scheduler
{
   internal class Program
   {
      private static IConfigurationRoot Configuration { get; set; }
      private static string _environmentVariable;
      static void Main(string[] args)
      {
         CreateHostBuilder(args).Build().Run();
      }

      public static IHostBuilder CreateHostBuilder(string[] args) =>
         Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
               Configuration = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                  .AddJsonFile($"appsettings.{_environmentVariable ?? "Production"}.json", optional: true)
                  .AddEnvironmentVariables()
                  .Build();
               
               Log.Logger = new LoggerConfiguration()
                  .WriteTo.Console()
                  .CreateLogger();

               // Add the required Quartz.NET services
               services.AddQuartz(q =>
               {
                  // Use a Scoped container to create jobs. I'll touch on this later
                  q.UseMicrosoftDependencyInjectionJobFactory();

                  const string calendarName = "myHolidayCalendar";
                  q.AddCalendar<HolidayCalendar>(
                     name: calendarName,
                     replace: true,
                     updateTriggers: true,
                     x => x.AddExcludedDate(new DateTime(2020, 5, 15))
                  );

                /*  q.AddTrigger(t => t
                     .WithIdentity("Daily Trigger")
                     .ForJob(jobKey)
                     .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(5)))
                     .WithDailyTimeIntervalSchedule(x => x.WithInterval(10, IntervalUnit.Second))
                     .WithDescription("my awesome daily time interval trigger")
                     .ModifiedByCalendar(calendarName)
                  ); */


                  var jobKey = new JobKey("StartOfDayTrigger");
                  q.AddJob<StartOfDayTrigger>(opts => opts.WithIdentity(jobKey));
                  q.AddTrigger(opts => opts
                     .ForJob(jobKey) // link to the HelloWorldJob
                     .WithIdentity("StartOfDay-trigger") // give the trigger a unique name
                     .WithCronSchedule("0 58 21 * * ?")); 
               });

               // Add the Quartz.NET hosted service

               services.AddQuartzHostedService(
                  q => q.WaitForJobsToComplete = true);

               // other config
               services.AddScoped<IStartOfDayTrigger, StartOfDayTrigger>();
               services.AddSingleton<IMessageBroker, RabbitMQBroker>();
           

               var config = new MapperConfiguration(cfg =>
               {
                  cfg.AddProfile(new AutoMapperProfile());
               });
               var mapper = config.CreateMapper();
              
               services.AddSingleton(mapper);
               services.AddScoped<LiquidityDbContext>();
               var connString = Configuration.GetConnectionString("DefaultConnection");
               services.AddDbContext<LiquidityDbContext>(
                  options => options.UseNpgsql(connString));
              
               services.AddSingleton<IPortfolioRepository, PortfolioRepository>();
            });
   }
}