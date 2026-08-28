using System;
using Volo.Abp.Application.Dtos;

namespace Admin.Quartzs.Dtos
{
    public class QrtzSimpleTriggerDto : EntityDto<Guid>
    {
        public Guid TriggerId { get; set; }
        public int RepeatCount { get; set; }
        public long IntervalMilliseconds { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }
}
