namespace MftParser.Mft.Decompression;

public static class SpanExtensions
{
    public static void CopyToAt<T>(this Span<T> source, Span<T> destination, int insertAt)
    {
        if (insertAt < 0)
            throw new ArgumentException("insertAt argument must be greater than or equal to zero");
        
        if (insertAt == 0)
        {
            source.CopyTo(destination);
            return;
        }
        
        if (source.Length + insertAt > destination.Length)
            throw new ArgumentException("The destination buffer is too small.");
        
        var buffer = destination.Slice(insertAt, source.Length);
        source.CopyTo(buffer);
    }
}