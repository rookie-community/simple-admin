using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;

namespace Admin.Quartz
{
    /// <summary>
    /// 触发器主表
    /// </summary>
    public class QrtzTrigger : FullAuditedAggregateRoot<Guid>
    {
        public string TriggerName { get; set; } = null!;
        public string TriggerGroup { get; set; } = null!;
        public Guid JobId { get; set; }

        /// <summary>
        /// 触发器类型（枚举）
        /// </summary>
        public TriggerType TriggerType { get; set; }

        public int Priority { get; set; } = 5;
        public bool IsEnabled { get; set; } = true;
        public string Description { get; set; } = null!;

        public QrtzJob Job { get; set; }

        // 导航属性：子表，根据 TriggerType 关联不同的子表
        public QrtzCronTrigger CronTrigger { get; set; } = null!;
        public QrtzSimpleTrigger SimpleTrigger { get; set; } = null!;
        public QrtzSimPropTrigger SimPropTrigger { get; set; } = null!;
    }
}
