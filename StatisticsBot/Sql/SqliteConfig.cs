namespace StatisticsBot.Sql;

public class SqliteConfig
{
    public const string DbFileName = "data.db";
    public static string ConnectionString => $"Data Source={DbFileName}";
}
