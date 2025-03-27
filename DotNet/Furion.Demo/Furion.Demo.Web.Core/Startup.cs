using Furion;
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

        // 添加SqlSugar
        services.AddSingleton<ISqlSugarClient>(DbContext.Instance); // 单例注册
        services.AddScoped(typeof(SugarRepository<>));


        services.AddCorsAccessor();

        services.AddEventBus();// 添加事件总线

        services.AddControllers()
                .AddInjectWithUnifyResult();

        var monitorChannel = Channel.CreateUnbounded<string>();
        services.AddKeyedSingleton<Channel<string>>("Monitor", Channel.CreateUnbounded<string>());


    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

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
