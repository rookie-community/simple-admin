using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Admin.Quartz
{
    /// <summary>
    /// Simple 触发器子表
    /// </summary>
    public class QrtzSimpleTrigger : FullAuditedAggregateRoot<Guid>
    {
        public Guid TriggerId { get; set; }
        public int RepeatCount { get; set; }
        public int IntervalSeconds { get; set; }

        public QrtzTrigger Trigger { get; set; } = null!;
    }
}
