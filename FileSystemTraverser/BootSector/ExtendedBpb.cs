namespace FileSystemTraverser.BootSector;

public record struct ExtendedBpb(long TotalSectors, long LogicalClusterForMft, long LogicalClusterForMftMirr, 
    sbyte ClustersPerFileRecordSegment, sbyte ClustersPerIndexBlock, long VolumeSerialNumber)
{
    public static ExtendedBpb CreateFromStream(BinaryReader reader)
    {
        if (reader.BaseStream.Position != 0x24)
        {
            throw new InvalidStartingPositionException(0x24, reader.BaseStream.Position);
        }

        _ = reader.ReadInt32(); // unused
        var totalSectors = reader.ReadLongLong();
        var logicalClusterForMft = reader.ReadLongLong();
        var logicalClusterForMftMirr = reader.ReadLongLong();
        var clustersPerFileRecordSegment = reader.ReadSByte();
        _ = reader.ReadBytes(3); // unused
        var clustersPerIndexBlock = reader.ReadSByte();
        _ = reader.ReadBytes(3); // unused
        var volumeSerialNumber = reader.ReadLongLong();
        for (int i = 0; i < 4; ++i) // checksum
        {
            var shouldBeZero = reader.ReadByte();
            if (shouldBeZero != 0)
            {
                throw new ShouldBeZeroException(shouldBeZero, reader.BaseStream.Position);
            }
        }

        return new ExtendedBpb(totalSectors, logicalClusterForMft, logicalClusterForMftMirr,
            clustersPerFileRecordSegment, clustersPerIndexBlock, volumeSerialNumber);
    }
}