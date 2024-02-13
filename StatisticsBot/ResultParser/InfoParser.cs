using HtmlAgilityPack;
using StatisticsBot.Extensions;

namespace StatisticsBot.ResultParser;

public class InfoParser
{
    private readonly string _login;
    private readonly Dictionary<string, string> _stats;

    private const string StatsSelector = "//div[@class='stat']";

    private const string Rank = "Rank";
    private const string Honor = "Honor";
    private const string Total = "Total Completed Kata";

    public InfoParser(string login)
    {
        _login = login;
        _stats = new();
    }

    public static async Task<UserInfo[]> GetInfos(string[] logins)
    {
        var parsers = logins.Select(x => new InfoParser(x));
        var tasks = parsers.Select(x => x.GetInfo());
        await Task.WhenAll(tasks);

        return tasks.Select(x => x.Result).ToArray();
    }

    public async Task<UserInfo> GetInfo()
    {
        var result = new UserInfo() { Name = _login };
        try
        {
            if (_stats.Count == 0)
            {
                await InitStats();
            }

            result.Honor = GetIntValue(Honor);
            result.TasksCount = GetIntValue(Total);
            if (_stats.TryGetValue(Rank, out var rankStr))
            {
                var rank = GetIntValue(Rank);
                if (rankStr.Contains("dan"))
                {
                    result.Rank = 7 + rank;
                }
                else
                {
                    result.Rank = 8 - rank;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Parsing exception: {ex.Message}");
        }

        return result;
    }

    private int GetIntValue(string key)
    {
        return _stats.TryGetValue(key, out var stringValue) && int.TryParse(stringValue.DigitsOnly(), out var result)
            ? result
            : 0;
    }

    private async Task InitStats()
    {
        var html = await GetHtml($"https://www.codewars.com/users/{_login}/stats");
        var stats = html.DocumentNode.SelectNodes(StatsSelector)
            .Select(x => x.InnerText);

        foreach (var stat in stats)
        {
            var parts = stat.Split(':');
            if (parts.Length != 2)
            {
                continue;
            }

            var key = parts[0];
            var value = parts[1];
            _stats.TryAdd(key, value);
        }
    }

    private static async Task<HtmlDocument> GetHtml(string url)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Not success response code: {response.StatusCode}");
        }

        var text = await response.Content.ReadAsStringAsync();

        var doc = new HtmlDocument();
        doc.LoadHtml(text);

        return doc;
    }
}
