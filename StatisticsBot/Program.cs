using Microsoft.Data.Sqlite;
using Quartz;
using Quartz.Impl;
using StatisticsBot.Render;
using StatisticsBot.ResultParser;
using StatisticsBot.Sql;
using StatisticsBot.Sql.Models;
using System.Linq;

namespace StatisticsBot;

public class Program
{
    public static async Task Main(string[] args)
    {
        Config.Init();

        if (args.Any(x => x.ToLower() == "--init"))
        {
            await DbRepository.Init();
            return;
        }

        var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        await scheduler.Start();

        var updateJob = JobBuilder.Create<UpdateJob>().Build();
        var trigger = TriggerBuilder.Create()
            .WithIdentity("UpdateJobTrigger")
            .StartNow()
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever())
            .Build();

        await scheduler.ScheduleJob(updateJob, trigger);

        await BotClient.Start();
    }
}