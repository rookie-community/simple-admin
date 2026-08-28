using Admin.Quartzs.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Admin.Quartzs
{
    public interface IQuartzCronTriggerAppService : ICrudAppService<QrtzCronTriggerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateQrtzCronTriggerDto, CreateUpdateQrtzCronTriggerDto>
    {
        Task<QrtzCronTriggerDto> GetByTriggerIdAsync(Guid triggerId);
    }
}
