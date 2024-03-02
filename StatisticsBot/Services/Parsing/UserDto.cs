namespace StatisticsBot.Services.Parsing;

public class UserDto
{
    public string Name { get; set; }
    public int? Honor { get; set; }
    public int? TotalTasks { get; set; }
    public int? Rank { get; set; }

    public bool IsCorrect => Honor != null && TotalTasks != null && Rank != null;
}
