namespace FileSystemTraverser.MasterFileTable.AttributeRecord;

public record struct Nonresident(ulong LowestVcn, ulong HighestVcn, ushort DataRunsOffset, ushort CompressionUnitSize, 
    ulong AllocatedSize, ulong DataSize, ulong ValidDataSize, ulong TotalAllocated)
{
    public static Nonresident Parse(ReadOnlySpan<byte> rawNonresident)
    {
        var reader = new SpanBinaryReader(rawNonresident);
        var lowestVcn = reader.ReadUInt64();
        var highestVcn = reader.ReadUInt64();
        var dataRunsOffset = reader.ReadUInt16();
        var compressionUnitSize = reader.ReadUInt16();
        reader.Skip(4); // padding
        var allocatedSize = reader.ReadUInt64();
        var dataSize = reader.ReadUInt64();
        var validDataSize = reader.ReadUInt64();
        var totalAllocated = compressionUnitSize > 0 ? reader.ReadUInt64() : 0;

        return new Nonresident(lowestVcn, highestVcn, dataRunsOffset, compressionUnitSize, allocatedSize, dataSize,
            validDataSize, totalAllocated);
    }
}