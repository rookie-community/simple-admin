using Admin.Quartzs.Dtos;
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Admin.Quartzs
{
    public interface IQuartzJobAppService : ICrudAppService<QrtzJobDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateQrtzJobDto, CreateUpdateQrtzJobDto>
    {
    }
}
