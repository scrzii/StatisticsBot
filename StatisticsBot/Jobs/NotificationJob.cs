using Microsoft.EntityFrameworkCore;
using Quartz;
using StatisticsBot.Services.Data;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace StatisticsBot.Jobs;
public class NotificationJob : IJob
{
    private readonly ITelegramBotClient _bot;
    private readonly DataContext _db;

    public NotificationJob(ITelegramBotClient bot, DataContext db)
    {
        _db = db;
        _bot = bot;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var notifications = await _db.Notifications.ToListAsync();

        if (!notifications.Any())
        {
            return;
        }

        foreach (var notification in notifications)
        {
            await _bot.SendTextMessageAsync(notification.ChatId, notification.Message,
                parseMode: notification.WithMarkup ? ParseMode.Markdown : ParseMode.Html);
            await Task.Delay(1000);
        }

        _db.Notifications.RemoveRange(notifications);
        await _db.SaveChangesAsync();
    }
}
