using Admin.Quartzs.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Admin.Quartzs
{
    public interface IQuartzTriggerAppService : ICrudAppService<QrtzTriggerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateQrtzTriggerDto, CreateUpdateQrtzTriggerDto>
    {
        Task<ListResultDto<QrtzTriggerDto>> GetListByJobIdAsync(Guid jobId);
    }
}
