namespace Admin.Quartz
{
    /// <summary>
    /// 触发器类型枚举
    /// </summary>
    public enum TriggerType
    {
        /// <summary>
        /// Cron 触发器
        /// </summary>
        Cron,

        /// <summary>
        /// Simple 触发器（固定次数/间隔）
        /// </summary>
        Simple,

        /// <summary>
        /// 日历周期触发器（按天/周/月/年等日历单位）
        /// </summary>
        CalendarInterval,

        /// <summary>
        /// 每日时间间隔触发器（在指定时间段内按固定间隔）
        /// </summary>
        DailyTimeInterval
    }
}
