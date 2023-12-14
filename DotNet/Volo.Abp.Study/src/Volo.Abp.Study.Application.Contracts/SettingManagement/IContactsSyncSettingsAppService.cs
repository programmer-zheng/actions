using Volo.Abp.Application.Services;
using Volo.Abp.Study.ContactsSetting;

namespace Volo.Abp.Study.SettingManagement;

public interface IContactsSyncSettingsAppService : IApplicationService
{

    Task<ContactsSyncSettingsDto> GetAsync();
    
    Task UpdateAsync(UpdateContactsSyncSettingsDto input);
}