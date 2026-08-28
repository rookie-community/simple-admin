using Admin.Quartz;
using Quartz;
using System.Text.Json;

namespace Admin.Utils
{
    public static class QuartzConverter
    {
        public static IJobDetail CreateJobDetail(QrtzJob jobConfig)
        {
            var jobType = Type.GetType(jobConfig.JobClassName);
            if (jobType == null)
            {
                throw new InvalidOperationException($"无法加载 Job 类型: {jobConfig.JobClassName}");
            }

            var builder = JobBuilder.Create(jobType)
                .WithIdentity(jobConfig.JobName, jobConfig.JobGroup)
                .StoreDurably();

            if (jobConfig.IsDisallowConcurrent)
            {
                builder.DisallowConcurrentExecution(true);
            }

            return builder.Build();
        }

        public static ITrigger CreateTrigger(QrtzTrigger triggerConfig)
        {
            var builder = TriggerBuilder.Create()
                .WithIdentity(triggerConfig.TriggerName, triggerConfig.TriggerGroup)
                .ForJob(triggerConfig.Job.JobName, triggerConfig.Job.JobGroup)
                .WithPriority(triggerConfig.Priority);

            switch (triggerConfig.TriggerType)
            {
                case TriggerType.Cron:
                    var cron = triggerConfig.CronTrigger;
                    builder.WithCronSchedule(cron.CronExpression, x =>
                    {
                        if (!string.IsNullOrEmpty(cron.TimeZoneId))
                        {
                            x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(cron.TimeZoneId));
                        }
                    });
                    break;

                case TriggerType.Simple:
                    var simple = triggerConfig.SimpleTrigger;
                    builder.WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(simple.IntervalSeconds)
                        .WithRepeatCount(simple.RepeatCount));
                    break;

                case TriggerType.CalendarInterval:
                    var cal = triggerConfig.SimPropTrigger;
                    var unit = Enum.Parse<IntervalUnit>(cal.StrProp1);
                    var interval = cal.IntProp1 ?? 1;
                    builder.WithCalendarIntervalSchedule(x => x
                        .WithInterval(interval, unit));
                    break;

                case TriggerType.DailyTimeInterval:
                    var daily = triggerConfig.SimPropTrigger;

                    // 解析 JSON 数组
                    var daysOfWeek = JsonSerializer.Deserialize<List<int>>(daily.StrProp3)?.Select(d => (DayOfWeek)d).ToArray();

                    var startTime = TimeSpan.Parse(daily.StrProp1);
                    var endTime = TimeSpan.Parse(daily.StrProp2);
                    builder.WithDailyTimeIntervalSchedule(x => x
                        .WithIntervalInSeconds(daily.IntProp1 ?? 60)
                        .StartingDailyAt(TimeOfDay.HourMinuteAndSecondOfDay(startTime.Hours, startTime.Minutes, startTime.Seconds))
                        .EndingDailyAt(TimeOfDay.HourMinuteAndSecondOfDay(endTime.Hours, endTime.Minutes, endTime.Seconds))
                        .OnDaysOfTheWeek(daysOfWeek!));
                    break;

                // 注意：RecurrenceTrigger 需要 Quartz.NET 3.18+ 才支持
                // ABP vNext 当前集成版本通常是 3.15，不支持
                // 如需支持，请升级 Quartz 版本或改用 Cron 表达式
                // case TriggerType.Recurrence:
                //     var rec = triggerConfig.SimPropTrigger;
                //     builder.WithRecurrenceSchedule(rec.StrProp1);
                //     break;

                default:
                    throw new NotSupportedException($"不支持的触发器类型: {triggerConfig.TriggerType}");
            }

            return builder.Build();
        }
    }
}
