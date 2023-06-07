using System.Security.Claims;
using Blazored.LocalStorage;
using BlazorLiquidity.Client;
using BlazorLiquidity.Client.Auth;
using BlazorLiquidity.Shared;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;
using Microsoft.AspNetCore.Components.WebAssembly;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Syncfusion.Blazor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


//Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NzAzOTIyQDMyMzAyZTMyMmUzMEptRy9JREk3Y1ZMQXpidW1uaU1objJCZGMrZnE2OW0rbVRySmtUODdrbWs9");
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
   "OTk2NzUxQDMyMzAyZTM0MmUzMFpTN05vNEZ6UGwrS0JSNHlocUZBWjFtNFVyK0gxVEFsZk85azFhWmZsaFk9");
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
Console.WriteLine($"Base Address is {baseAddress}");

builder.Services.AddHttpClient<PortfolioHttpClient>(client => client.BaseAddress = baseAddress)
   .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
   .CreateClient("PortfolioHttpClient"));
builder.Services.AddSingleton<PageHistoryState>();
builder.Services.AddSingleton<HubConnection>(sp => {
var navigationManager = sp.GetRequiredService<NavigationManager>();
   
   return new HubConnectionBuilder()
  .WithUrl(navigationManager.ToAbsoluteUri("/portfoliohub"))
  .WithAutomaticReconnect()
  .Build();
});

var audience = builder.Configuration["Liquidity:Auth:Auth0:Audience"];
Console.WriteLine($"************************  audience is {audience}");

builder.Services.AddOidcAuthentication(options =>
{
   builder.Configuration.Bind("Liquidity:Auth:Auth0", options.ProviderOptions);
   options.ProviderOptions.ResponseType = "code";
   options.ProviderOptions.AdditionalProviderParameters.Add("audience", audience);
}); //.AddAccountClaimsPrincipalFactory<CustomUserFactory<RemoteUserAccount>> ();

builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
