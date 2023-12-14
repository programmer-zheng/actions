using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Volo.Abp.SettingManagement.Web.Pages.SettingManagement;
using Volo.Abp.Study.Localization;
using Volo.Abp.Study.Web.Components.ContactsSyncSettings;

namespace Volo.Abp.Study.Web.Settings;

public class VoloAbpStudySettingPageContributor : ISettingPageContributor
{
    public Task ConfigureAsync(SettingPageCreationContext context)
    {
        var L = context.ServiceProvider.GetRequiredService<IStringLocalizer<StudyResource>>();
        context.Groups.Add(
            new SettingPageGroup(
                "Volo.Abp.Study.ContactsSyncSetting",
                L["ContactsSyncSetting"],
                typeof(ContactsSyncSettingsViewComponent),
                order: 1
            )
        );

        return Task.CompletedTask;
    }

    public Task<bool> CheckPermissionsAsync(SettingPageCreationContext context)
    {
        return Task.FromResult(true);
    }
}