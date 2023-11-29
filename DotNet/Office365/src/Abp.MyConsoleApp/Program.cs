using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Volo.Abp;

namespace Abp.MyConsoleApp;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Volo", LogEventLevel.Error)//屏蔽abp框架启动时的模块加载信息
            .Enrich.FromLogContext()
            // .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        try
        {
            Log.Information("Starting console host.");
            ThreadPool.GetMinThreads(out int minWorkerThreads, out int minCompletionPortThreads);
            var coreCount = Environment.ProcessorCount;
            Log.Information($"Machine cpu core count:{coreCount}, minimum worker threads:{minWorkerThreads}, minimum completion port threads:{minCompletionPortThreads}.");
            ThreadPool.SetMinThreads(100, 25);

            ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);
            Log.Information($"Set minimum thread pool completed, minimum worker threads:{minWorkerThreads}, minimum completion port threads:{minCompletionPortThreads}.");

            var builder = Host.CreateDefaultBuilder(args);

            builder.ConfigureServices(services =>
            {
                services.Configure<SharePointSettings>(services.GetConfiguration().GetSection("SharePointSetting"));
                services.AddHostedService<MyConsoleAppHostedService>();
                services.AddApplicationAsync<MyConsoleAppModule>(options =>
                {
                    options.Services.ReplaceConfiguration(services.GetConfiguration());
                    options.Services.AddLogging(loggingBuilder => loggingBuilder.ClearProviders().AddSerilog());
                });
            }).AddAppSettingsSecretsJson().UseAutofac().UseConsoleLifetime();

            var host = builder.Build();
            await host.Services.GetRequiredService<IAbpApplicationWithExternalServiceProvider>()
                .InitializeAsync(host.Services);

            await host.RunAsync();

            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}