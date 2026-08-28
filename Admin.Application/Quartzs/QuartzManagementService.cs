using System;
using System.Threading.Tasks;
using Admin.Quartz;
using Quartz;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Admin.Quartzs
{
    public class QuartzManagementService : ITransientDependency
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IRepository<QrtzJob, Guid> _jobRepository;

        public QuartzManagementService(
            ISchedulerFactory schedulerFactory,
            IRepository<QrtzJob, Guid> jobRepository)
        {
            _schedulerFactory = schedulerFactory;
            _jobRepository = jobRepository;
        }

        public async Task PauseJobAsync(string jobName, string jobGroup)
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            var job = await _jobRepository
                .FirstOrDefaultAsync(x => x.JobName == jobName && x.JobGroup == jobGroup);
            if (job != null)
            {
                job.IsEnabled = false;
                await _jobRepository.UpdateAsync(job);
            }

            await scheduler.PauseJob(new JobKey(jobName, jobGroup));
        }

        public async Task ResumeJobAsync(string jobName, string jobGroup)
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            var job = await _jobRepository
                .FirstOrDefaultAsync(x => x.JobName == jobName && x.JobGroup == jobGroup);
            if (job != null)
            {
                job.IsEnabled = true;
                await _jobRepository.UpdateAsync(job);
            }

            await scheduler.ResumeJob(new JobKey(jobName, jobGroup));
        }

        public async Task TriggerJobAsync(string jobName, string jobGroup)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.TriggerJob(new JobKey(jobName, jobGroup));
        }

        public async Task DeleteJobAsync(string jobName, string jobGroup)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.DeleteJob(new JobKey(jobName, jobGroup));
        }

        public async Task<bool> CheckJobExistsAsync(string jobName, string jobGroup)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            return await scheduler.CheckExists(new JobKey(jobName, jobGroup));
        }
    }
}
