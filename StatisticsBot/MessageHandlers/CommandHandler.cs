using SkiaSharp;
using StatisticsBot.Sql;
using StatisticsBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StatisticsBot.MessageHandlers;
public class CommandHandler
{
    private const string SetCWCommand = "setcw";
    private const string RemoveCWCommand = "removecw";
    private const string ChangeColorCommand = "changecolor";

    public static async Task HandleCommand(ITelegramBotClient bot, Update update)
    {
        var args = update.Message.Text.Split();
        var command = args[0].Replace("/", "").ToLower().Split("@").FirstOrDefault();
        args = args[1..];
        var isFromAdmin = await IsFromAdmin(bot, update);

        try
        {
            switch (command)
            {
                case SetCWCommand:
                    await SetCodewarsLogin(bot, update.Message.Chat.Id, update.Message.From.Id, args);
                    break;
                case RemoveCWCommand:
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

    private static async Task<bool> IsFromAdmin(ITelegramBotClient bot, Update update)
    {
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

    private static async Task SetCodewarsLogin(ITelegramBotClient bot, long chatId, long tgUserId, string[] args)
    {
        var user = await DbRepository.ReadUser(tgUserId);

        if (user == null)
        {
            user = new Sql.Models.User(tgUserId, chatId, args[0]);
            await DbRepository.CreateUser(user);
            await bot.SendTextMessageAsync(chatId, $"Логин {args[0]} успешно добавлен");
            return;
        }

        user.CodewarsLogin = args[0];
        await DbRepository.UpdateUser(user);
        await MainService.UpdateAll();
    }

    private static async Task RemoveCodewarsLogin(ITelegramBotClient bot, long chatId, bool isFromAdmin, string[] args)
    {
        if (!isFromAdmin)
        {
            await bot.SendTextMessageAsync(chatId, "Данная команда доступная только для администратора");
            return;
        }

        var user = (await DbRepository.ReadAllUsers()).FirstOrDefault(x => x.CodewarsLogin.ToLower() == args[0].ToLower());
        await DbRepository.RemoveUser(user.ChatId);
        await bot.SendTextMessageAsync(chatId, "Логин успешнно удален");
        await MainService.UpdateAll();
    }

    private static async Task ChangeUserColor(ITelegramBotClient bot, long chatId, bool isFromAdmin, long telegramId, string[] args)
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
                    to = (await DbRepository.ReadUser(args[0])).TelegramId;
                    color = ColorUtils.GenerateRandom();
                }
                break;
            case 2:
                color = SKColor.Parse(args[1]);
                to = (await DbRepository.ReadUser(args[0])).TelegramId;
                break;
            default:
                await bot.SendTextMessageAsync(chatId, "Неверное количество аргументов (их должно быть от 0 до 2)");
                return;
        }

        await ChangeUserColor(bot, chatId, isFromAdmin, telegramId, to, color);
        await MainService.UpdateAll();
    }

    private static async Task ChangeUserColor(ITelegramBotClient bot, long chatId, bool isFromAdmin, 
        long from, long to, SKColor newColor)
    {
        if (from != to && !isFromAdmin)
        {
            await bot.SendTextMessageAsync(chatId, "Изменять цвет другого пользователя может только администратор");
            return;
        }

        var user = await DbRepository.ReadUser(to);
        user.Color = newColor.ToString();
        await DbRepository.UpdateUser(user);
        await MainService.UpdateChart();
        await bot.SendTextMessageAsync(chatId, "Цвет успешно изменен");
    }
}
