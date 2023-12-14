namespace Volo.Abp.Study.Settings;

public static class StudySettings
{
    private const string Prefix = "Study";

    //Add your own setting names here. Example:
    //public const string MySetting1 = Prefix + ".MySetting1";


    public const string SyncEnabled = Prefix + ".SyncEnabled";

    public const string ProviderName = Prefix + ".ProviderName";

    public static class DingDing
    {
        public const string CorpId = Prefix + ".DingDing.CorpId";
        public const string AgentId = Prefix + ".DingDing.AgentId";
        public const string AppKey = Prefix + ".DingDing.AppKey";
        public const string AppSecret = Prefix + ".DingDing.AppSecret";
        public const string AesKey = Prefix + ".DingDing.AesKey";
        public const string Token = Prefix + ".DingDing.Token";
        public const string ProcessCode = Prefix + ".DingDing.ProcessCode";
    }

    public static class WeWork
    {
        public const string CorpId = Prefix + ".WeWork.CorpId";
        public const string AgentId = Prefix + ".WeWork.AgentId";
        public const string AppSecret = Prefix + ".WeWork.AppSecret";
        public const string Token = Prefix + ".WeWork.Token";
        public const string EncodingAESKey = Prefix + ".WeWork.EncodingAESKey";
        public const string TemplateId = Prefix + ".WeWork.TemplateId";
    }
}