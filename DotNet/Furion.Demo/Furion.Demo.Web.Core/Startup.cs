using System.Linq;
using System.Net;
using Furion;
using Furion.Demo.Application.Monitor.Dtos;
using Furion.Demo.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlSugar;
using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace Furion.Demo.Web.Core;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddConsoleFormatter();
        services.AddJwt<JwtHandler>();


        services.AddCorsAccessor();

        services.AddEventBus(); // 添加事件总线

        services.AddControllers()
            .AddInjectWithUnifyResult()
            .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new DateTimeConverterUsingDateTimeParse()); });

        var monitorChannel = Channel.CreateUnbounded<string>();
        services.AddKeyedSingleton<Channel<CustomMonitorEventDto>>("Monitor", Channel.CreateUnbounded<CustomMonitorEventDto>());

        services.AddSqlSugar();

        // services.Configure<ForwardedHeadersOptions>(options =>
        // {
        //     options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
        //     options.ForwardLimit = null; // 如果有多个代理层，可以设为 null 来接收所有头
        //     options.KnownProxies.Add(IPAddress.Any);
        // });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseForwardedHeaders(new ForwardedHeadersOptions()
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
            ForwardLimit = null, // 如果有多个代理层，可以设为 null 来接收所有头
            RequireHeaderSymmetry = false,
            KnownProxies = { IPAddress.Any },

            KnownNetworks = { new IPNetwork(IPAddress.Parse("192.168.88.0"), 24)  }

        });
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.Use(async (context, next) =>
        {
            var forwardedHost = context.Request.Headers["X-Forwarded-Host"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHost))
            {
                context.Request.Host = HostString.FromUriComponent("https://" + forwardedHost);
            }
            await next();
        });
        
        app.UseStaticFiles();
        // app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCorsAccessor();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseInject(string.Empty);

        app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
    }
}