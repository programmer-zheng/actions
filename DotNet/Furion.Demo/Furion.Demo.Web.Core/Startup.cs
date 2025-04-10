using Furion;
using Furion.Demo.Application.Monitor.Dtos;
using Furion.Demo.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlSugar;
using System.Threading.Channels;

namespace Furion.Demo.Web.Core;

public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddConsoleFormatter();
        services.AddJwt<JwtHandler>();



        services.AddCorsAccessor();

        services.AddEventBus();// 添加事件总线

        services.AddControllers()
                .AddInjectWithUnifyResult()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new DateTimeConverterUsingDateTimeParse());
                });

        var monitorChannel = Channel.CreateUnbounded<string>();
        services.AddKeyedSingleton<Channel<CustomMonitorEventDto>>("Monitor", Channel.CreateUnbounded<CustomMonitorEventDto>());

        services.AddSqlSugar();

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseStaticFiles();
        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCorsAccessor();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseInject(string.Empty);

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
