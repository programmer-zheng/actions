using System.Linq;
using System.Net;
using System.Threading.Channels;
using Furion.Demo.Application.Monitor.Dtos;
using Furion.Demo.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
        services.AddEventBus();

        services.AddSqlSugar();

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseForwardedHeaders();
        /*app.UseForwardedHeaders(new ForwardedHeadersOptions()
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
            ForwardLimit = null,
            RequireHeaderSymmetry = false,
            KnownProxies = { IPAddress.Any },
        });*/

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.Use(async (context, next) =>
        {
            // 记录所有请求头
            var customHost = App.GetConfig<string>("CustomHostAndPort");
            var logger = context.RequestServices.GetRequiredService<ILogger<Startup>>();
            logger.LogDebug("所有请求头信息：");
            foreach (var header in context.Request.Headers)
            {
                // logger.LogInformation($"{header.Key}: {header.Value}");
            }

            var forwardedHost = context.Request.Headers["X-Forwarded-Host"].FirstOrDefault();
            var forwardedProto = context.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
            var forwardedPort = context.Request.Headers["X-Forwarded-Port"].FirstOrDefault();

            logger.LogDebug("原始请求信息：");
            logger.LogDebug($"X-Forwarded-Host: {forwardedHost}");
            logger.LogDebug($"X-Forwarded-Proto: {forwardedProto}");
            logger.LogDebug($"X-Forwarded-Port: {forwardedPort}");
            logger.LogDebug($"原始Host: {context.Request.Host}");
            logger.LogDebug($"原始Scheme: {context.Request.Scheme}");

            // 如果没有转发头部，尝试从原始请求中获取信息
            if (string.IsNullOrEmpty(forwardedHost))
            {
                forwardedHost = context.Request.Host.Host;
                logger.LogDebug($"使用原始Host作为转发Host: {forwardedHost}");
            }

            if (string.IsNullOrEmpty(forwardedProto))
            {
                forwardedProto = context.Request.Scheme;
                logger.LogDebug($"使用原始Scheme作为转发Scheme: {forwardedProto}");
            }

            if (!string.IsNullOrWhiteSpace(customHost))
            {
                context.Request.Host = new HostString(customHost);
            }

            logger.LogDebug($"修改后的Host: {context.Request.Host}");

            if (!string.IsNullOrEmpty(forwardedProto))
            {
                context.Request.Scheme = forwardedProto;
                logger.LogDebug($"修改后的Scheme: {context.Request.Scheme}");
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