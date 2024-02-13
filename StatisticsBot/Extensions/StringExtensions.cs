namespace StatisticsBot.Extensions;

public static class StringExtensions
{
    public static string DigitsOnly(this string source)
    {
        return string.Join("", source.Where(x => char.IsDigit(x)));
    }
}
