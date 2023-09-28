using Volo.Abp.Study.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Volo.Abp.Study;

[DependsOn(
    typeof(StudyEntityFrameworkCoreTestModule)
    )]
public class StudyDomainTestModule : AbpModule
{

}
