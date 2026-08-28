using Admin.Quartzs.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Admin.Quartzs
{
    public interface IQuartzSimpleTriggerAppService : ICrudAppService<QrtzSimpleTriggerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateQrtzSimpleTriggerDto, CreateUpdateQrtzSimpleTriggerDto>
    {
        Task<QrtzSimpleTriggerDto> GetByTriggerIdAsync(Guid triggerId);
    }
}
