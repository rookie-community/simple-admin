using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace Admin.Quartz
{
    /// <summary>
    /// Job 配置表
    /// </summary>
    public class QrtzJob : FullAuditedAggregateRoot<Guid>
    {
        public string JobName { get; set; } = null!;
        public string JobGroup { get; set; } = null!;
        public string JobClassName { get; set; } = null!;
        public bool IsDisallowConcurrent { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string Description { get; set; } = null!;

        public ICollection<QrtzTrigger> Triggers { get; set; } = null!;
    }
}
