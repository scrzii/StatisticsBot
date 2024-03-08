namespace StatisticsBot.Services.Parsing;

public class TaskDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime CompletedAt { get; set; }
    public int? Difficulty { get; set; }

    public string Url => $"https://www.codewars.com/kata/{Id}";
}
