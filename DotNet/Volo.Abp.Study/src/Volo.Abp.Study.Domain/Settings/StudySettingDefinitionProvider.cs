using Volo.Abp.Emailing;
using Volo.Abp.Settings;

namespace Volo.Abp.Study.Settings;

public class StudySettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(StudySettings.MySetting1));

        context.Add(
            new SettingDefinition(StudySettings.ProviderName, "DingDing"),
            new SettingDefinition(StudySettings.SyncEnabled, "false"),

            // dingding
            new SettingDefinition(StudySettings.DingDing.CorpId),
            new SettingDefinition(StudySettings.DingDing.AgentId),
            new SettingDefinition(StudySettings.DingDing.AppKey),
            new SettingDefinition(StudySettings.DingDing.AppSecret),
            new SettingDefinition(StudySettings.DingDing.AesKey),
            new SettingDefinition(StudySettings.DingDing.Token),
            new SettingDefinition(StudySettings.DingDing.ProcessCode),

            // wework
            new SettingDefinition(StudySettings.WeWork.CorpId),
            new SettingDefinition(StudySettings.WeWork.AgentId),
            new SettingDefinition(StudySettings.WeWork.AppSecret),
            new SettingDefinition(StudySettings.WeWork.Token),
            new SettingDefinition(StudySettings.WeWork.EncodingAESKey),
            new SettingDefinition(StudySettings.WeWork.TemplateId)
        );
    }
}