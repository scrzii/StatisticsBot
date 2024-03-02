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

        await ScheduleNow<UpdateJob>(scheduler, TimeSpan.FromSeconds(60));
        await ScheduleNow<NotificationJob>(scheduler, TimeSpan.FromSeconds(5));
    }

    private static async Task ScheduleNow<T>(IScheduler scheduler, TimeSpan interval) where T : IJob
    {
        var job = JobBuilder.Create<T>().Build();
        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{typeof(T).Name}Trigger")
            .StartNow()
            .WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever())
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }
}
