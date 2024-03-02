using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StatisticsBot.Services.Data.Models;

[Table("Notifications")]
public class Notification
{
    [Key]
    public int Id { get; set; }
    [MaxLength(200)]
    public string Message { get; set; }
    public bool WithMarkup { get; set; }
    [Required]
    public long ChatId { get; set; }

    public Notification(string message, long chatId, bool withMarkup = true)
    {
        Message = message;
        ChatId = chatId;
        WithMarkup = withMarkup;
    }
}
