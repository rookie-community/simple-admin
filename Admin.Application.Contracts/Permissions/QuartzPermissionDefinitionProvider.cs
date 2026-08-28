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

            // ========== Job 权限 ==========
            var jobsPermission = quartzGroup.AddPermission(QuartzPermissions.Jobs.Default, L("Permission:Jobs"));
            jobsPermission.AddChild(QuartzPermissions.Jobs.Create, L("Permission:Jobs.Create"));
            jobsPermission.AddChild(QuartzPermissions.Jobs.Edit, L("Permission:Jobs.Edit"));
            jobsPermission.AddChild(QuartzPermissions.Jobs.Delete, L("Permission:Jobs.Delete"));
            jobsPermission.AddChild(QuartzPermissions.Jobs.Pause, L("Permission:Jobs.Pause"));
            jobsPermission.AddChild(QuartzPermissions.Jobs.Resume, L("Permission:Jobs.Resume"));
            jobsPermission.AddChild(QuartzPermissions.Jobs.Trigger, L("Permission:Jobs.Trigger"));

            // ========== Trigger 权限 ==========
            var triggersPermission = quartzGroup.AddPermission(QuartzPermissions.Triggers.Default, L("Permission:Triggers"));
            triggersPermission.AddChild(QuartzPermissions.Triggers.Create, L("Permission:Triggers.Create"));
            triggersPermission.AddChild(QuartzPermissions.Triggers.Edit, L("Permission:Triggers.Edit"));
            triggersPermission.AddChild(QuartzPermissions.Triggers.Delete, L("Permission:Triggers.Delete"));

            // ========== Cron Trigger 权限 ==========
            var cronTriggersPermission = quartzGroup.AddPermission(QuartzPermissions.CronTriggers.Default, L("Permission:CronTriggers"));
            cronTriggersPermission.AddChild(QuartzPermissions.CronTriggers.Create, L("Permission:CronTriggers.Create"));
            cronTriggersPermission.AddChild(QuartzPermissions.CronTriggers.Edit, L("Permission:CronTriggers.Edit"));
            cronTriggersPermission.AddChild(QuartzPermissions.CronTriggers.Delete, L("Permission:CronTriggers.Delete"));

            // ========== Simple Trigger 权限 ==========
            var simpleTriggersPermission = quartzGroup.AddPermission(QuartzPermissions.SimpleTriggers.Default, L("Permission:SimpleTriggers"));
            simpleTriggersPermission.AddChild(QuartzPermissions.SimpleTriggers.Create, L("Permission:SimpleTriggers.Create"));
            simpleTriggersPermission.AddChild(QuartzPermissions.SimpleTriggers.Update, L("Permission:SimpleTriggers.Update"));
            simpleTriggersPermission.AddChild(QuartzPermissions.SimpleTriggers.Delete, L("Permission:SimpleTriggers.Delete"));

            // ========== SimProp Trigger 权限 ==========
            var simPropTriggersPermission = quartzGroup.AddPermission(QuartzPermissions.SimPropTriggers.Default, L("Permission:SimPropTriggers"));
            simPropTriggersPermission.AddChild(QuartzPermissions.SimPropTriggers.Create, L("Permission:SimPropTriggers.Create"));
            simPropTriggersPermission.AddChild(QuartzPermissions.SimPropTriggers.Update, L("Permission:SimPropTriggers.Update"));
            simPropTriggersPermission.AddChild(QuartzPermissions.SimPropTriggers.Delete, L("Permission:SimPropTriggers.Delete"));
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<AdminResource>(name);
        }
    }

}
