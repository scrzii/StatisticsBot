using StatisticsBot.MessageHandlers;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace StatisticsBot;

public class BotClient
{
    public static ITelegramBotClient Bot { get; private set; }

    public static async Task Start()
    {
        Bot = new TelegramBotClient(Config.Instance.TelegramToken);
        var options = new ReceiverOptions()
        {
            
            ThrowPendingUpdates = true
        };

        using var cancellationTokenSource = new CancellationTokenSource();
        Bot.StartReceiving(UpdateHandler, ErrorHandler, options, cancellationTokenSource.Token);

        Console.WriteLine("Bot started");

        await Task.Delay(-1);
    }

    private static async Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    if (update.Message?.Text?.StartsWith("/") == true)
                    {
                        await CommandHandler.HandleCommand(bot, update);
                    }
                    return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception throwed: {ex.Message}");
        }
    }

    private static async Task ErrorHandler(ITelegramBotClient bot, Exception ex, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Exception from error hander: {ex.Message}");
    }
}
