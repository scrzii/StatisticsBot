namespace StatisticsBot.ResultParser;

public class UserInfo
{
    public string Name { get; set; }
    public int TasksCount { get; set; }
    public int Rank { get; set; }
    public int Honor { get; set; }

    public override string ToString()
    {
        return $"{Name}: \nHonor {Honor} \nRank {Rank} \nTotal tasks {TasksCount}";
    }
}
