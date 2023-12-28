using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Volo.Abp.Study.CustomSetting;

// [RemoteService(Name = SettingManagementRemoteServiceConsts.RemoteServiceName)]
// [Area(SettingManagementRemoteServiceConsts.ModuleName)]
// [Route("/api/setting-management/contactssettings")]

// [RemoteService(Name = "CustomSetting")]
// [Area("customSetting")]
//  [Route("/api/custom-setting/contacts")]
public class ContactsSyncSettingsController : AbpControllerBase, IContactsSyncSettingsAppService
{
    private readonly IContactsSyncSettingsAppService _contactsSyncSettingAppService;

    public ContactsSyncSettingsController(IContactsSyncSettingsAppService contactsSyncSettingAppService)
    {
        _contactsSyncSettingAppService = contactsSyncSettingAppService;
    }
     // [HttpGet]
    public Task<ContactsSyncSettingsDto> GetAsync()
    {
        return _contactsSyncSettingAppService.GetAsync();
    }

    /// <summary>
    /// controller中
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    // [HttpPost]
    public Task UpdateAsync(UpdateContactsSyncSettingsDto input)
    {
        return _contactsSyncSettingAppService.UpdateAsync(input);
    }
}