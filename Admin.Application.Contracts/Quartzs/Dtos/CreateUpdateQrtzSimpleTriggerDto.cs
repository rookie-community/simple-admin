using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Admin.Quartzs.Dtos
{
    public class CreateUpdateQrtzSimpleTriggerDto : EntityDto
    {
        [Required]
        public Guid TriggerId { get; set; }

        public int RepeatCount { get; set; } = -1;

        [Required]
        public int IntervalSeconds { get; set; }
    }
}
