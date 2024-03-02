using Microsoft.EntityFrameworkCore;
using StatisticsBot.Extensions;
using StatisticsBot.Services.Data;
using StatisticsBot.Services.Data.Models;
using StatisticsBot.Services.Parsing;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = StatisticsBot.Services.Data.Models.User;

namespace StatisticsBot.Services;
public class UpdateService
{
    private readonly RenderService _renderService;
    private readonly ITelegramBotClient _bot;
    private readonly DataContext _db;

    public UpdateService(RenderService renderService, ITelegramBotClient bot, DataContext db)
    {
        _renderService = renderService;
        _bot = bot;
        _db = db;
    }

    public async Task UpdateAll()
    {
        await UpdateUsers();
        await UpdateChart();
    }

    public async Task UpdateUsers()
    {
        var users = await _db.Users.ToDictionaryAsync(x => x.CodewarsLogin);
        var infos = (await GetCWData(users.Keys.ToList())).Where(x => x.IsCorrect);

        foreach (var info in infos)
        {
            var user = users[info.Name];

            await CheckUpdate(user, info);

            user.Honor = info.Honor;
            user.TotalTasks = info.TotalTasks;
            user.Rank = info.Rank;
            user.LastUpdate = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
    }

    private async Task<List<UserDto>> GetCWData(List<string> logins)
    {
        var result = new List<UserDto>();
        foreach (var login in logins)
        {
            var parser = new CodewarsUserParser(login);
            result.Add(await parser.Parse());
        }

        return result;
    }

    private async Task CheckUpdate(User user, UserDto dto)
    {
        if (dto.TotalTasks <= user.TotalTasks || dto.Honor <= user.Honor)
        {
            return;
        }

        if (user.Honor == null)
        {
            return;
        }

        var diff = dto.Honor - user.Honor;
        var isRankUp = dto.Rank > user.Rank;

        var messageText = $"[{user.CodewarsLogin}](tg://user?id={user.TelegramId}) решил задачу (+{diff} рейтинга).";
        if (isRankUp)
        {
            messageText += $"\nНовый ранг: {GetRankName((int) dto.Rank)}";
        }

        await _db.Notifications.AddAsync(new(messageText, user.ChatId));
    }

    private string GetRankName(int rank)
    {
        if (rank < 8)
        {
            return $"{8 - rank} кю";
        }

        return $"{rank - 7} дан";
    }

    public async Task UpdateChart()
    {
        var groupedUsers = (await _db.Users.ToListAsync())
            .GroupBy(x => x.ChatId);

        foreach (var group in groupedUsers)
        {
            var chat = group.Key;
            var users = group.ToList();
            var bytes = await _renderService.Generate(users);
            using var stream = new MemoryStream();

            await stream.WriteAllBytes(bytes);

            var filename = "chart.png";
            var inputFile = InputFile.FromStream(stream, filename);

            var dbMessage = await _db.ChartMessages.FirstOrDefaultAsync(x => x.ChatId == chat);
            if (dbMessage == null)
            {
                var message = await _bot.SendPhotoAsync(chat, inputFile);
                dbMessage = new ChartMessage(chat, message.MessageId);
                _db.ChartMessages.Add(dbMessage);
                await _db.SaveChangesAsync();
                return;
            }

            try
            {
                var media = new InputMediaPhoto(inputFile);
                var messageSuccess = await _bot.EditMessageMediaAsync(dbMessage.ChatId, (int)dbMessage.MessageId, media);
                dbMessage.MessageId = messageSuccess.MessageId;
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _db.ChartMessages.Remove(dbMessage);
                await _db.SaveChangesAsync();
                Console.WriteLine($"Update chart exception: {ex.Message}");
            }
            finally
            {
                stream.Dispose();
            }
        }
    }
}
