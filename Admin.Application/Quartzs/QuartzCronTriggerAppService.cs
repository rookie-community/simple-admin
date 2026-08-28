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
    [Authorize(QuartzPermissions.CronTriggers.Default)]
    public class QuartzCronTriggerAppService :
            CrudAppService<QrtzCronTrigger, QrtzCronTriggerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateQrtzCronTriggerDto, CreateUpdateQrtzCronTriggerDto>,
            IQuartzCronTriggerAppService
    {
        private readonly IRepository<QrtzCronTrigger, Guid> _cronTriggerRepository;
        private readonly IRepository<QrtzTrigger, Guid> _triggerRepository;
        private readonly QuartzManagementService _quartzManagementService;

        public QuartzCronTriggerAppService(
            IRepository<QrtzCronTrigger, Guid> cronTriggerRepository,
            IRepository<QrtzTrigger, Guid> triggerRepository,
            QuartzManagementService quartzManagementService)
            : base(cronTriggerRepository)
        {
            _cronTriggerRepository = cronTriggerRepository;
            _triggerRepository = triggerRepository;
            _quartzManagementService = quartzManagementService;
        }

        public override async Task<QrtzCronTriggerDto> CreateAsync(CreateUpdateQrtzCronTriggerDto input)
        {
            var queryable = await _triggerRepository.WithDetailsAsync(x => x.Job);
            var trigger = queryable.FirstOrDefault(x => x.Id == input.TriggerId);

            if (trigger == null)
            {
                throw new Exception($"触发器 {input.TriggerId} 不存在");
            }

            if (trigger.TriggerType != TriggerType.Cron)
            {
                throw new Exception($"触发器类型不是 Cron，当前类型: {trigger.TriggerType}");
            }

            var exists = await _cronTriggerRepository
                .AnyAsync(x => x.TriggerId == input.TriggerId);

            if (exists)
            {
                throw new Exception($"触发器 {input.TriggerId} 已有 Cron 配置");
            }

            var entity = ObjectMapper.Map<CreateUpdateQrtzCronTriggerDto, QrtzCronTrigger>(input);
            await _cronTriggerRepository.InsertAsync(entity);

            if (trigger.IsEnabled && trigger.Job.IsEnabled)
            {
                await RescheduleTriggerAsync(trigger.Id);
            }

            return ObjectMapper.Map<QrtzCronTrigger, QrtzCronTriggerDto>(entity);
        }

        public override async Task<QrtzCronTriggerDto> UpdateAsync(Guid id, CreateUpdateQrtzCronTriggerDto input)
        {
            var entity = await _cronTriggerRepository.GetAsync(id);
            entity.CronExpression = input.CronExpression;
            entity.TimeZoneId = input.TimeZoneId;

            await _cronTriggerRepository.UpdateAsync(entity);

            await RescheduleTriggerAsync(entity.TriggerId);

            return ObjectMapper.Map<QrtzCronTrigger, QrtzCronTriggerDto>(entity);
        }

        public override async Task DeleteAsync(Guid id)
        {
            var cronQueryable = await _cronTriggerRepository.WithDetailsAsync(x => x.Trigger);
            var entity = cronQueryable.FirstOrDefault(x => x.Id == id);

            if (entity == null)
            {
                throw new Exception($"Cron 配置 {id} 不存在");
            }

            var triggerId = entity.TriggerId;

            var triggerQueryable = await _triggerRepository.WithDetailsAsync(x => x.Job);
            var trigger = triggerQueryable.FirstOrDefault(x => x.Id == triggerId);

            if (trigger != null)
            {
                await _quartzManagementService.UnscheduleJobAsync(trigger.TriggerName, trigger.TriggerGroup);
            }

            await _cronTriggerRepository.DeleteAsync(entity);
        }

        public async Task<QrtzCronTriggerDto> GetByTriggerIdAsync(Guid triggerId)
        {
            var entity = await _cronTriggerRepository
                .FirstOrDefaultAsync(x => x.TriggerId == triggerId);

            return ObjectMapper.Map<QrtzCronTrigger, QrtzCronTriggerDto>(entity);
        }

        private async Task RescheduleTriggerAsync(Guid triggerId)
        {
            var queryable = await _triggerRepository.WithDetailsAsync(x => x.Job);
            var trigger = queryable.FirstOrDefault(x => x.Id == triggerId);

            if (trigger == null || !trigger.IsEnabled || !trigger.Job.IsEnabled) return;

            var cron = await _cronTriggerRepository
                .FirstOrDefaultAsync(x => x.TriggerId == triggerId);

            if (cron == null) return;

            var scheduler = await _quartzManagementService.GetSchedulerAsync();

            await _quartzManagementService.UnscheduleJobAsync(trigger.TriggerName, trigger.TriggerGroup);

            var jobType = Type.GetType(trigger.Job.JobClassName);
            if (jobType == null) return;

            var jobBuilder = JobBuilder.Create(jobType)
                .WithIdentity(trigger.Job.JobName, trigger.Job.JobGroup)
                .StoreDurably();

            if (trigger.Job.IsDisallowConcurrent)
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
                .ForJob(trigger.Job.JobName, trigger.Job.JobGroup)
                .WithPriority(trigger.Priority)
                .WithCronSchedule(cron.CronExpression, x =>
                {
                    if (!string.IsNullOrEmpty(cron.TimeZoneId))
                    {
                        x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(cron.TimeZoneId));
                    }
                });

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
