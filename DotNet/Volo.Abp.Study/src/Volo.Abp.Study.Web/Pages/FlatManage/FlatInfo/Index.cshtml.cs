using System;
using System.Threading.Tasks;
using Volo.Abp.Study.Web.Pages.FlatManage.FlatInfo.ViewModels;

namespace Volo.Abp.Study.Web.Pages.FlatManage.FlatInfo;

public class IndexModel : StudyPageModel
{
    public FlatInfoFilterInput FlatInfoFilter { get; set; }
    
    public virtual async Task OnGetAsync()
    {
        await Task.CompletedTask;
    }
}