using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Volo.Abp.Study.Web.Components.ContactsSyncSettings;

public class ContactsSyncSettingsViewComponent : AbpViewComponent
{
    /*
     * 参考链接
     * https://github.com/abpframework/abp/tree/7.4.3/modules/setting-management/src/Volo.Abp.SettingManagement.Web/Pages/SettingManagement/Components/EmailSettingGroup
     */

    public virtual async Task<IViewComponentResult> InvokeAsync()
    {
        return View("~/Components/ContactsSyncSettings/Default.cshtml", new UpdateContactsSettingsViewModel());
    }
}

public class UpdateContactsSettingsViewModel
{
    [Display(Name = "SyncEnabled")]
    public bool SyncEnabled { get; set; }

    [Display(Name = "SyncProviderName")]
    [Required]
    public string ProviderName { get; set; }
}