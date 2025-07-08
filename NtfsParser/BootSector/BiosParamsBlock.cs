namespace NtfsParser.BootSector;

/// <summary>
/// Structure that describes physical layout of the volume
/// </summary>
/// <param name="BytesPerSector">Amount of bytes in a single sector. Sector is the smallest unit of storage. Most of the disks have this value set to 512 bytes</param>
/// <param name="SectorsPerCluster">Amount of sectors in a single cluster. Cluster is the smallest unit of storage that can be allocated on the disk. Most of the disks have this value set to 8 (4096 bytes per sector)</param>
/// <param name="MediaDescriptor">Information about the media used. Mostly legacy. Value of 0xF8 means a hard drive, 0xF0 - high-density 3.5-inch floppy disk</param>
/// <param name="SectorsPerTrack">Number of disk sectors per drive track. Relevant for INT 13h</param>
/// <param name="NumberOfHeads">Number of heads of a disk. Relevant for INT 13h</param>
/// <param name="HiddenSectors">Number of sectors before boot sector. Generally relevant for INT 13h</param>
public readonly record struct BiosParamsBlock(ushort BytesPerSector, byte SectorsPerCluster, byte MediaDescriptor, 
    ushort SectorsPerTrack, ushort NumberOfHeads, uint HiddenSectors)
{
    public static BiosParamsBlock Parse(Span<byte> rawBpb)
    {
        var reader = new SpanBinaryReader(rawBpb);
        var bytesPerSector = reader.ReadUInt16();
        var sectorsPerCluster = reader.ReadByte();
        for (var i = 0; i < 7; ++i) // reserved, always 0, unused
        { 
            var shouldBeZero = reader.ReadByte();
            if (shouldBeZero != 0)
                throw new ZeroedFieldException(shouldBeZero, reader.Position);
        }
        
        var mediaDescriptor = reader.ReadByte();
        for (var i = 0; i < 2; ++i) // should be zero
        {
            var shouldBeZero = reader.ReadByte();
            if (shouldBeZero != 0)
                throw new ZeroedFieldException(shouldBeZero, reader.Position);
        }
        
        var sectorsPerTrack = reader.ReadUInt16(); // not used but still store it
        var numberOfHeads = reader.ReadUInt16(); // not used but still store it
        var hiddenSectors = reader.ReadUInt32(); // not used but still store it
        for (var i = 0; i < 4; ++i)
        {
            var shouldBeZero = reader.ReadByte();
            if (shouldBeZero != 0)
                throw new ZeroedFieldException(shouldBeZero, reader.Position);
        }
        
        return new BiosParamsBlock(bytesPerSector, sectorsPerCluster, mediaDescriptor, sectorsPerTrack, numberOfHeads, hiddenSectors);
    }
}