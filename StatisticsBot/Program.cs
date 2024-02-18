using Microsoft.Extensions.DependencyInjection;
using StatisticsBot.Jobs;
using StatisticsBot.MessageHandlers;
using StatisticsBot.Services.Data;
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