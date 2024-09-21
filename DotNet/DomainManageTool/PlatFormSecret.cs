namespace DomainManageTool
{
    public class PlatFormSecret
    {
        public const string SecretIdVariable = "SecretId";
        public const string SecretKeyVariable = "SecretKey";

        public PlatFormSecret(string id, string key)
        {
            SecretId = id;
            SecretKey = key;
        }

        public string SecretId { get; set; }

        public string SecretKey { get; set; }
    }
}