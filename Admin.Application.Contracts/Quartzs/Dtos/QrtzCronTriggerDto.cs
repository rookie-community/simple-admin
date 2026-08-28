using System;
using Volo.Abp.Application.Dtos;

namespace Admin.Quartzs.Dtos
{
    public class QrtzCronTriggerDto : EntityDto<Guid>
    {
        public Guid TriggerId { get; set; }
        public string CronExpression { get; set; } = null!;
        public string TimeZoneId { get; set; } = null!;
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }
}
