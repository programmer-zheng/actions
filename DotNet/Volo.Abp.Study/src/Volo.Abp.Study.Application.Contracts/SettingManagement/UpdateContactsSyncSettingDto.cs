namespace Volo.Abp.Study.ContactsSetting;

public class UpdateContactsSyncSettingsDto
{
    public bool SyncEnabled { get; set; }
    public string ProviderName { get; set; }
}