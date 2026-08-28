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
    [Authorize(QuartzPermissions.SimPropTriggers.Default)]
    public class QuartzSimPropTriggerAppService :
             CrudAppService<QrtzSimPropTrigger, QrtzSimPropTriggerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateQrtzSimPropTriggerDto, CreateUpdateQrtzSimPropTriggerDto>,
             IQuartzSimPropTriggerAppService
    {
        private readonly IRepository<QrtzSimPropTrigger, Guid> _simPropTriggerRepository;
        private readonly IRepository<QrtzTrigger, Guid> _triggerRepository;
        private readonly QuartzManagementService _quartzManagementService;

        public QuartzSimPropTriggerAppService(
            IRepository<QrtzSimPropTrigger, Guid> simPropTriggerRepository,
            IRepository<QrtzTrigger, Guid> triggerRepository,
            QuartzManagementService quartzManagementService)
            : base(simPropTriggerRepository)
        {
            _simPropTriggerRepository = simPropTriggerRepository;
            _triggerRepository = triggerRepository;
            _quartzManagementService = quartzManagementService;
        }

        public override async Task<QrtzSimPropTriggerDto> CreateAsync(CreateUpdateQrtzSimPropTriggerDto input)
        {
            var queryable = await _triggerRepository.WithDetailsAsync(x => x.Job);
            var trigger = queryable.FirstOrDefault(x => x.Id == input.TriggerId);

            if (trigger == null)
            {
                throw new Exception($"触发器 {input.TriggerId} 不存在");
            }

            if (trigger.TriggerType != TriggerType.CalendarInterval && trigger.TriggerType != TriggerType.DailyTimeInterval)
            {
                throw new Exception($"触发器类型不是 CalendarInterval 或 DailyTimeInterval，当前类型: {trigger.TriggerType}");
            }

            var exists = await _simPropTriggerRepository
                .AnyAsync(x => x.TriggerId == input.TriggerId);

            if (exists)
            {
                throw new Exception($"触发器 {input.TriggerId} 已有 SimProp 配置");
            }

            var entity = ObjectMapper.Map<CreateUpdateQrtzSimPropTriggerDto, QrtzSimPropTrigger>(input);
            await _simPropTriggerRepository.InsertAsync(entity);

            if (trigger.IsEnabled && trigger.Job.IsEnabled)
            {
                await RescheduleTriggerAsync(trigger.Id);
            }

            return ObjectMapper.Map<QrtzSimPropTrigger, QrtzSimPropTriggerDto>(entity);
        }

        public override async Task<QrtzSimPropTriggerDto> UpdateAsync(Guid id, CreateUpdateQrtzSimPropTriggerDto input)
        {
            var entity = await _simPropTriggerRepository.GetAsync(id);

            entity.StrProp1 = input.StrProp1;
            entity.StrProp2 = input.StrProp2;
            entity.StrProp3 = input.StrProp3;
            entity.IntProp1 = input.IntProp1;
            entity.IntProp2 = input.IntProp2;
            entity.LongProp1 = input.LongProp1;
            entity.LongProp2 = input.LongProp2;
            entity.DecProp1 = input.DecProp1;
            entity.DecProp2 = input.DecProp2;
            entity.BoolProp1 = input.BoolProp1;
            entity.BoolProp2 = input.BoolProp2;

            await _simPropTriggerRepository.UpdateAsync(entity);

            await RescheduleTriggerAsync(entity.TriggerId);

            return ObjectMapper.Map<QrtzSimPropTrigger, QrtzSimPropTriggerDto>(entity);
        }

        public override async Task DeleteAsync(Guid id)
        {
            var simPropTriggerQueryable = await _simPropTriggerRepository.WithDetailsAsync(x => x.Trigger);
            var entity = simPropTriggerQueryable.FirstOrDefault(x => x.Id == id);

            if (entity == null)
            {
                throw new Exception($"SimProp 配置 {id} 不存在");
            }

            var triggerId = entity.TriggerId;

            var triggerQueryable = await _triggerRepository.WithDetailsAsync(x => x.Job);
            var trigger = triggerQueryable.FirstOrDefault(x => x.Id == triggerId);

            if (trigger != null)
            {
                await _quartzManagementService.UnscheduleJobAsync(trigger.TriggerName, trigger.TriggerGroup);
            }

            await _simPropTriggerRepository.DeleteAsync(entity);
        }

        public async Task<QrtzSimPropTriggerDto> GetByTriggerIdAsync(Guid triggerId)
        {
            var entity = await _simPropTriggerRepository
                .FirstOrDefaultAsync(x => x.TriggerId == triggerId);

            return ObjectMapper.Map<QrtzSimPropTrigger, QrtzSimPropTriggerDto>(entity);
        }

        private async Task RescheduleTriggerAsync(Guid triggerId)
        {
            var queryable = await _triggerRepository.WithDetailsAsync(x => x.Job);

            var trigger = queryable.FirstOrDefault(x => x.Id == triggerId);

            if (trigger == null || !trigger.IsEnabled || !trigger.Job.IsEnabled) return;

            var simProp = await _simPropTriggerRepository
                .FirstOrDefaultAsync(x => x.TriggerId == triggerId);

            if (simProp == null) return;

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
                .WithPriority(trigger.Priority);

            switch (trigger.TriggerType)
            {
                case TriggerType.CalendarInterval:
                    var unit = Enum.Parse<IntervalUnit>(simProp.StrProp1);
                    var interval = simProp.IntProp1 ?? 1;
                    triggerBuilder.WithCalendarIntervalSchedule(x => x
                        .WithInterval(interval, unit));
                    break;

                case TriggerType.DailyTimeInterval:
                    var daysOfWeek = System.Text.Json.JsonSerializer
                        .Deserialize<List<int>>(simProp.StrProp3)
                        ?.Select(d => (DayOfWeek)d)
                        .ToArray() ?? Array.Empty<DayOfWeek>();

                    var startTime = TimeSpan.Parse(simProp.StrProp1);
                    var endTime = TimeSpan.Parse(simProp.StrProp2);

                    triggerBuilder.WithDailyTimeIntervalSchedule(x => x
                        .WithIntervalInSeconds(simProp.IntProp1 ?? 60)
                        .StartingDailyAt(TimeOfDay.HourMinuteAndSecondOfDay(
                            startTime.Hours, startTime.Minutes, startTime.Seconds))
                        .EndingDailyAt(TimeOfDay.HourMinuteAndSecondOfDay(
                            endTime.Hours, endTime.Minutes, endTime.Seconds))
                        .OnDaysOfTheWeek(daysOfWeek));
                    break;
            }

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
