using Volo.Abp.Application.Services;
using Volo.Abp.Study.ContactsSetting;

namespace Volo.Abp.Study.SettingManagement;

public interface IContactsSyncSettingsAppService : IApplicationService
{
    Task UpdateAsync(UpdateContactsSyncSettingsDto input);
}