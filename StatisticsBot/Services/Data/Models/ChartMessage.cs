using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StatisticsBot.Services.Data.Models;

[Table("ChartMessages")]
public class ChartMessage
{
    [Key]
    public long ChatId { get; set; }
    public long MessageId { get; set; }

    public ChartMessage(long chatId, long messageId)
    {
        ChatId = chatId;
        MessageId = messageId;
    }

    public ChartMessage() { }
}
