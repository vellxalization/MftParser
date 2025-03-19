namespace NtfsParser.Mft.ParsedAttributeData;

public record struct FileTime(long Ticks)
{
    private static readonly DateTimeOffset StartingPoint = new DateTimeOffset(1601, 1, 1, 0, 0, 0, DateTimeOffset.Now.Offset) 
                                                           + DateTimeOffset.Now.Offset;

    public DateTimeOffset ToDateTimeOffset() => StartingPoint + TimeSpan.FromTicks(Ticks);
}