using Volo.Abp.Study.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Volo.Abp.Study.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class StudyController : AbpControllerBase
{
    protected StudyController()
    {
        LocalizationResource = typeof(StudyResource);
    }
}
