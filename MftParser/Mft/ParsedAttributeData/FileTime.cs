namespace MftParser.Mft.ParsedAttributeData;

/// <summary>
/// Represents number of 100-nanoseconds ticks since 00:00 January 1st, 1601 (UTC)  
/// </summary>
/// <param name="Ticks">Number of ticks</param>
public readonly record struct FileTime(long Ticks)
{
    private static readonly DateTimeOffset StartingPoint = new DateTimeOffset(1601, 1, 1, 0, 0, 0, DateTimeOffset.Now.Offset) 
                                                           + DateTimeOffset.Now.Offset;

    public DateTimeOffset ToDateTimeOffset() => StartingPoint + TimeSpan.FromTicks(Ticks);
}