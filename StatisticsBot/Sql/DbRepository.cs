using StatisticsBot.Sql.Models;

namespace StatisticsBot.Sql;

public static class DbRepository
{
    public static async Task Init()
    {
        File.WriteAllText(SqliteConfig.DbFileName, "");
        var scripts = ResourceHelper.Read(ScriptConsts.Init).Split("\n\n");
        foreach (var script in scripts)
        {
            await Sqlite.Execute(script);
        }
    }

    public static async Task CreateUser(User user)
    {
        var parameters = new Dictionary<string, object>();
        parameters["@tgId"] = user.TelegramId;
        parameters["@cwLogin"] = user.CodewarsLogin;
        parameters["@chatId"] = user.ChatId;

        await Sqlite.Execute(@"INSERT INTO Users (TelegramId, CWLogin, ChatId) VALUES (@tgId, @cwLogin, @chatId)", parameters);
    }

    public static async Task<List<User>> ReadAllUsers()
    {
        var result = new List<User>();
        var rows = Sqlite.Read(@"SELECT TelegramId, ChatId, CWLogin, Rank, Honor, TotalTasks, LastUpdate, Color FROM Users");
        await foreach (var row in rows)
        {
            result.Add(new User((long)row[0], (long)row[1], (string)row[2], CastToInt(row[3]), 
                CastToInt(row[4]), CastToInt(row[5]), CastToDateTime(row[6]), (string)row[7]));
        }

        return result;
    }

    public static async Task<User> ReadUser(long telegramId)
    {
        var result = new List<User>();
        var parameters = new Dictionary<string, object>();
        parameters["@tgId"] = telegramId;
        var rows = Sqlite.Read(@"
SELECT 
  TelegramId, 
  ChatId, 
  CWLogin, 
  Rank, 
  Honor, 
  TotalTasks, 
  LastUpdate,
  Color
FROM 
  Users 
WHERE 
  TelegramId = @tgId
        ", parameters);
        await foreach (var row in rows)
        {
            result.Add(new User((long)row[0], (long)row[1], (string)row[2], CastToInt(row[3]),
                CastToInt(row[4]), CastToInt(row[5]), CastToDateTime(row[6]), (string)row[7]));
        }

        return result.FirstOrDefault();
    }

    public static async Task<User> ReadUser(string codewarsLogin)
    {
        var result = new List<User>();
        var parameters = new Dictionary<string, object>();
        parameters["@cwLogin"] = codewarsLogin;
        var rows = Sqlite.Read(@"
SELECT 
  TelegramId, 
  ChatId, 
  CWLogin, 
  Rank, 
  Honor, 
  TotalTasks, 
  LastUpdate,
  Color
FROM 
  Users 
WHERE 
  CWLogin = @cwLogin
        ", parameters);
        await foreach (var row in rows)
        {
            result.Add(new User((long)row[0], (long)row[1], (string)row[2], CastToInt(row[3]),
                CastToInt(row[4]), CastToInt(row[5]), CastToDateTime(row[6]), (string)row[7]));
        }

        return result.FirstOrDefault();
    }

    public static async Task UpdateUser(User user)
    {
        var parameters = new Dictionary<string, object>();
        parameters["@tgId"] = user.TelegramId;
        parameters["@chatId"] = user.ChatId;
        parameters["@cwLogin"] = user.CodewarsLogin;
        parameters["@honor"] = user.Honor;
        parameters["@total"] = user.TotalTasks;
        parameters["@rank"] = user.Rank;
        parameters["@lastUpdate"] = user.LastUpdate;
        parameters["@color"] = user.Color;

        await Sqlite.Execute(@"
UPDATE 
  Users 
SET 
  CWLogin = @cwLogin, 
  ChatId = @chatId,
  Honor = @honor, 
  TotalTasks = @total, 
  Rank = @rank, 
  LastUpdate = @lastUpdate,
  Color = @color
WHERE 
  TelegramId = @tgId
        ", parameters);
    }

    public static async Task RemoveUser(long telegramId)
    {
        var parameters = new Dictionary<string, object>();
        parameters["@tgId"] = telegramId;

        await Sqlite.Execute(@"DELETE FROM Users WHERE TelegramId = @tgId", parameters);
    }

    public static async Task CreateChartMessage(ChartMessage chart)
    {
        var parameters = new Dictionary<string, object>();
        parameters["@chatId"] = chart.ChatId;
        parameters["@messageId"] = chart.MessageId;

        await Sqlite.Execute(@"INSERT INTO ChartMessages (ChatId, MessageId) VALUES (@chatId, @messageId)", parameters);
    }

    public static async Task<ChartMessage> ReadChartMessage(long chatId)
    {
        var parameters = new Dictionary<string, object>();
        parameters["@chatId"] = chatId;

        var row = (await Sqlite.FastRead(@"SELECT ChatId, MessageId FROM ChartMessages WHERE ChatId = @chatId", parameters))
            .FirstOrDefault();
        if (row == null)
        {
            return null;
        }

        return new ChartMessage((long)row[0], (long)row[1]);
    }

    public static async Task UpdateChartMessage(ChartMessage chart)
    {
        var parameters = new Dictionary<string, object>();
        parameters["@chatId"] = chart.ChatId;
        parameters["@messageId"] = chart.MessageId;

        await Sqlite.Execute(@"UPDATE ChartMessages SET MessageId = @messageId WHERE ChatId = @chatId", parameters);
    }

    public static async Task RemoveChartMessage(long chatId)
    {
        var parameters = new Dictionary<string, object>();
        parameters["@chatId"] = chatId;

        await Sqlite.Execute(@"DELETE FROM ChartMessages WHERE chatId = @chatId", parameters);
    }

    private static int? CastToInt(object source) => Convert.IsDBNull(source) ? null : (int?)(long?)source;
    private static DateTime? CastToDateTime(object source) => Convert.IsDBNull(source) ? null : DateTime.Parse((string)source);
}
