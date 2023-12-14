using Volo.Abp.Application.Services;
using Volo.Abp.SettingManagement;
using Volo.Abp.Study.ContactsSetting;
using Volo.Abp.Study.Settings;

namespace Volo.Abp.Study.SettingManagement;

public class ContactsSyncSettingsAppService : ApplicationService, IContactsSyncSettingsAppService
{
    private readonly ISettingManager _settingManager;

    public ContactsSyncSettingsAppService(ISettingManager settingManager)
    {
        _settingManager = settingManager;
    }

    /// <summary>
    /// 更新通讯录同步设置
    /// </summary>
    /// <param name="input"></param>
    // [RemoteService(false)]
    public async Task UpdateAsync(UpdateContactsSyncSettingsDto input)
    {
        await _settingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, StudySettings.SyncEnabled, input.SyncEnabled.ToString());
        await _settingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, StudySettings.ProviderName, input.ProviderName);
    }
}