using Admin.Quartz;
using Quartz;
using System;
using System.Threading.Tasks;
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

        /// <summary>
        /// 暂停 Job
        /// </summary>
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

        /// <summary>
        /// 恢复 Job
        /// </summary>
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

        /// <summary>
        /// 立即触发 Job（手动执行一次）
        /// </summary>
        public async Task TriggerJobAsync(string jobName, string jobGroup)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.TriggerJob(new JobKey(jobName, jobGroup));
        }

        /// <summary>
        /// 删除 Job（从 Quartz 中移除）
        /// </summary>
        public async Task DeleteJobAsync(string jobName, string jobGroup)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.DeleteJob(new JobKey(jobName, jobGroup));
        }

        /// <summary>
        /// 取消调度触发器（从 Quartz 中移除单个 Trigger）
        /// </summary>
        public async Task UnscheduleJobAsync(string triggerName, string triggerGroup)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var triggerKey = new TriggerKey(triggerName, triggerGroup);

            if (await scheduler.CheckExists(triggerKey))
            {
                await scheduler.UnscheduleJob(triggerKey);
            }
        }

        /// <summary>
        /// 检查 Job 是否存在
        /// </summary>
        public async Task<bool> CheckJobExistsAsync(string jobName, string jobGroup)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            return await scheduler.CheckExists(new JobKey(jobName, jobGroup));
        }

        /// <summary>
        /// 检查 Trigger 是否存在
        /// </summary>
        public async Task<bool> CheckTriggerExistsAsync(string triggerName, string triggerGroup)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            return await scheduler.CheckExists(new TriggerKey(triggerName, triggerGroup));
        }

        /// <summary>
        /// 获取调度器实例
        /// </summary>
        public async Task<IScheduler> GetSchedulerAsync()
        {
            return await _schedulerFactory.GetScheduler();
        }
    }
}
