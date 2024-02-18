using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using StatisticsBot.Extensions;
using StatisticsBot.ResultParser;
using StatisticsBot.Services.Data;
using StatisticsBot.Services.Data.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

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
        var infos = await InfoParser.GetInfos(users.Select(x => x.Value.CodewarsLogin).ToArray());

        foreach (var info in infos)
        {
            var user = users[info.Name];
            user.Honor = info.Honor;
            user.TotalTasks = info.TasksCount;
            user.Rank = info.Rank;
            user.LastUpdate = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
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

    private static async Task<InputFile> CreateFileFromBytes(byte[] bytes, string filename)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        foreach (var chunk in bytes.Chunk(32))
        {
            await stream.WriteAsync(chunk.ToArray());
        }

        return InputFile.FromStream(stream, filename);
    }
}
