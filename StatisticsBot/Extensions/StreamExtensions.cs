namespace StatisticsBot.Extensions;

public static class StreamExtensions
{
    private const int BufferSize = 4096;

    public static async Task<byte[]> ReadAllBytes(this Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var result = new List<byte>();
        var buffer = new byte[BufferSize];
        while (true)
        {
            var count = await stream.ReadAsync(buffer);
            if (count == 0)
            {
                break;
            }
            result.AddRange(buffer.Take(count));
        }

        return result.ToArray();
    }

    public static async Task WriteAllBytes(this Stream stream, byte[] bytes)
    {
        foreach (var chunk in bytes.Chunk(BufferSize))
        {
            await stream.WriteAsync(chunk.ToArray());
        }
        stream.Seek(0, SeekOrigin.Begin);
    }
}
