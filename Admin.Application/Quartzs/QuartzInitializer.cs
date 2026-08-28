using System;
using System.Threading.Tasks;
using Admin.Quartz;
using Admin.Utils;
using System.Linq;
using Quartz;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Admin.Quartzs
{
    public class QuartzInitializer : ITransientDependency
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IRepository<QrtzJob, Guid> _jobRepository;
        private readonly IRepository<QrtzTrigger, Guid> _triggerRepository;

        public QuartzInitializer(
            ISchedulerFactory schedulerFactory,
            IRepository<QrtzJob, Guid> jobRepository,
            IRepository<QrtzTrigger, Guid> triggerRepository)
        {
            _schedulerFactory = schedulerFactory;
            _jobRepository = jobRepository;
            _triggerRepository = triggerRepository;
        }

        public async Task InitializeAsync()
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            if (scheduler.IsStarted)
            {
                await scheduler.Clear();
            }

            var queryable = await _jobRepository.WithDetailsAsync(x => x.Triggers);
            var jobs = queryable.Where(x => x.IsEnabled).ToList();

            if (jobs.Count == 0)
            {
                await scheduler.Start();
                return;
            }

            foreach (var jobConfig in jobs)
            {
                var jobDetail = QuartzConverter.CreateJobDetail(jobConfig);
                await scheduler.AddJob(jobDetail, true);

                var enabledTriggers = jobConfig.Triggers?.Where(t => t.IsEnabled).ToList();
                if (enabledTriggers != null && enabledTriggers.Any())
                {
                    foreach (var triggerConfig in enabledTriggers)
                    {
                        var fullTrigger = await LoadTriggerWithDetailAsync(triggerConfig.Id);
                        if (fullTrigger == null) continue;

                        var trigger = QuartzConverter.CreateTrigger(fullTrigger);
                        await scheduler.ScheduleJob(trigger);
                    }
                }
            }

            if (!scheduler.IsStarted)
            {
                await scheduler.Start();
            }
        }

        private async Task<QrtzTrigger?> LoadTriggerWithDetailAsync(Guid triggerId)
        {
            var queryable = await _triggerRepository.WithDetailsAsync(x => x.Job);
            var trigger = queryable.Where(x => x.Id == triggerId).FirstOrDefault();

            if (trigger == null) return null;

            switch (trigger.TriggerType)
            {
                case TriggerType.Cron:
                    await _triggerRepository.EnsurePropertyLoadedAsync(trigger, x => x.CronTrigger);
                    break;
                case TriggerType.Simple:
                    await _triggerRepository.EnsurePropertyLoadedAsync(trigger, x => x.SimpleTrigger);
                    break;
                case TriggerType.CalendarInterval:
                case TriggerType.DailyTimeInterval:
                case TriggerType.Recurrence:
                    await _triggerRepository.EnsurePropertyLoadedAsync(trigger, x => x.SimPropTrigger);
                    break;
            }

            return trigger;
        }
    }
}
