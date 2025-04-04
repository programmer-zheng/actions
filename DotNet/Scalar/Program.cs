using System.Collections.Specialized;
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
using Scalar.ServiceLifetime;

namespace Scalar;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddTransient<ITransientService, TransientService>();
        builder.Services.AddTransient<IScopedService, ScopedService>();
        builder.Services.AddTransient<ISingletonService, SingletonService>();


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
        builder.Services.AddHangfireService();

        var properties = new NameValueCollection
        {
            { "quartz.serializer.type", "newtonsoft" },
            { "quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.MySQLDelegate, Quartz" },
            { "quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz" },
            { "quartz.jobStore.dataSource", "default" },
            { "quartz.dataSource.default.provider", "MySql" },
            {"quartz.jobStore.performSchemaValidation","false" },
            { "quartz.dataSource.default.connectionString", defaultConnectionString },
        };
        ISchedulerFactory schedulerFactory = new StdSchedulerFactory(properties);
        IScheduler scheduler = await schedulerFactory.GetScheduler();
        await scheduler.StartDelayed(TimeSpan.FromSeconds(5));
/*
        builder.Services.AddQuartz(option =>
        {
            option.UsePersistentStore(storeOptions =>
            {
                storeOptions.UseProperties = true;
                storeOptions.UseNewtonsoftJsonSerializer();
                storeOptions.UseMySql(defaultConnectionString);
            });
        });*/
        builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        // 注册IScheduler服务
        builder.Services.AddSingleton(typeof(ISchedulerFactory), schedulerFactory);
        builder.Services.AddSingleton(typeof(IScheduler), scheduler);
        /*builder.Services.AddSingleton<IScheduler>(provider =>
        {
            var factory = provider.GetRequiredService<ISchedulerFactory>();
            var scheduler = factory.GetScheduler().GetAwaiter().GetResult();
            scheduler.Start().GetAwaiter().GetResult();
            return scheduler;
        });*/
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