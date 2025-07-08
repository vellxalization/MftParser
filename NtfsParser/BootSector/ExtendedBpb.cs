namespace NtfsParser.BootSector;

/// <summary>
/// Structure that describes physical layout of the volume
/// </summary>
/// <param name="TotalSectors">Total size of the volume</param>
/// <param name="MftCluster">Cluster number of the $MFT meta file data</param>
/// <param name="MftMirrCluster">Cluster number of the $MFTMirr meta file data</param>
/// <param name="ClustersPerMftRecord">Number of clusters used per single MFT record. If this value is greater than 0, then it represents number of clusters.
/// If it's negative, then use it's absolute value as a power of two to get record size in bytes.
/// Typical value is 0xF6 (-10) meaning that we use 1024 bytes per single record.</param>
/// <param name="ClustersPerIndexRecord">Number of cluster used per single INDX record. If this value is greater than 0, then it represents number of clusters.
/// If it's negative, then use it's absolute value as a power of two to get record size in bytes.
/// Typical value is 1 meaning that we use single cluster (4096 bytes typically) per single record</param>
/// <param name="VolumeSerialNumber">Unique volume id</param>
public readonly record struct ExtendedBpb(long TotalSectors, long MftCluster, long MftMirrCluster, 
    sbyte ClustersPerMftRecord, sbyte ClustersPerIndexRecord, long VolumeSerialNumber)
{
    public static ExtendedBpb Parse(Span<byte> rawExBpb)
    {
        var reader = new SpanBinaryReader(rawExBpb);
        reader.Skip(4);
        var totalSectors = reader.ReadInt64();
        var logicalClusterForMft = reader.ReadInt64();
        var logicalClusterForMftMirr = reader.ReadInt64();
        var clustersPerMftRecord = reader.ReadSByte();
        reader.Skip(3); // unused
        var clustersPerIndexRecord = reader.ReadSByte();
        reader.Skip(3); // unused
        var volumeSerialNumber = reader.ReadInt64();
        for (int i = 0; i < 4; ++i) // checksum
        {
            var shouldBeZero = reader.ReadByte();
            if (shouldBeZero != 0)
                throw new ZeroedFieldException(shouldBeZero, reader.Position);
        }

        return new ExtendedBpb(totalSectors, logicalClusterForMft, logicalClusterForMftMirr,
            clustersPerMftRecord, clustersPerIndexRecord, volumeSerialNumber);
    }
}