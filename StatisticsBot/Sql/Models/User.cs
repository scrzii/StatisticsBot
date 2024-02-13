using SkiaSharp;
using StatisticsBot.Utils;
using System.Drawing;

namespace StatisticsBot.Sql.Models;

public class User
{
    public string CodewarsLogin { get; set; }
    public long TelegramId { get; set; }
    public int? Rank { get; set; }
    public int? Honor { get; set; }
    public int? TotalTasks { get; set; }
    public DateTime? LastUpdate { get; set; }
    public long ChatId { get; set; }
    public string Color { get; set; }

    public User(long telegramId, 
        long chatId,
        string codewarsLogin, 
        int? rank = null, 
        int? honor = null, 
        int? totalTasks = null, 
        DateTime? lastUpdate = null,
        string color = null)
    {
        TelegramId = telegramId;
        ChatId = chatId;
        CodewarsLogin = codewarsLogin;
        Rank = rank;
        Honor = honor;
        TotalTasks = totalTasks;
        LastUpdate = lastUpdate;
        Color = color ?? ColorUtils.GenerateRandom(TelegramId).ToString();
    }

    public override string ToString()
    {
        return $"[{LastUpdate}] {TelegramId} ({CodewarsLogin} - {Rank}): {Honor} ({TotalTasks} tasks)";
    }
}
