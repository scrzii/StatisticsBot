using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using StatisticsBot.Utils;

namespace StatisticsBot.Services.Parsing;

public class CodewarsParser
{
    private readonly UserDto _user;

    public CodewarsParser(string codewarsLogin)
    {
        _user = new() { Name = codewarsLogin };
    }

    public async Task<UserDto> Parse()
    {
        await ParseRankAndHonor();
        await ParseTasksCount();

        return _user;
    }

    private async Task ParseRankAndHonor()
    {
        var url = $"https://www.codewars.com/api/v1/users/{_user.Name}";
        var user = await GetResult(url);
        _user.Honor = user.honor;
        _user.Rank = RankUtil.ParseCodewarsFormat((int)user?.ranks?.overall?.rank);
    }

    private async Task ParseTasksCount()
    {
        var url = $"https://www.codewars.com/api/v1/users/{_user.Name}/code-challenges/completed";
        var content = await GetResult(url);
        _user.TotalTasks = content?.totalItems;
    }

    public async Task<TaskDto> GetLastTask()
    {
        var result = (await ParseTasks()).MaxBy(x => x.CompletedAt);
        result.Difficulty = await GetTaskDifficulty(result.Id);

        return result;
    }

    private async Task<int> GetTaskDifficulty(string taskId)
    {
        var url = $"https://www.codewars.com/kata/{taskId}";
        var xPath = "//div[contains(@class, 'px-0') and contains(@class, 'w-full')]//span";
        var html = new HtmlDocument();
        html.LoadHtml(await GetResultString(url));
        var difficultyTag = html.DocumentNode.SelectNodes(xPath).First();

        return RankUtil.ParseDisplay(difficultyTag.InnerText);
    }

    private async Task<TaskDto[]> ParseTasks()
    {
        var result = new List<TaskDto>();
        var firstPage = await ParseTasksPage(0);
        var pagesCount = GetPagesCount(firstPage);
        result.AddRange(ParsePage(firstPage));

        for (var pageIndex = 1; pageIndex < pagesCount; pageIndex++)
        {
            var page = await ParseTasksPage(pageIndex);
            result.AddRange(ParsePage(page));
        }

        return result.ToArray();
    }

    private async Task<dynamic> ParseTasksPage(int page)
    {
        var url = $"https://www.codewars.com/api/v1/users/{_user.Name}/code-challenges/completed?page={page}";
        return await GetResult(url);
    }

    private int GetPagesCount(dynamic parsedInput)
    {
        return parsedInput.totalPages;
    }

    private TaskDto[] ParsePage(dynamic parsedInput)
    {
        var list = ((JArray)parsedInput.data).ToObject<List<dynamic>>();
        return list
            .Select(x => new TaskDto { Id = (string)x.id, CompletedAt = (DateTime)x.completedAt, Name = (string)x.name })
            .ToArray();
    }

    private async Task<dynamic> GetResult(string url)
    {
        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Request response error, code: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();
        return JObject.Parse(content);
    }

    private async Task<string> GetResultString(string url)
    {
        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Request response error, code: {response.StatusCode}");
        }

        return await response.Content.ReadAsStringAsync();
    }
}
