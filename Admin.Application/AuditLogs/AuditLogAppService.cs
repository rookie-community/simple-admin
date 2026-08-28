using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.AuditLogging;

namespace Admin.AuditLogs
{
    [Authorize]
    public class AuditLogAppService : ApplicationService, IAuditLogAppService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogAppService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<AuditLogDto> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var auditLog = await _auditLogRepository.GetAsync(id, cancellationToken: cancellationToken);
            return ObjectMapper.Map<AuditLog, AuditLogDto>(auditLog);
        }

        public async Task<PagedResultDto<AuditLogDto>> GetListAsync(GetAuditLogListInput input, CancellationToken cancellationToken = default)
        {
            if (input.Sorting.IsNullOrWhiteSpace())
            {
                input.Sorting = nameof(AuditLog.ExecutionTime) + " " + "DESC";
            }

            var auditLogs = await _auditLogRepository.GetListAsync(input.Sorting,
                                                                   input.MaxResultCount,
                                                                   input.SkipCount,
                                                                   startTime: input.StartTime,
                                                                   endTime: input.EndTime,
                                                                   userName: input.UserName,
                                                                   url: input.Url,
                                                                   minExecutionDuration: input.MinDuration,
                                                                   maxExecutionDuration: input.MaxDuration,
                                                                   httpMethod: input.HttpMethod,
                                                                   httpStatusCode: input.HttpStatusCode,
                                                                   applicationName: input.ApplicationName,
                                                                   clientIpAddress: input.ClientIpAddress,
                                                                   cancellationToken: cancellationToken);

            var totalCount = await _auditLogRepository.GetCountAsync(startTime: input.StartTime,
                                                                   endTime: input.EndTime,
                                                                   userName: input.UserName,
                                                                   url: input.Url,
                                                                   minExecutionDuration: input.MinDuration,
                                                                   maxExecutionDuration: input.MaxDuration,
                                                                   httpMethod: input.HttpMethod,
                                                                   httpStatusCode: input.HttpStatusCode,
                                                                   applicationName: input.ApplicationName,
                                                                   clientIpAddress: input.ClientIpAddress,
                                                                   cancellationToken: cancellationToken);
            return new PagedResultDto<AuditLogDto>(totalCount, ObjectMapper.Map<List<AuditLog>, List<AuditLogDto>>(auditLogs));
        }

        private Expression<Func<AuditLog, bool>> GetListFilter(GetAuditLogListInput model)
        {
            var filter = PredicateBuilder.New<AuditLog>(true);
            if (!string.IsNullOrWhiteSpace(model.Url))
            {
                filter = filter.And(x => x.HttpMethod.Equals(model.Url));
            }
            return filter;
        }
    }
}
