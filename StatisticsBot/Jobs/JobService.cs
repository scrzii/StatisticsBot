using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;

namespace StatisticsBot.Jobs;

public static class JobService
{
    public static async Task StartJobs(IServiceProvider serviceProvider)
    {
        var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        await scheduler.Start();
        scheduler.JobFactory = new JobFactory(serviceProvider);

        var updateJob = JobBuilder.Create<UpdateJob>().Build();
        var trigger = TriggerBuilder.Create()
            .WithIdentity("UpdateJobTrigger")
            .StartNow()
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever())
            .Build();

        await scheduler.ScheduleJob(updateJob, trigger);
    }
}
