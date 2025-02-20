using System.Transactions;
using Hangfire;
using Hangfire.LiteDB;
using Hangfire.MySql;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Quartz.Impl;
// using Quartz.AspNetCore;
using Scalar.AspNetCore;
using Scalar.Filters;
using Scalar.Middleware;

namespace Scalar;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers(options =>
        {
            // 添加自定义过滤器
            // options.Filters.Add(typeof(CustomValidFilter), 1);
            // options.Filters.Add(typeof(CustomResultFilter), 2);
            // options.Filters.Add(typeof(CustomExceptionFilter), 3);
            options.Filters.Add<CustomValidFilter>();
            options.Filters.Add<CustomResultFilter>();
            options.Filters.Add<CustomExceptionFilter>();
        });
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            // 关闭 [ApiController] 自动返回 400 的行为
            options.SuppressModelStateInvalidFilter = true;
        });
        
        var defaultConnectionString = builder.Configuration.GetConnectionString("Default");
        builder.Services.AddHangfire(options =>
        {
            options.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();

            // options.UseLiteDbStorage(hfSqliteConnectionString);
            options.UseInMemoryStorage();
            // options.UseStorage(new MySqlStorage(hfMySqliteConnectionString, new MySqlStorageOptions()
            // {
            //     TransactionIsolationLevel = IsolationLevel.ReadCommitted,
            //     QueuePollInterval = TimeSpan.FromSeconds(15),
            //     JobExpirationCheckInterval = TimeSpan.FromHours(1),
            //     CountersAggregateInterval = TimeSpan.FromMinutes(5),
            //     PrepareSchemaIfNecessary = true,
            //     DashboardJobListLimit = 50000,
            //     TransactionTimeout = TimeSpan.FromMinutes(1),
            //     TablesPrefix = "Hangfire"
            // }));
        });
        StdSchedulerFactory factory = new StdSchedulerFactory();
        IScheduler scheduler = await factory.GetScheduler();

        // and start it off
        await scheduler.Start();
        
        builder.Services.AddHangfireServer(options =>
        {
            // options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
            // options.HeartbeatInterval = TimeSpan.FromMinutes(3);//设置服务器心跳间隔，每隔多久发送一次心跳
            // options.ServerTimeout = TimeSpan.FromMinutes(5);//设置服务器超时时间，超过这个时间，服务器会自动关闭,默认是5分钟
            // options.ServerCheckInterval = TimeSpan.FromMinutes(5);
            options.ServerName = "HangFireServer";
        });


        // Quartz.net 数据库表结构参考：https://github.com/quartznet/quartznet/tree/main/database/tables
        // Quartz.net并不会在程序启动时，自动生成表
        // builder.Services.AddQuartzServer(options =>
        // {
        //     // 当关闭服务器时，是否等待所有任务执行完成
        //     options.WaitForJobsToComplete = true;
        //     options.AwaitApplicationStarted = true;
        // });


        var app = builder.Build();

        app.UseHangfireDashboard();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("文档中心") //设置浏览器标签页标题
                    .WithForceThemeMode(ThemeMode.Dark) // 强制页面只能使用深色模式
                    .WithTheme(ScalarTheme.Kepler) // 设置默认主题
                    // .WithEndpointPrefix("/swagger/{documentName}") // 默认为/scalar/v1，更改后使用/swagger/v1访问
                    .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios)
                    ;
                ;

                // options.EnabledClients = new[] { ScalarClient.HttpClient, ScalarClient.Axios, ScalarClient.Curl, ScalarClient.AsyncHttp, ScalarClient.Request };
                // options.EnabledTargets = new[] { ScalarTarget.CSharp, ScalarTarget.Java, ScalarTarget.JavaScript, ScalarTarget.Shell,  };

                /*
                 主题
                Alternate：
                Default：黑夜
                Moon：月亮
                Purple：紫色（默认）
                Solarized：Solarized
                BluePlanet：蓝星
                Saturn：土星
                Kepler：开普勒
                Mars：火星
                DeepSpace：深空
                */
            });
        }

        app.UseAuthorization();

        app.UseRouting();

        app.UseMiddleware<CustomNotFoundMiddleware>();
        // app.MapControllers();

        app.UseEndpoints(endpoints => { endpoints.MapControllerRoute("Default", "{controller}/{action}/{id?}"); });
        app.Run();
    }
}