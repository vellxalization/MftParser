namespace NtfsParser.Mft.Attribute;

/// <summary>
/// A structure that describes nonresident's attribute content
/// </summary>
/// <param name="LowestVcn">Starting virtual cluster number of the content. For most of the nonresident attributes is set to 0</param>
/// <param name="HighestVcn">Ending virtual cluster number of the content</param>
/// <param name="DataRunsOffset">Offset from which data runs start</param>
/// <param name="CompressionUnitSize">Compression unit size stored as a power of two.
/// Most common value is 4 which result in a 2^4 (16) clusters size (65536 bytes, assuming cluster size is 4096)</param>
/// <param name="AllocatedSizeByte">Total size of all clusters.
/// Because NTFS can only allocate data in clusters, this value is a multiple of the cluster size</param>
/// <param name="ActualSizeByte">Actual size of the data stored</param>
/// <param name="ValidDataSizeByte">Data that is actually written. Everything after is treated as a zero by file system</param>
/// <param name="AllocatedClustersSizeByte">Size of the clusters that are actually allocated
/// (i.e., size without sparse block because they aren't physically on the disk). Only present in compressed attributes</param>
public readonly record struct Nonresident(ulong LowestVcn, ulong HighestVcn, ushort DataRunsOffset, ushort CompressionUnitSize, 
    ulong AllocatedSizeByte, ulong ActualSizeByte, ulong ValidDataSizeByte, ulong AllocatedClustersSizeByte)
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
        var validDataSizeByte = reader.ReadUInt64();
        var allocatedClustersSizeByte = compressionUnitSizeCluster > 0 ? reader.ReadUInt64() : 0;

        return new Nonresident(lowestVcn, highestVcn, dataRunsOffset, compressionUnitSizeCluster, allocatedSizeByte, actualSizeByte,
            validDataSizeByte, allocatedClustersSizeByte);
    }
}