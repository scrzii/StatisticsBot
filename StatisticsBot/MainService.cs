using StatisticsBot.Render;
using StatisticsBot.ResultParser;
using StatisticsBot.Sql;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StatisticsBot;
public static class MainService
{
    public static async Task UpdateAll()
    {
        await UpdateUsers();
        await UpdateChart();
    }

    public static async Task UpdateUsers()
    {
        var users = (await DbRepository.ReadAllUsers()).ToDictionary(x => x.CodewarsLogin);
        var infos = await InfoParser.GetInfos(users.Select(x => x.Value.CodewarsLogin).ToArray());

        foreach (var info in infos)
        {
            var user = users[info.Name];
            user.Honor = info.Honor;
            user.TotalTasks = info.TasksCount;
            user.Rank = info.Rank;
            user.LastUpdate = DateTime.UtcNow;

            await DbRepository.UpdateUser(user);
        }
    }

    public static async Task UpdateChart()
    {
        var groupedUsers = (await DbRepository.ReadAllUsers())
            .GroupBy(x => x.ChatId);

        foreach (var group in groupedUsers)
        {
            var chat = group.Key;
            var users = group.ToList();
            ChartGenerator.Generate(users);

            var filename = "chart.png";
            using var stream = new FileStream(filename, FileMode.Open);
            var inputFile = InputFile.FromStream(stream, filename);

            var dbMessage = await DbRepository.ReadChartMessage(chat);
            if (dbMessage == null)
            {
                var message = await BotClient.Bot.SendPhotoAsync(chat, inputFile);
                dbMessage = new Sql.Models.ChartMessage(chat, message.MessageId);
                await DbRepository.CreateChartMessage(dbMessage);
                return;
            }

            try
            {
                var media = new InputMediaPhoto(inputFile);
                var messageSuccess = await BotClient.Bot.EditMessageMediaAsync(dbMessage.ChatId, (int)dbMessage.MessageId, media);
                dbMessage.MessageId = messageSuccess.MessageId;
                await DbRepository.UpdateChartMessage(dbMessage);
            }
            catch (Exception ex)
            {
                await DbRepository.RemoveChartMessage(chat);
                Console.WriteLine($"Update chart exception: {ex.Message}");
            }
        }
    }
}
