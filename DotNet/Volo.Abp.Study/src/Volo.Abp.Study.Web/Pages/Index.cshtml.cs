using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace Volo.Abp.Study.Web.Pages;

public class IndexModel : StudyPageModel
{
    public void OnGet()
    {

    }

    public async Task OnPostLoginAsync()
    {
        await HttpContext.ChallengeAsync("oidc");
    }
}
