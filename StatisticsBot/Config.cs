using System.Text.Json;

namespace StatisticsBot;

public class Config
{
    private const string _filename = "config.json";

    public static Config Instance { get; private set; }

    public string TelegramToken { get; set; }

    public static void Init()
    {
        if (!File.Exists(_filename))
        {
            Instance = new Config
            {
                TelegramToken = "###"
            };

            var options = new JsonSerializerOptions() { WriteIndented = true };
            File.WriteAllText(_filename, JsonSerializer.Serialize(Instance, options));
            return;
        }

        Instance = JsonSerializer.Deserialize<Config>(File.ReadAllText(_filename));
    }
}
