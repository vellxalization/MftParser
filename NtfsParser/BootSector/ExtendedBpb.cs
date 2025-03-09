namespace NtfsParser.BootSector;

public record struct ExtendedBpb(long TotalSectors, long LogicalClusterForMft, long LogicalClusterForMftMirr, 
    sbyte ClustersPerFileRecordSegment, sbyte ClustersPerIndexBlock, long VolumeSerialNumber)
{
    public static ExtendedBpb Parse(Span<byte> rawExBpb)
    {
        var reader = new SpanBinaryReader(rawExBpb);
        reader.Skip(4);
        var totalSectors = reader.ReadInt64();
        var logicalClusterForMft = reader.ReadInt64();
        var logicalClusterForMftMirr = reader.ReadInt64();
        var clustersPerFileRecordSegment = reader.ReadSByte();
        reader.Skip(3); // unused
        var clustersPerIndexBlock = reader.ReadSByte();
        reader.Skip(3); // unused
        var volumeSerialNumber = reader.ReadInt64();
        for (int i = 0; i < 4; ++i) // checksum
        {
            var shouldBeZero = reader.ReadByte();
            if (shouldBeZero != 0)
            {
                throw new ShouldBeZeroException(shouldBeZero, reader.Position);
            }
        }

        return new ExtendedBpb(totalSectors, logicalClusterForMft, logicalClusterForMftMirr,
            clustersPerFileRecordSegment, clustersPerIndexBlock, volumeSerialNumber);
    }
}