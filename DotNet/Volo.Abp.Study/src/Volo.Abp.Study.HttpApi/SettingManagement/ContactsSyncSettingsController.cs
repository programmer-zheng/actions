using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Study.ContactsSetting;

namespace Volo.Abp.Study.SettingManagement;

// [RemoteService(Name = SettingManagementRemoteServiceConsts.RemoteServiceName)]
// [Area(SettingManagementRemoteServiceConsts.ModuleName)]
// [Route("/api/ContactsSetting/settings")]
// [Route("/api/setting-management/contactssettings")]
public class ContactsSyncSettingsController : AbpControllerBase, IContactsSyncSettingsAppService
{
    private readonly IContactsSyncSettingsAppService _contactsSyncSettingAppService;

    public ContactsSyncSettingsController(IContactsSyncSettingsAppService contactsSyncSettingAppService)
    {
        _contactsSyncSettingAppService = contactsSyncSettingAppService;
    }

    /// <summary>
    /// controller中
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public Task UpdateAsync(UpdateContactsSyncSettingsDto input)
    {
        return _contactsSyncSettingAppService.UpdateAsync(input);
    }
}