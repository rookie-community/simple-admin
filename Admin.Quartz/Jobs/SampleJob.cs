using Microsoft.Extensions.Logging;
using Quartz;
using Volo.Abp.DependencyInjection;

namespace Admin.Jobs
{
    public class SampleJob : IJob, ITransientDependency
    {
        private readonly ILogger<SampleJob> _logger;

        public SampleJob(ILogger<SampleJob> logger)
        {
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobKey = context.JobDetail.Key;
            var triggerKey = context.Trigger.Key;

            _logger.LogInformation($"[SampleJob] 执行时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _logger.LogInformation($"[SampleJob] Job: {jobKey}, Trigger: {triggerKey}");

            await Task.CompletedTask;
        }
    }
}
