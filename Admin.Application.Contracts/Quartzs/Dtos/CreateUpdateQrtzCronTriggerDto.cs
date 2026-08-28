using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Admin.Quartzs.Dtos
{
    public class CreateUpdateQrtzCronTriggerDto : EntityDto
    {
        [Required]
        public Guid TriggerId { get; set; }

        [Required]
        public string CronExpression { get; set; } = null!;

        public string TimeZoneId { get; set; } = null!;
    }
}
