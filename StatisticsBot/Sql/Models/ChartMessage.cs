namespace StatisticsBot.Sql.Models;

public class ChartMessage
{
    public long ChatId { get; set; }
    public long MessageId { get; set; }

    public ChartMessage(long chatId, long messageId)
    {
        ChatId = chatId;
        MessageId = messageId;
    }
}
