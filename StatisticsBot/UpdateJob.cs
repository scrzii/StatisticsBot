using Quartz;
using SkiaSharp;
using StatisticsBot.Render;
using StatisticsBot.ResultParser;
using StatisticsBot.Sql;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StatisticsBot;

public class UpdateJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await MainService.UpdateUsers();
            await MainService.UpdateChart();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Task exception: {ex.Message}");
        }
    }
}
