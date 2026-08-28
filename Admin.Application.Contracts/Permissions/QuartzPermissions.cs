namespace Admin.Permissions
{
    public static class QuartzPermissions
    {
        public const string GroupName = "Quartz";

        public static class Jobs
        {
            public const string Default = GroupName + ".Jobs";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
            public const string Pause = Default + ".Pause";
            public const string Resume = Default + ".Resume";
            public const string Trigger = Default + ".Trigger";
        }

        public static class Triggers
        {
            public const string Default = GroupName + ".Triggers";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }

        public static class CronTriggers
        {
            public const string Default = GroupName + ".CronTriggers";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }

        public static class SimpleTriggers
        {
            public const string Default = GroupName + ".SimpleTriggers";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
        }

        public static class SimPropTriggers
        {
            public const string Default = GroupName + ".SimPropTriggers";
            public const string Create = Default + ".Create";
            public const string Update = Default + ".Update";
            public const string Delete = Default + ".Delete";
        }
    }
}
