using System.Reflection;

namespace StatisticsBot;

public static class ResourceHelper
{
    public static string Read(string path)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(x => x.Contains(path));
        if (string.IsNullOrEmpty(resourceName))
        {
            return null;
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
