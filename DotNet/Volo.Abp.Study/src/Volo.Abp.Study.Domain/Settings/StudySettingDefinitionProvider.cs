using Volo.Abp.Settings;

namespace Volo.Abp.Study.Settings;

public class StudySettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(StudySettings.MySetting1));
    }
}
