using Microsoft.AspNetCore.Authorization;

namespace Volo.Abp.Study.Web.Pages;

[Authorize]
public class IndexModel : StudyPageModel
{
    public void OnGet()
    {

    }
}
