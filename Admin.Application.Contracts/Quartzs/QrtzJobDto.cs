using System;
using Volo.Abp.Application.Dtos;

namespace Admin.Quartzs
{
    public class QrtzJobDto : EntityDto<Guid>
    {
        public string JobName { get; set; } = null!;
        public string JobGroup { get; set; } = null!;
        public string JobClassName { get; set; } = null!;
        public bool IsDisallowConcurrent { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string Description { get; set; } = null!;

        public DateTime CreationTime { get; set; }

        public DateTime LastModificationTime { get; set; }
    }
}
