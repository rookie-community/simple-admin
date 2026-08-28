using Admin.Permissions;
using Admin.Quartz;
using Admin.Quartzs.Dtos;
using Microsoft.AspNetCore.Authorization;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Admin.Quartzs
{
    [Authorize(QuartzPermissions.Triggers.Default)]
    public class QuartzTriggerAppService :
            CrudAppService<QrtzTrigger, QrtzTriggerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateQrtzTriggerDto, CreateUpdateQrtzTriggerDto>, 
            IQuartzTriggerAppService
    {
        private readonly IRepository<QrtzTrigger, Guid> _triggerRepository;
        private readonly IRepository<QrtzJob, Guid> _jobRepository;
        private readonly QuartzManagementService _quartzManagementService;

        public QuartzTriggerAppService(IRepository<QrtzTrigger,
            Guid> triggerRepository, 
            IRepository<QrtzJob, Guid> jobRepository,
            QuartzManagementService quartzManagementService) 
            : base(triggerRepository)
        {
            _triggerRepository = triggerRepository;
            _jobRepository = jobRepository;
            _quartzManagementService = quartzManagementService;
        }

        public override async Task<QrtzTriggerDto> CreateAsync(CreateUpdateQrtzTriggerDto input)
        {
            var exists = await _triggerRepository.AnyAsync(x => x.TriggerName == input.TriggerName && x.TriggerGroup == input.TriggerGroup);

            if (exists)
            {
                throw new Exception($"触发器 {input.TriggerName} (组: {input.TriggerGroup}) 已存在");
            }

            var job = await _jobRepository.FirstOrDefaultAsync(x => x.Id == input.JobId);
            if (job == null)
            {
                throw new Exception($"Job {input.JobId} 不存在");
            }

            var trigger = ObjectMapper.Map<CreateUpdateQrtzTriggerDto, QrtzTrigger>(input);
            trigger.Job = job;

            await _triggerRepository.InsertAsync(trigger);

            if (job.IsEnabled && trigger.IsEnabled)
            {
                await RegisterTriggerToQuartzAsync(trigger);
            }

            return ObjectMapper.Map<QrtzTrigger, QrtzTriggerDto>(trigger);
        }

        public override async Task<QrtzTriggerDto> UpdateAsync(Guid id, CreateUpdateQrtzTriggerDto input)
        {
            var queryable = await _triggerRepository.WithDetailsAsync(x => x.Job);
            var trigger = queryable.FirstOrDefault(x => x.Id == id);

            if (trigger == null)
            {
                throw new Exception($"触发器 {id} 不存在");
            }

            await _quartzManagementService.UnscheduleJobAsync(trigger.TriggerName, trigger.TriggerGroup);

            trigger.TriggerName = input.TriggerName;
            trigger.TriggerGroup = input.TriggerGroup;
            trigger.JobId = input.JobId;
            trigger.TriggerType = input.TriggerType;
            trigger.Priority = input.Priority;
            trigger.IsEnabled = input.IsEnabled;
            trigger.Description = input.Description;

            await _triggerRepository.UpdateAsync(trigger);

            var job = await _jobRepository.FirstOrDefaultAsync(x => x.Id == trigger.JobId);
            if (job != null && job.IsEnabled && trigger.IsEnabled)
            {
                await RegisterTriggerToQuartzAsync(trigger);
            }

            return ObjectMapper.Map<QrtzTrigger, QrtzTriggerDto>(trigger);
        }

        public override async Task DeleteAsync(Guid id)
        {
            var queryable = await _triggerRepository.WithDetailsAsync(x => x.Job);
            var trigger = queryable.FirstOrDefault(x => x.Id == id);

            if (trigger == null)
            {
                throw new Exception($"触发器 {id} 不存在");
            }

            await _quartzManagementService.UnscheduleJobAsync(trigger.TriggerName, trigger.TriggerGroup);
            await _triggerRepository.DeleteAsync(trigger);
        }

        public async Task<ListResultDto<QrtzTriggerDto>> GetListByJobIdAsync(Guid jobId)
        {
            var queryable = await _triggerRepository.WithDetailsAsync(x => x.Job);
            var triggers = queryable.Where(x => x.JobId == jobId).ToList();

            var dtos = ObjectMapper.Map<List<QrtzTrigger>, List<QrtzTriggerDto>>(triggers);
            return new ListResultDto<QrtzTriggerDto>(dtos);
        }

        private async Task RegisterTriggerToQuartzAsync(QrtzTrigger trigger)
        {
            var scheduler = await _quartzManagementService.GetSchedulerAsync();

            var queryable = await _jobRepository.WithDetailsAsync(x => x.Triggers);

            var job = queryable.FirstOrDefault(x => x.Id == trigger.JobId);

            if (job == null) return;

            var jobType = Type.GetType(job.JobClassName);
            if (jobType == null) return;

            var jobBuilder = JobBuilder.Create(jobType)
                .WithIdentity(job.JobName, job.JobGroup)
                .StoreDurably();

            if (job.IsDisallowConcurrent)
            {
                jobBuilder.DisallowConcurrentExecution(true);
            }

            var jobDetail = jobBuilder.Build();

            if (!await scheduler.CheckExists(jobDetail.Key))
            {
                await scheduler.AddJob(jobDetail, true);
            }

            var triggerBuilder = TriggerBuilder.Create()
                .WithIdentity(trigger.TriggerName, trigger.TriggerGroup)
                .ForJob(job.JobName, job.JobGroup)
                .WithPriority(trigger.Priority);

            var quartzTrigger = triggerBuilder.Build();

            if (await scheduler.CheckExists(quartzTrigger.Key))
            {
                await scheduler.RescheduleJob(quartzTrigger.Key, quartzTrigger);
            }
            else
            {
                await scheduler.ScheduleJob(quartzTrigger);
            }
        }
    }
}
