using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace StatisticsBot.Services.Parsing;

public class CodewarsUserParser
{
    private readonly UserDto _user;

    public CodewarsUserParser(string codewarsLogin)
    {
        _user = new() { Name = codewarsLogin };
    }

    public async Task<UserDto> Parse()
    {
        await ParseRankAndHonor();
        await ParseTasks();

        return _user;
    }

    private async Task ParseRankAndHonor()
    {
        var url = $"https://www.codewars.com/api/v1/users/{_user.Name}";
        var content = await GetResult(url);
        _user.Honor = (int) content["honor"];
        _user.Rank = (int) content?["ranks"]?["overall"]?["rank"];

        if (_user.Rank > 0)
        {
            _user.Rank--;
        }
        _user.Rank += 8;
    }

    private async Task ParseTasks()
    {
        var url = $"https://www.codewars.com/api/v1/users/{_user.Name}/code-challenges/completed";
        var content = await GetResult(url);
        _user.TotalTasks = (int) content?["totalItems"];
    }

    private async Task<dynamic> GetResult(string url)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Request response error, code: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(content);
    }
}
