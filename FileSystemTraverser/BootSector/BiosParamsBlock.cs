namespace FileSystemTraverser.BootSector;

public record struct BiosParamsBlock(ushort BytesPerSector, byte SectorsPerCluster, byte MediaDescriptor, 
    ushort SectorsPerTrack, ushort NumberOfHeads, uint HiddenSectors)
{
    public static BiosParamsBlock Parse(ReadOnlySpan<byte> rawBpb)
    {
        var reader = new SpanBinaryReader(rawBpb);
        var bytesPerSector = reader.ReadUInt16();
        var sectorsPerCluster = reader.ReadByte();
        for (var i = 0; i < 7; ++i) // reserved, always 0, unused
        { 
            var shouldBeZero = reader.ReadByte();
            if (shouldBeZero != 0)
            {
                throw new ShouldBeZeroException(shouldBeZero, reader.Position);
            }
        }
        
        var mediaDescriptor = reader.ReadByte();
        for (var i = 0; i < 2; ++i) // should be zero
        {
            var shouldBeZero = reader.ReadByte();
            if (shouldBeZero != 0)
            {
                throw new ShouldBeZeroException(shouldBeZero, reader.Position);
            }
        }
        
        var sectorsPerTrack = reader.ReadUInt16(); // not used but still store it
        var numberOfHeads = reader.ReadUInt16(); // not used but still store it
        var hiddenSectors = reader.ReadUInt32(); // not used but still store it
        for (var i = 0; i < 4; ++i)
        {
            var shouldBeZero = reader.ReadByte();
            if (shouldBeZero != 0)
            {
                throw new ShouldBeZeroException(shouldBeZero, reader.Position);
            }
        }
        
        return new BiosParamsBlock(bytesPerSector, sectorsPerCluster, mediaDescriptor, sectorsPerTrack, numberOfHeads, hiddenSectors);
    }
}