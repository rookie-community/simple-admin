using System;
using Admin.Quartz;
using Volo.Abp.Application.Dtos;

namespace Admin.Quartzs
{
    public class QrtzTriggerDto : EntityDto<Guid>
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
    }
}
