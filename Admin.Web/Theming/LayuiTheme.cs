using Volo.Abp.AspNetCore.Mvc.UI.Theming;
using Volo.Abp.DependencyInjection;

namespace Admin.Web.Theming;

/// <summary>
/// 占位主题。
/// 本项目页面全部由 layui 自定义视图承载，不需要 ABP 主题；
/// 但 ABP 内建视图（如 OpenIddict 的 /Volo/Abp/OpenIddict/Views/Authorize/Authorize.cshtml）
/// 的 _ViewStart 会通过 <see cref="IThemeManager"/> 取布局，
/// 若 <see cref="AbpThemingOptions.Themes"/> 为空会抛
/// "No theme registered! Use AbpThemingOptions to register themes."。
/// 因此这里注册一个指向本站布局的轻量主题。
/// </summary>
[ThemeName(Name)]
public class LayuiTheme : ITheme, ITransientDependency
{
    public const string Name = "Layui";

    public string GetLayout(string name, bool fallbackToDefault = true)
    {
        return "~/Views/Shared/_Layout.cshtml";
    }
}
