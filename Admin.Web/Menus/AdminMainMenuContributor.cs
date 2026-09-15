using Admin.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Volo.Abp.UI.Navigation;

namespace Admin.Web.Menus;

/// 左侧菜单（工作台/系统管理/日志审计/任务调度），驱动 layui 侧边栏渲染。
/// layui 仅支持「分组 + 叶子」两级结构。
public class AdminMainMenuContributor : IMenuContributor
{
    public virtual Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return Task.CompletedTask;
        }

        ConfigureMainMenu(context);
        return Task.CompletedTask;
    }

    private void ConfigureMainMenu(MenuConfigurationContext context)
    {
        var localizer = context.ServiceProvider.GetRequiredService<IStringLocalizer<AdminResource>>();
        var menu = context.Menu;

        // 工作台
        menu.AddItem(
            new ApplicationMenuItem(
                "Workbench",
                localizer["Menu:Workbench"],
                icon: "layui-icon-home",
                order: 1)
            .AddItem(new ApplicationMenuItem(
                "Workbench.Console",
                localizer["Menu:Console"],
                url: "/Home/Console",
                icon: "layui-icon-dialogue",
                order: 1))
        );

        // 系统管理
        menu.AddItem(
            new ApplicationMenuItem(
                "System",
                localizer["Menu:System"],
                icon: "layui-icon-set",
                order: 2)
            .AddItem(new ApplicationMenuItem(
                    "System.Users",
                    localizer["Menu:Users"],
                    url: "/Admin/Users",
                    icon: "layui-icon-user",
                    order: 1))
            .AddItem(new ApplicationMenuItem(
                    "System.Roles",
                    localizer["Menu:Roles"],
                    url: "/Admin/Roles",
                    icon: "layui-icon-auz",
                    order: 2))
            .AddItem(new ApplicationMenuItem(
                    "System.OrganizationUnits",
                    localizer["Menu:OrganizationUnits"],
                    url: "/Admin/OrganizationUnits",
                    icon: "layui-icon-group",
                    order: 3))
            .AddItem(new ApplicationMenuItem(
                    "System.Permissions",
                    localizer["Menu:Permissions"],
                    url: "/Admin/Permissions",
                    icon: "layui-icon-password",
                    order: 4))
        );

        // 日志审计
        menu.AddItem(
            new ApplicationMenuItem(
                "Logs",
                localizer["Menu:Logs"],
                icon: "layui-icon-log",
                order: 3)
            .AddItem(new ApplicationMenuItem(
                "Logs.AuditLogs",
                localizer["Menu:AuditLogs"],
                url: "/Admin/AuditLogs",
                icon: "layui-icon-log",
                order: 1))
        );

        // 任务调度
        menu.AddItem(
            new ApplicationMenuItem(
                "Jobs",
                localizer["Menu:Jobs"],
                icon: "layui-icon-time",
                order: 4)
            .AddItem(new ApplicationMenuItem(
                "Jobs.ScheduledJobs",
                localizer["Menu:ScheduledJobs"],
                url: "/Admin/Jobs",
                icon: "layui-icon-time",
                order: 1))
        );
    }
}
