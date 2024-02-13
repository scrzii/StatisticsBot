using Microsoft.Data.Sqlite;

namespace StatisticsBot.Sql;

public static class Sqlite
{
    public static async Task Execute(string sql, Dictionary<string, object> parameters = null)
    {
        parameters ??= new();

        using var connection = new SqliteConnection(SqliteConfig.ConnectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var param in parameters)
        {
            command.Parameters.Add(new SqliteParameter(param.Key, param.Value));
        }

        await command.ExecuteNonQueryAsync();
    }

    public static async IAsyncEnumerable<object[]> Read(string sql, Dictionary<string, object> parameters = null)
    {
        parameters ??= new();

        using var connection = new SqliteConnection(SqliteConfig.ConnectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var param in parameters)
        {
            command.Parameters.Add(new SqliteParameter(param.Key, param.Value));
        }

        using var reader = await command.ExecuteReaderAsync();
        while (reader.Read())
        {
            var result = new object[reader.FieldCount];
            reader.GetValues(result);
            yield return result;
        }
    }

    public static async Task<List<object[]>> FastRead(string sql, Dictionary<string, object> parameters = null)
    {
        var result = new List<object[]>();

        await foreach (var row in Read(sql, parameters))
        {
            result.Add(row);
        }

        return result;
    }
}
