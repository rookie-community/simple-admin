using Admin.Permissions;
using Admin.Quartz;
using Admin.Quartzs.Dtos;
using Microsoft.AspNetCore.Authorization;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Admin.Quartzs
{
    [Authorize(QuartzPermissions.Jobs.Default)]
    public class QuartzJobAppService :
            CrudAppService<QrtzJob, QrtzJobDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateQrtzJobDto, CreateUpdateQrtzJobDto>,
            IQuartzJobAppService
    {
        private readonly IRepository<QrtzJob, Guid> _jobRepository;
        private readonly QuartzManagementService _quartzManagementService;

        public QuartzJobAppService(
            IRepository<QrtzJob, Guid> jobRepository,
            QuartzManagementService quartzManagementService)
            : base(jobRepository)
        {
            _jobRepository = jobRepository;
            _quartzManagementService = quartzManagementService;
        }

        public override async Task<QrtzJobDto> CreateAsync(CreateUpdateQrtzJobDto input)
        {
            var exists = await _jobRepository
                .AnyAsync(x => x.JobName == input.JobName && x.JobGroup == input.JobGroup);

            if (exists)
            {
                throw new Exception($"任务 {input.JobName} (组: {input.JobGroup}) 已存在");
            }

            var job = ObjectMapper.Map<CreateUpdateQrtzJobDto, QrtzJob>(input);
            await _jobRepository.InsertAsync(job);

            if (job.IsEnabled)
            {
                await RegisterJobToQuartzAsync(job);
            }

            return ObjectMapper.Map<QrtzJob, QrtzJobDto>(job);
        }

        public override async Task<QrtzJobDto> UpdateAsync(Guid id, CreateUpdateQrtzJobDto input)
        {
            var job = await _jobRepository.GetAsync(id);
            await _quartzManagementService.DeleteJobAsync(job.JobName, job.JobGroup);

            job.JobName = input.JobName;
            job.JobGroup = input.JobGroup;
            job.JobClassName = input.JobClassName;
            job.IsDisallowConcurrent = input.IsDisallowConcurrent;
            job.IsEnabled = input.IsEnabled;
            job.Description = input.Description;

            await _jobRepository.UpdateAsync(job);

            if (job.IsEnabled)
            {
                await RegisterJobToQuartzAsync(job);
            }

            return ObjectMapper.Map<QrtzJob, QrtzJobDto>(job);
        }

        public override async Task DeleteAsync(Guid id)
        {
            var queryable = await _jobRepository.WithDetailsAsync(x => x.Triggers);
            var job = queryable.FirstOrDefault(x => x.Id == id);

            if (job == null)
            {
                throw new Exception($"Job {id} 不存在");
            }

            await _quartzManagementService.DeleteJobAsync(job.JobName, job.JobGroup);
            await _jobRepository.DeleteAsync(job);
        }

        private async Task RegisterJobToQuartzAsync(QrtzJob job)
        {
            var scheduler = await _quartzManagementService.GetSchedulerAsync();

            var jobType = Type.GetType(job.JobClassName);
            if (jobType == null)
            {
                throw new Exception($"无法加载 Job 类型: {job.JobClassName}");
            }

            var builder = JobBuilder.Create(jobType)
                .WithIdentity(job.JobName, job.JobGroup)
                .StoreDurably();

            if (job.IsDisallowConcurrent)
            {
                builder.DisallowConcurrentExecution(true);
            }

            var jobDetail = builder.Build();

            if (await scheduler.CheckExists(jobDetail.Key))
            {
                await scheduler.DeleteJob(jobDetail.Key);
            }

            await scheduler.AddJob(jobDetail, true);
        }
    }
}