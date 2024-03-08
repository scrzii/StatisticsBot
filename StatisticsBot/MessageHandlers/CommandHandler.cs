using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using StatisticsBot.Services;
using StatisticsBot.Services.Data;
using StatisticsBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = StatisticsBot.Services.Data.Models.User;

namespace StatisticsBot.MessageHandlers;

public class CommandHandler : IUpdateHandler
{
    private readonly DataContext _db;
    private readonly UpdateService _updateService;

    public CommandHandler(DataContext db, UpdateService updateService)
    {
        _db = db;
        _updateService = updateService;
    }

    private const string SetCodewarsLoginCommand = "setcw";
    private const string RemoveCodewarsLoginCommand = "removecw";
    private const string ChangeColorCommand = "changecolor";

    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    if (update.Message?.Text?.StartsWith("/") == true)
                    {
                        await HandleCommand(bot, update);
                    }
                    return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception throwed: {ex.Message}");
        }
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Update exception: {exception.Message}");
    }

    private async Task HandleCommand(ITelegramBotClient bot, Update update)
    {
        var args = update.Message.Text.Split();
        var command = args[0].Replace("/", "").ToLower().Split("@").FirstOrDefault();
        args = args[1..];
        var isFromAdmin = await IsFromAdmin(bot, update);

        try
        {
            switch (command)
            {
                case SetCodewarsLoginCommand:
                    await SetCodewarsLogin(bot, update.Message.Chat.Id, update.Message.From.Id, args);
                    break;
                case RemoveCodewarsLoginCommand:
                    await RemoveCodewarsLogin(bot, update.Message.Chat.Id, isFromAdmin, args);
                    break;
                case ChangeColorCommand:
                    await ChangeUserColor(bot, update.Message.Chat.Id, isFromAdmin, update.Message.From.Id, args);
                    break;
                default:
                    await bot.SendTextMessageAsync(update.Message.Chat.Id, "Неизвестная команда");
                    return;
            }
        }
        catch (Exception ex)
        {
            await bot.SendTextMessageAsync(update.Message.Chat.Id, $"Неправильный ввод команды\n{ex.Message}");
        }
    }

    private async Task<bool> IsFromAdmin(ITelegramBotClient bot, Update update)
    {
        if (update.Message.Chat.Type == ChatType.Private)
        {
            return true;
        }

        var admins = await bot.GetChatAdministratorsAsync(update.Message.Chat.Id);
        foreach (var admin in admins)
        {
            if (admin.User.Id == update.Message.From.Id)
            {
                return true;
            }
        }

        return false;
    }

    private async Task SetCodewarsLogin(ITelegramBotClient bot, long chatId, long tgUserId, string[] args)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => tgUserId == x.TelegramId);

        if (user == null)
        {
            user = new User(tgUserId, chatId, args[0]);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            await bot.SendTextMessageAsync(chatId, $"Логин {args[0]} успешно добавлен");

            await _updateService.UpdateAll();
            return;
        }

        var needUpdate = user.CodewarsLogin != args[0];

        user.CodewarsLogin = args[0];
        await _db.SaveChangesAsync();

        if (needUpdate)
        {
            await _updateService.UpdateAll();
        }
    }

    private async Task RemoveCodewarsLogin(ITelegramBotClient bot, long chatId, bool isFromAdmin, string[] args)
    {
        if (!isFromAdmin)
        {
            await bot.SendTextMessageAsync(chatId, "Данная команда доступная только для администратора");
            return;
        }

        var user = await _db.Users.FirstOrDefaultAsync(x => x.CodewarsLogin.ToLower() == args[0].ToLower());
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        await bot.SendTextMessageAsync(chatId, "Логин успешнно удален");
    }

    private async Task ChangeUserColor(ITelegramBotClient bot, long chatId, bool isFromAdmin, long telegramId, string[] args)
    {
        long to = telegramId;
        SKColor color = SKColor.Empty;

        switch (args.Length)
        {
            case 0:
                color = ColorUtils.GenerateRandom();
                break;
            case 1:
                if (args[0].StartsWith("#"))
                {
                    color = SKColor.Parse(args[0]);
                }
                else
                {
                    to = (await _db.Users.FirstOrDefaultAsync(x => x.CodewarsLogin == args[0])).TelegramId;
                    color = ColorUtils.GenerateRandom();
                }
                break;
            case 2:
                color = SKColor.Parse(args[1]);
                to = (await _db.Users.FirstOrDefaultAsync(x => x.CodewarsLogin == args[0])).TelegramId;
                break;
            default:
                await bot.SendTextMessageAsync(chatId, "Неверное количество аргументов (их должно быть от 0 до 2)");
                return;
        }

        await ChangeUserColor(bot, chatId, isFromAdmin, telegramId, to, color);
    }

    private async Task ChangeUserColor(ITelegramBotClient bot, long chatId, bool isFromAdmin, 
        long from, long to, SKColor newColor)
    {
        if (from != to && !isFromAdmin)
        {
            await bot.SendTextMessageAsync(chatId, "Изменять цвет другого пользователя может только администратор");
            return;
        }

        var user = await _db.Users.FirstOrDefaultAsync(x => x.TelegramId == to);
        user.Color = newColor;
        await _db.SaveChangesAsync();

        await _updateService.UpdateChart();
        await bot.SendTextMessageAsync(chatId, "Цвет успешно изменен");
    }
}
