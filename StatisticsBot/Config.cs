using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StatisticsBot;

public class Config
{
    private const string _filename = "config.json";

    private static Config _instance { get; set; }
    public static Config Instance
    {
        get
        {
            if (_instance == null)
            {
                Init();
            }

            return _instance;
        }
    }

    public string TelegramToken { get; set; }
    public string DbName { get; set; }
    [JsonIgnore]
    public string ConnectionString => $"Data Source={GetDbPath()}";
    [JsonIgnore]
    public bool IsDev { get; set; } = true;

    private string GetDbPath()
    {
        var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        return Path.Combine(directory, DbName);
    }

    public static void Init()
    {
        if (!File.Exists(_filename))
        {
            _instance = new Config
            {
                TelegramToken = "###",
                DbName = "data.db"
            };

            var options = new JsonSerializerOptions() { WriteIndented = true };
            File.WriteAllText(_filename, JsonSerializer.Serialize(Instance, options));
            return;
        }
        else
        {
            _instance = JsonSerializer.Deserialize<Config>(File.ReadAllText(_filename));
        }

#if RELEASE
        _instance.IsDev = false;
#endif
    }
}
