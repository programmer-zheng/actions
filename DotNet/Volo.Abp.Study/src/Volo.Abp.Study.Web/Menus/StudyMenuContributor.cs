using System.Threading.Tasks;
using Volo.Abp.Study.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Study.Localization;
using Volo.Abp.Study.MultiTenancy;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.Web.Navigation;
using Volo.Abp.UI.Navigation;

namespace Volo.Abp.Study.Web.Menus;

public class StudyMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<StudyResource>();
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
        else if (context.Menu.Name == StandardMenus.User)
        {
            context.Menu.Items.Insert(0, new ApplicationMenuItem(StudyMenus.UserIndex, l["Menu:UserIndex"], "~/User/Index", icon: "fa fa-users", order: 0));
        }
        else if (context.Menu.Name == StandardMenus.Shortcut)
        {
            context.Menu.Items.Insert(0, new ApplicationMenuItem(StudyMenus.ShortIndex, l["Menu:ShortIndex"], "~/Short/Index", icon: "fa fa-tools", order: 0));
        }
    }

    private async Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var administration = context.Menu.GetAdministration();
        var l = context.GetLocalizer<StudyResource>();


        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                StudyMenus.Home,
                l["Menu:Home"],
                "~/",
                icon: "fas fa-home",
                order: 0
            ).RequireAuthenticated()
        );

        // if (await context.IsGrantedAsync(StudyPermissions.FlatInfo.Default))
        // {
        //     context.Menu.GetAdministration().AddItem(
        //         new ApplicationMenuItem(StudyMenus.FlatInfo, l["Menu:FlatInfo"], "/FlatManage/FlatInfo")
        //     );
        // }
        context.Menu.Items.Insert(
            1,
            new ApplicationMenuItem(
                StudyMenus.FlatInfo, l["Menu:FlatInfo"], "/FlatManage/FlatInfo",
                icon: "fa-solid fa-shop",
                order: 1
            ).RequirePermissions(StudyPermissions.FlatInfo.Default)
        );
        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        }

        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 3);

        var firstLevel =
            new ApplicationMenuItem(StudyMenus.FirstLevel, l["Menu:FirstLevel"], icon: "fas fa-bars")
                .RequirePermissions(TenantManagementPermissions.Tenants.Default);

        administration.AddItem(firstLevel);

        // 若子项中未设置 URL，则无法正常显示，若存在子项，则父级的 URL 设置也无效果，点击只会展示
        // 二层目录中存在子项，二层目录设置了 URL ，点击二层目录，只会展开菜单，而不会跳转至二层目录所指定的 URL
        firstLevel.AddItem(new ApplicationMenuItem(StudyMenus.SecondLevel, l["Menu:SecondLevel"], "~/SecondLevel"));

        var secondLevel = context.Menu.GetAdministration().GetMenuItemOrNull(StudyMenus.FirstLevel)?.GetMenuItemOrNull(StudyMenus.SecondLevel);
        if (secondLevel != null)
        {
            secondLevel.AddItem(new ApplicationMenuItem(StudyMenus.ThirdLevel, l["Menu:ThirdLevel"], "~/ThirdLevel"));
        }


    }
}
