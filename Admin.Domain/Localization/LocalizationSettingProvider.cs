using Volo.Abp.Localization;
using Volo.Abp.Localization.Resources.AbpLocalization;
using Volo.Abp.Settings;

namespace Admin.Localization;

public class LocalizationSettingProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(LocalizationSettingNames.DefaultLanguage,
                "zh-Hans",
                L("DisplayName:Abp.Localization.DefaultLanguage"),
                L("Description:Abp.Localization.DefaultLanguage"),
                isVisibleToClients: true)
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AbpLocalizationResource>(name);
    }
}