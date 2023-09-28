using Volo.Abp.Study.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Volo.Abp.Study.Permissions;

public class StudyPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(StudyPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(StudyPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<StudyResource>(name);
    }
}
