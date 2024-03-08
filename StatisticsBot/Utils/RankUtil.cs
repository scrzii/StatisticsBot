namespace StatisticsBot.Utils;

public static class RankUtil
{
    public static int ParseDisplay(string source)
    {
        var number = int.Parse(string.Join("", source.Where(x => char.IsDigit(x))));
        var classification = string.Join("", source.ToLower().Where(x => char.IsLetter(x)));
        if (classification == "kyu" || classification == "кю")
        {
            return 8 - number;
        }

        return number + 7;
    }

    public static int ParseCodewarsFormat(int source)
    {
        return source < 0 ? 8 + source : 7 + source;
    }

    public static string ToStringRu(int rank)
    {
        return ToDisplay(rank).ToString() + (IsDan(rank) ? " дан" : " кю");
    }

    public static string ToStringEn(int rank)
    {
        return ToDisplay(rank).ToString() + (IsDan(rank) ? " dan" : " kyu");
    }

    private static int ToDisplay(int rank) => rank < 8 ? 8 - rank : rank - 7;

    private static bool IsDan(int rank) => rank > 7;
}
