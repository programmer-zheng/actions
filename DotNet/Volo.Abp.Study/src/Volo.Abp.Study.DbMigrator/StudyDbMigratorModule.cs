using Volo.Abp.Study.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Volo.Abp.Study.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(StudyEntityFrameworkCoreModule),
    typeof(StudyApplicationContractsModule)
    )]
public class StudyDbMigratorModule : AbpModule
{
}
