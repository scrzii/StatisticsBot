using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace StatisticsBot.Jobs;

public class JobFactory : IJobFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<IJob, IServiceScope> _jobScopeMapping = new();

    public JobFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        var scope = _serviceProvider.CreateScope();
        var job = scope.ServiceProvider.GetService(bundle.JobDetail.JobType) as IJob;
        _jobScopeMapping.Add(job, scope);

        return job;
    }

    public void ReturnJob(IJob job)
    {
        if (!_jobScopeMapping.TryGetValue(job, out var scope))
        {
            Console.WriteLine("Mapping for job does not found!");
            return;
        }

        _jobScopeMapping.Remove(job);
        scope.Dispose();
    }
}
