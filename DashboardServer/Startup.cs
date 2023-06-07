using DashboardServer.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using DashboardServer.Sender;

namespace DashboardServer
{
   public class Startup
   {
      public Startup(IConfiguration configuration)
      {
         Configuration = configuration;
      }

      public IConfiguration Configuration { get; }

      // This method gets called by the runtime. Use this method to add services to the container.
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddSingleton<IFakeOrderBookSender, FakeOrderBookSender>();
         services.AddSingleton<IChatHub, ChatHub>();
         services.AddCors(options =>
         {
            options.AddPolicy("CorsPolicy", policy =>
            {
               policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
         });
         services.AddControllers();
         services.AddSwaggerGen(c =>
         {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "DashboardServer", Version = "v1" });
         });
         services.AddSignalR();
         services.AddRazorPages();
         services.AddResponseCompression(opts =>
         {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                new[] { "application/octet-stream" });
         });
         
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
      {
         if (env.IsDevelopment())
         {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DashboardServer v1"));
         }

         app.UseHttpsRedirection();

         app.UseRouting();
         app.UseCors("CorsPolicy");
         app.UseAuthorization();
         app.UseEndpoints(endpoints =>
         {
            endpoints.MapHub<ChatHub>("/chat");
         });
       
         app.UseEndpoints(endpoints =>
         {
            endpoints.MapControllers();
         });
      }
   }
}
