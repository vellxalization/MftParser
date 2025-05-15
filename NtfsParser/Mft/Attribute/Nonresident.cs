namespace NtfsParser.Mft.Attribute;

public record struct Nonresident(ulong LowestVcn, ulong HighestVcn, ushort DataRunsOffset, ushort CompressionUnitSize, 
    ulong AllocatedSizeByte, ulong ActualSizeByte, ulong InitializedDataSizeByte, ulong AllocatedClustersSizeByte)
{
    public static Nonresident Parse(Span<byte> rawNonresident)
    {
        var reader = new SpanBinaryReader(rawNonresident);
        var lowestVcn = reader.ReadUInt64();
        var highestVcn = reader.ReadUInt64();
        var dataRunsOffset = reader.ReadUInt16();
        var compressionUnitSizeCluster = reader.ReadUInt16();
        reader.Skip(4); // padding
        var allocatedSizeByte = reader.ReadUInt64();
        var actualSizeByte = reader.ReadUInt64();
        var initializedDataSizeByte = reader.ReadUInt64();
        var allocatedClustersSizeByte = compressionUnitSizeCluster > 0 ? reader.ReadUInt64() : 0;

        return new Nonresident(lowestVcn, highestVcn, dataRunsOffset, compressionUnitSizeCluster, allocatedSizeByte, actualSizeByte,
            initializedDataSizeByte, allocatedClustersSizeByte);
    }
}