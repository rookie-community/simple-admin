using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Admin.Quartz
{
    /// <summary>
    /// Cron 触发器子表
    /// </summary>
    public class QrtzCronTrigger : FullAuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// 外键 QrtzTrigger.Id
        /// </summary>
        public Guid TriggerId { get; set; }

        public string CronExpression { get; set; } = null!;

        /// <summary>
        /// 可选，默认 UTC
        /// </summary>
        public string TimeZoneId { get; set; } = null!;

        public QrtzTrigger Trigger { get; set; } = null!;
    }
}
