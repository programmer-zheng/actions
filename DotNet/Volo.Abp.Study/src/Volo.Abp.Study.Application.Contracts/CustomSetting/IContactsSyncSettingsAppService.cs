using Volo.Abp.Application.Services;

namespace Volo.Abp.Study.CustomSetting;

public interface IContactsSyncSettingsAppService : IApplicationService
{

    Task<ContactsSyncSettingsDto> GetAsync();
    
    Task UpdateAsync(UpdateContactsSyncSettingsDto input);
}