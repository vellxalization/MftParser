namespace FileSystemTraverser.BootSector;

public record struct BiosParamsBlock(ushort BytesPerSector, byte SectorsPerCluster, byte MediaDescriptor, 
    ushort SectorsPerTrack, ushort NumberOfHeads, uint HiddenSectors)
{
    public static BiosParamsBlock CreateFromStream(BinaryReader reader)
    {
        if (reader.BaseStream.Position != 0x0B)
        {
            throw new InvalidStartingPositionException(0x0B, reader.BaseStream.Position);
        }
        
        var bytesPerSector = reader.ReadWord();
        var sectorsPerCluster = reader.ReadByte();
        for (var i = 0; i < 7; ++i) // reserved, always 0, unused
        { 
            var shouldBeZero = reader.ReadByte();
            if (shouldBeZero != 0)
            {
                throw new ShouldBeZeroException(shouldBeZero, reader.BaseStream.Position);
            }
        }
        
        var mediaDescriptor = reader.ReadByte();
        for (var i = 0; i < 2; ++i) // should be zero
        {
            var shouldBeZero = reader.ReadByte();
            if (shouldBeZero != 0)
            {
                throw new ShouldBeZeroException(shouldBeZero, reader.BaseStream.Position);
            }
        }
        
        var sectorsPerTrack = reader.ReadWord(); // not used but still store it
        var numberOfHeads = reader.ReadWord(); // not used but still store it
        var hiddenSectors = reader.ReadDword(); // not used but still store it
        for (var i = 0; i < 4; ++i)
        {
            var shouldBeZero = reader.ReadByte();
            if (shouldBeZero != 0)
            {
                throw new ShouldBeZeroException(shouldBeZero, reader.BaseStream.Position);
            }
        }
        
        return new BiosParamsBlock(bytesPerSector, sectorsPerCluster, mediaDescriptor, sectorsPerTrack, numberOfHeads, hiddenSectors);
    }
}