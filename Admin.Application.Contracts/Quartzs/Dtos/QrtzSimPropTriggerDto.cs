using System;
using Volo.Abp.Application.Dtos;

namespace Admin.Quartzs.Dtos
{
    public class QrtzSimPropTriggerDto : EntityDto<Guid>
    {
        public Guid TriggerId { get; set; }
        public string StrProp1 { get; set; } = null!;
        public string StrProp2 { get; set; } = null!;
        public string StrProp3 { get; set; } = null!;
        public int? IntProp1 { get; set; }
        public int? IntProp2 { get; set; }
        public long? LongProp1 { get; set; }
        public long? LongProp2 { get; set; }
        public decimal? DecProp1 { get; set; }
        public decimal? DecProp2 { get; set; }
        public bool? BoolProp1 { get; set; }
        public bool? BoolProp2 { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }
}
