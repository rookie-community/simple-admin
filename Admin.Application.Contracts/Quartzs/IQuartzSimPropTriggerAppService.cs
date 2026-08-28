using Admin.Quartzs.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Admin.Quartzs
{
    public interface IQuartzSimPropTriggerAppService : ICrudAppService<QrtzSimPropTriggerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateQrtzSimPropTriggerDto, CreateUpdateQrtzSimPropTriggerDto>
    {
        Task<QrtzSimPropTriggerDto> GetByTriggerIdAsync(Guid triggerId);
    }
}
