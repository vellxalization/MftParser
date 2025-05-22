namespace NtfsParser.Mft.Attribute;

public readonly record struct DataRun(long Length, long Offset, bool IsSparse)
{
    public static DataRun[] ParseDataRuns(byte[] rawDataRuns)
    {
        var span = rawDataRuns.AsSpan();
        List<DataRun> runs = new();
        var i = 0;
        while (i < rawDataRuns.Length)
        {
            var header = rawDataRuns[i++];
            var lengthNibble = header & 0x0F;
            var offsetNibble = (header & 0xF0) >> 4;
            var dataRunLength = lengthNibble + offsetNibble;
            var rawDataRun = span.Slice(i, dataRunLength);
            i += dataRunLength;
            runs.Add(ParseDataRun(rawDataRun, lengthNibble, offsetNibble));
        }
        
        return runs.ToArray();
    }

    private static DataRun ParseDataRun(Span<byte> rawDataRun, int lengthNibble, int offsetNibble)
    {
        var rawLength = rawDataRun[..lengthNibble];
        var length = BytesToLong(rawLength);
        if (offsetNibble == 0)
            return new DataRun(length, 0, true);

        var rawOffset = rawDataRun.Slice(lengthNibble, offsetNibble);
        var offset = BytesToLong(rawOffset);
        if (IsNegative(offset, offsetNibble - 1))
            offset = OverflowNumber(offset, offsetNibble);
        
        return new DataRun(length, offset, false);
    }

    private static long BytesToLong(Span<byte> data)
    {
        long value = 0;
        for (var j = 0; j < data.Length; ++j)
            value |= (long)data[j] << (8 * j);

        return value;
    }

    private static bool IsNegative(long number, int msbIndex)
    {
        var mask = 0x80 << (8 * msbIndex);
        return (number & mask) != 0;
    }

    private static long OverflowNumber(long number, int offsetLength)
    {
        // we're using long numbers so we calculate everything with 8 bytes per number in mind
        for (var v = 7; v >= offsetLength; --v)
            number |= (long)0xFF << (8 * v);
        
        return number;
    }
}