using Volo.Abp.Study.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Volo.Abp.Study.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class StudyPageModel : AbpPageModel
{
    protected StudyPageModel()
    {
        LocalizationResourceType = typeof(StudyResource);
    }
}
