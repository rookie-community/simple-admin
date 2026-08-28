using Admin.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Admin.Permissions
{
    public class QuartzPermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var quartzGroup = context.AddGroup(QuartzPermissions.GroupName, L("Permission:Quartz"));

            // Job权限
            var jobsPermission = quartzGroup.AddPermission(QuartzPermissions.Jobs.Default, L("Permission:Jobs"));
            jobsPermission.AddChild(QuartzPermissions.Jobs.Create, L("Permission:Jobs.Create"));
            jobsPermission.AddChild(QuartzPermissions.Jobs.Edit, L("Permission:Jobs.Edit"));
            jobsPermission.AddChild(QuartzPermissions.Jobs.Delete, L("Permission:Jobs.Delete"));

            // Job权限
            var triggersPermission = quartzGroup.AddPermission(QuartzPermissions.Triggers.Default, L("Permission:Triggers"));
            triggersPermission.AddChild(QuartzPermissions.Triggers.Create, L("Permission:Triggers.Create"));
            triggersPermission.AddChild(QuartzPermissions.Triggers.Edit, L("Permission:Triggers.Edit"));
            triggersPermission.AddChild(QuartzPermissions.Triggers.Delete, L("Permission:Triggers.Delete"));
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<AdminResource>(name);
        }
    }

}
