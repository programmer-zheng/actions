using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Study.Localization;
using Volo.Abp.Validation.StringValues;

namespace Volo.Abp.Study
{
    public class StudyFeatureDefinitionProvider : FeatureDefinitionProvider
    {
        // 功能定义
        public override void Define(IFeatureDefinitionContext context)
        {
            var group = context.AddGroup("MyAppGroup", L("Feature:UserManageGroup"));
            group.AddFeature(
                "MyApp.LimitMaxUserCount",
                defaultValue: "false",
                displayName: L("Feature:LimitMaxUserCount"),
                isVisibleToClients: true,
                valueType: new ToggleStringValueType()
                );

            group.AddFeature(
                "MyApp.MaxUserCount",
                defaultValue: "100",
                displayName: L("Feature:MaxUserCount"),
                valueType: new FreeTextStringValueType(new NumericValueValidator())
                );
        }

        private ILocalizableString L(string name)
        {
            return LocalizableString.Create<StudyResource>(name);
        }
    }
}
