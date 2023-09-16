using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;

namespace VideoMerge;

[DependsOn(typeof(AbpAutofacModule),
    typeof(AbpBackgroundWorkersModule))]
public class AppModule : AbpModule
{
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<AutoVideoMergeWorker>();
    }
}