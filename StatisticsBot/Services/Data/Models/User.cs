using SkiaSharp;
using StatisticsBot.Extensions;
using StatisticsBot.Utils;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StatisticsBot.Services.Data.Models;

[Table("Users")]
public class User
{
    [Key]
    public long TelegramId { get; set; }
    [Required, MaxLength(50)]
    public string CodewarsLogin { get; set; }
    public int? Rank { get; set; }
    public int? Honor { get; set; }
    public int? TotalTasks { get; set; }
    public DateTime? LastUpdate { get; set; }
    public long ChatId { get; set; }
    [Required, MaxLength(7)]
    public string ColorHex { get; set; }

    [NotMapped]
    public SKColor Color
    {
        get => SKColor.TryParse(ColorHex, out var color) ? color : SKColor.Empty;
        set => ColorHex = value.ToHex();
    }

    public User(long telegramId,
        long chatId,
        string codewarsLogin,
        int? rank = null,
        int? honor = null,
        int? totalTasks = null,
        DateTime? lastUpdate = null,
        SKColor? color = null)
    {
        TelegramId = telegramId;
        ChatId = chatId;
        CodewarsLogin = codewarsLogin;
        Rank = rank;
        Honor = honor;
        TotalTasks = totalTasks;
        LastUpdate = lastUpdate;
        Color = color ?? ColorUtils.GenerateRandom(TelegramId);
    }

    public User() { }

    public override string ToString()
    {
        return $"[{LastUpdate}] {TelegramId} ({CodewarsLogin} - {Rank}): {Honor} ({TotalTasks} tasks)";
    }
}
