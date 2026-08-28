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
        }

        public static class Triggers
        {
            public const string Default = GroupName + ".Triggers";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }
    }
}
