namespace MftParser.Mft.Attribute;

/// <summary>
/// A struct that describes a single data run. Nonresident attributes store their data outside their MFT record.
/// An array of data runs is used to describe the location of a nonresident attribute data.
/// Single data runs consists of a single-byte header, followed by a length value and an offset value.
/// The length of both values can vary and is stored in the header
/// (first four least significant bits of the header show the length of the length value, next four - the length of the offset value, both in bytes) 
/// </summary>
/// <param name="Length">The amount of consecutive clusters taken by the data run</param>
/// <param name="Offset">Cluster number offset of the data run relative to the previous one in the list. Can be negative.
/// First run's data will be located at the offset; while the rest should add their offset to the previous one</param>
/// <param name="IsSparse">If set, the block isn't allocated on the disk and should be treated as zeroes</param>
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