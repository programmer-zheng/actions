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

        myGroup.AddPermission(StudyPermissions.ContactSetting, L("Permission:ContractSetting"));
        
        
        

        var flatInfoPermission = myGroup.AddPermission(StudyPermissions.FlatInfo.Default, L("Permission:FlatInfo"));
        flatInfoPermission.AddChild(StudyPermissions.FlatInfo.Create, L("Permission:Create"));
        flatInfoPermission.AddChild(StudyPermissions.FlatInfo.Update, L("Permission:Update"));
        flatInfoPermission.AddChild(StudyPermissions.FlatInfo.Delete, L("Permission:Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<StudyResource>(name);
    }
}
