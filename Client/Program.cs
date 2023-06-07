using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Syncfusion.Blazor;
using SyncfusionLiquidity.Client;


using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;

using SyncfusionLiquidity.Shared;

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NzAzOTIyQDMyMzAyZTMyMmUzMEptRy9JREk3Y1ZMQXpidW1uaU1objJCZGMrZnE2OW0rbVRySmtUODdrbWs9");

var levelSwitch = new LoggingLevelSwitch();
Log.Logger = new LoggerConfiguration()
   .MinimumLevel.ControlledBy(levelSwitch)
   .Enrich.WithProperty("InstanceId", Guid.NewGuid().ToString("n"))
   .WriteTo.BrowserConsole()
   .CreateLogger();
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddSyncfusionBlazor();
var baseAddress = new Uri(builder.HostEnvironment.BaseAddress);
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddHttpClient<PortfolioHttpClient>(client => client.BaseAddress = baseAddress);

builder.Services.AddSingleton<HubConnection>(sp => {
   var navigationManager = sp.GetRequiredService<NavigationManager>();

   return new HubConnectionBuilder()
      .WithUrl(navigationManager.ToAbsoluteUri("/portfoliohub"))
      .WithAutomaticReconnect()
      .Build();
});

await builder.Build().RunAsync();

