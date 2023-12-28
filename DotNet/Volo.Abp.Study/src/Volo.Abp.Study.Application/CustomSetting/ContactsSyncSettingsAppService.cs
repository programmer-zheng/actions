using Volo.Abp.Application.Services;
using Volo.Abp.SettingManagement;
using Volo.Abp.Study.Settings;

namespace Volo.Abp.Study.CustomSetting;

public class ContactsSyncSettingsAppService : ApplicationService, IContactsSyncSettingsAppService
{
    private readonly ISettingManager _settingManager;

    public ContactsSyncSettingsAppService(ISettingManager settingManager)
    {
        _settingManager = settingManager;
    }

    
    public virtual async Task<ContactsSyncSettingsDto> GetAsync()
    {
        var settingsDto = new ContactsSyncSettingsDto
        {
            SyncEnabled = Convert.ToBoolean(await SettingProvider.GetOrNullAsync(StudySettings.SyncEnabled)),
            ProviderName = await SettingProvider.GetOrNullAsync(StudySettings.ProviderName),
        };

        return settingsDto;
    }

    /// <summary>
    /// 更新通讯录同步设置
    /// </summary>
    /// <param name="input"></param>
    // [RemoteService(false)]
    public virtual async Task UpdateAsync(UpdateContactsSyncSettingsDto input)
    {
        // 
        // await _settingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, StudySettings.SyncEnabled, input.SyncEnabled.ToString());
        await _settingManager.SetGlobalAsync(StudySettings.SyncEnabled, input.SyncEnabled.ToString());
        await _settingManager.SetGlobalAsync(StudySettings.ProviderName, input.ProviderName);
    }
}