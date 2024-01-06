using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Volo.Abp.SettingManagement;
using Volo.Abp.SettingManagement.Web.Pages.SettingManagement;
using Volo.Abp.Study.Localization;
using Volo.Abp.Study.Permissions;
using Volo.Abp.Study.Web.Components.ContactsSyncSettings;

namespace Volo.Abp.Study.Web.Settings;

public class VoloAbpStudySettingPageContributor : SettingPageContributorBase
{
    public VoloAbpStudySettingPageContributor()
    {
        RequiredTenantSideFeatures(SettingManagementFeatures.Enable);
        RequiredPermissions(StudyPermissions.ContactSetting);
    }
    public override Task ConfigureAsync(SettingPageCreationContext context)
    {
        var l = context.ServiceProvider.GetRequiredService<IStringLocalizer<StudyResource>>();
        context.Groups.Add(
            new SettingPageGroup(
                "Volo.Abp.Study.ContactsSyncSetting",
                l["ContactsSyncSetting"],
                typeof(ContactsSyncSettingsViewComponent),
                order: 1
            )
        );

        return Task.CompletedTask;
    }
}