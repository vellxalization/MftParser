namespace MftParser.Mft.Attribute;

/// <summary>
/// A structure that describes nonresident's attribute content
/// </summary>
/// <param name="LowestVcn">Starting virtual cluster number of the content. For most of the nonresident attributes is set to 0</param>
/// <param name="HighestVcn">Ending virtual cluster number of the content</param>
/// <param name="DataRunsOffset">Offset to the data runs from the start of the attribute</param>
/// <param name="CompressionUnitSize">Size of the compression unit. Stored value is a power of two.
/// Most common value is 4 which results in the 2^4 = 16 clusters size (65536 bytes, assuming single cluster is 4096 bytes)</param>
/// <param name="AllocatedSize">Total size of all clusters used by the file (including sparse blocks if the file is sparse or compressed).
/// This value is a multiple of the cluster size because NTFS can only store data in clusters.
/// If the file isn't sparse of compressed, Windows will use this value as the "Size on disk"</param>
/// <param name="ActualSize">Actual size of the data in bytes</param>
/// <param name="ValidDataSize">Size of the data that is actually written. Everything after is treated as a zero by file system.
/// https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-setfilevaliddata</param>
/// <param name="AllocatedClustersSize">Present only in sparse and compressed files. Size of all the allocated clusters (without sparse blocks).
/// Windows will use this value as the "Size on disk" if possible</param>
public readonly record struct Nonresident(ulong LowestVcn, ulong HighestVcn, ushort DataRunsOffset, ushort CompressionUnitSize, 
    ulong AllocatedSize, ulong ActualSize, ulong ValidDataSize, ulong AllocatedClustersSize)
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