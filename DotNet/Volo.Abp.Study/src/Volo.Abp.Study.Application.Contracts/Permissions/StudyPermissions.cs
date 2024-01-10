namespace Volo.Abp.Study.Permissions;

public static class StudyPermissions
{
    public const string GroupName = "Study";

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";

    public const string ContactSetting = GroupName + ".ContactSetting";
    public class FlatInfo
    {
        public const string Default = GroupName + ".FlatInfo";
        public const string Update = Default + ".Update";
        public const string Create = Default + ".Create";
        public const string Delete = Default + ".Delete";
    }
}
