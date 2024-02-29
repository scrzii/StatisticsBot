using Microsoft.Extensions.DependencyInjection;
using StatisticsBot.Extensions;
using StatisticsBot.Jobs;
using StatisticsBot.MessageHandlers;
using StatisticsBot.Services.Data;
using StatisticsBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace StatisticsBot;

public class Program
{
    public static async Task Main(string[] args)
    {
        Config.Init();
        var provider = DI.ConfigureServices();
        await ApplyMigrations(provider);

        await JobService.StartJobs(provider);
        StartBot(provider);

        await Task.Delay(-1);
    }

    private static async Task Test()
    {
        var chart = new RadarChartGenerator(200);
        chart.RegisterFeatures("Honor", "Qwe", "Ewq");
        chart.AddObject(new SkiaSharp.SKColor(255, 0, 0), new { Honor = 200, Qwe = 1, Ewq = 2 });
        chart.AddObject(new SkiaSharp.SKColor(255, 230, 0), new { Honor = 150, Qwe = 5, Ewq = 3 });
        chart.AddObject(new SkiaSharp.SKColor(255, 0, 230), new { Honor = 130, Qwe = 4, Ewq = 2 });
        chart.SetMax("Ewq", 5);

        await chart.Render().SaveToFile("chart.png");
    }

    private static void StartBot(IServiceProvider provider)
    {
        var bot = provider.GetService<ITelegramBotClient>();
        var handler = provider.GetService<CommandHandler>();
        bot.StartReceiving(handler, new ReceiverOptions { ThrowPendingUpdates = true });

        Console.WriteLine("Bot started");
    }

    private static async Task ApplyMigrations(IServiceProvider provider)
    {
        var db = provider.GetService<DataContext>();
        await db.ApplyMigrations();
    }
}