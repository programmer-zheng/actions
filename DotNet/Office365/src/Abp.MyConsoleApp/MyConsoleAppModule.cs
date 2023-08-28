using Volo.Abp.Autofac;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Modularity;

namespace Abp.MyConsoleApp;

[DependsOn(
    typeof(AbpAutofacModule)
    // ,typeof(AbpCachingStackExchangeRedisModule)
)]
public class MyConsoleAppModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Configure<RedisCacheOptions>(options => { options.InstanceName = "Abp.MyConsoleApp."; });
    }
    // public override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    // {
    //     var logger = context.ServiceProvider.GetRequiredService<ILogger<MyConsoleAppModule>>();
    //     var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
    //     logger.LogInformation($"MySettingName => {configuration["MySettingName"]}");
    //
    //     var hostEnvironment = context.ServiceProvider.GetRequiredService<IHostEnvironment>();
    //     logger.LogInformation($"EnvironmentName => {hostEnvironment.EnvironmentName}");
    //
    //     return Task.CompletedTask;
    // }
}