using Admin.Quartz;
using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Admin.Quartzs.Dtos
{
    public class CreateUpdateQrtzTriggerDto : EntityDto
    {
        [Required]
        [MaxLength(100)]
        public string TriggerName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string TriggerGroup { get; set; } = null!;

        [Required]
        public Guid JobId { get; set; }

        [Required]
        public TriggerType TriggerType { get; set; }

        public int Priority { get; set; } = 5;
        public bool IsEnabled { get; set; } = true;
        public string Description { get; set; } = null!;
    }
}
