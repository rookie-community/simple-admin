using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Admin.Quartzs.Dtos
{
    public class CreateUpdateQrtzJobDto : EntityDto
    {
        [Required]
        [MaxLength(100)]
        public string JobName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string JobGroup { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string JobClassName { get; set; } = null!;

        public bool IsDisallowConcurrent { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string Description { get; set; } = null!;
    }
}
