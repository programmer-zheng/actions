using Volo.Abp.Modularity;

namespace Volo.Abp.Study;

[DependsOn(
    typeof(StudyApplicationModule),
    typeof(StudyDomainTestModule)
    )]
public class StudyApplicationTestModule : AbpModule
{

}
