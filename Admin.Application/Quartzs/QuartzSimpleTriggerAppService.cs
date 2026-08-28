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
    [Authorize(QuartzPermissions.SimpleTriggers.Default)]
    public class QuartzSimpleTriggerAppService :
            CrudAppService<QrtzSimpleTrigger, QrtzSimpleTriggerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateQrtzSimpleTriggerDto, CreateUpdateQrtzSimpleTriggerDto>,
            IQuartzSimpleTriggerAppService
    {
        private readonly IRepository<QrtzSimpleTrigger, Guid> _simpleTriggerRepository;
        private readonly IRepository<QrtzTrigger, Guid> _triggerRepository;
        private readonly QuartzManagementService _quartzManagementService;

        public QuartzSimpleTriggerAppService(
            IRepository<QrtzSimpleTrigger, Guid> simpleTriggerRepository,
            IRepository<QrtzTrigger, Guid> triggerRepository,
            QuartzManagementService quartzManagementService)
            : base(simpleTriggerRepository)
        {
            _simpleTriggerRepository = simpleTriggerRepository;
            _triggerRepository = triggerRepository;
            _quartzManagementService = quartzManagementService;
        }

        public override async Task<QrtzSimpleTriggerDto> CreateAsync(CreateUpdateQrtzSimpleTriggerDto input)
        {
            var queryable = await _triggerRepository.WithDetailsAsync(x => x.Job);
            var trigger = queryable.FirstOrDefault(x => x.Id == input.TriggerId);

            if (trigger == null)
            {
                throw new Exception($"触发器 {input.TriggerId} 不存在");
            }

            if (trigger.TriggerType != TriggerType.Simple)
            {
                throw new Exception($"触发器类型不是 Simple，当前类型: {trigger.TriggerType}");
            }

            var exists = await _simpleTriggerRepository
                .AnyAsync(x => x.TriggerId == input.TriggerId);

            if (exists)
            {
                throw new Exception($"触发器 {input.TriggerId} 已有 Simple 配置");
            }

            var entity = ObjectMapper.Map<CreateUpdateQrtzSimpleTriggerDto, QrtzSimpleTrigger>(input);
            await _simpleTriggerRepository.InsertAsync(entity);

            if (trigger.IsEnabled && trigger.Job.IsEnabled)
            {
                await RescheduleTriggerAsync(trigger.Id);
            }

            return ObjectMapper.Map<QrtzSimpleTrigger, QrtzSimpleTriggerDto>(entity);
        }

        public override async Task<QrtzSimpleTriggerDto> UpdateAsync(Guid id, CreateUpdateQrtzSimpleTriggerDto input)
        {
            var entity = await _simpleTriggerRepository.GetAsync(id);
            entity.RepeatCount = input.RepeatCount;
            entity.IntervalSeconds = input.IntervalSeconds;

            await _simpleTriggerRepository.UpdateAsync(entity);

            await RescheduleTriggerAsync(entity.TriggerId);

            return ObjectMapper.Map<QrtzSimpleTrigger, QrtzSimpleTriggerDto>(entity);
        }

        public override async Task DeleteAsync(Guid id)
        {
            var simpleTriggerQueryable = await _simpleTriggerRepository.WithDetailsAsync(x => x.Trigger);
            var entity = simpleTriggerQueryable.FirstOrDefault(x => x.Id == id);

            if (entity == null)
            {
                throw new Exception($"Simple 配置 {id} 不存在");
            }

            var triggerId = entity.TriggerId;
            var triggerQueryable = await _triggerRepository.WithDetailsAsync(x => x.Job);
            var trigger = triggerQueryable.FirstOrDefault(x => x.Id == triggerId);

            if (trigger != null)
            {
                await _quartzManagementService.UnscheduleJobAsync(trigger.TriggerName, trigger.TriggerGroup);
            }

            await _simpleTriggerRepository.DeleteAsync(entity);
        }

        public async Task<QrtzSimpleTriggerDto> GetByTriggerIdAsync(Guid triggerId)
        {
            var entity = await _simpleTriggerRepository
                .FirstOrDefaultAsync(x => x.TriggerId == triggerId);

            return ObjectMapper.Map<QrtzSimpleTrigger, QrtzSimpleTriggerDto>(entity);
        }

        private async Task RescheduleTriggerAsync(Guid triggerId)
        {
            var queryable = await _triggerRepository.WithDetailsAsync(x => x.Job);
            var trigger = queryable.FirstOrDefault(x => x.Id == triggerId);

            if (trigger == null || !trigger.IsEnabled || !trigger.Job.IsEnabled) return;

            var simple = await _simpleTriggerRepository
                .FirstOrDefaultAsync(x => x.TriggerId == triggerId);

            if (simple == null) return;

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
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(simple.IntervalSeconds)
                    .WithRepeatCount(simple.RepeatCount));

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
