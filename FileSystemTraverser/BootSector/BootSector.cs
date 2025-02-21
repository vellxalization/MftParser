namespace FileSystemTraverser.BootSector;

public record struct BootSector(byte[] Jmp, ulong OemId, BiosParamsBlock Bpb, ExtendedBpb ExtBpb, byte[] Bootstrap, ushort SectorEnd)
{
    public static BootSector CreateFromStream(BinaryReader reader)
    {
        if (reader.BaseStream.Position != 0)
        {
            throw new InvalidStartingPositionException(0, reader.BaseStream.Position);
        }
        
        var jmp = reader.ReadBytes(3);
        if (jmp is not [0xEB, 0x52, 0x90])
        {
            throw new InvalidJmpException(jmp, reader.BaseStream.Position);
        }
        
        var oemId = reader.ReadUInt64();
        if (oemId != 0x202020205346544E)
        {
            throw new InvalidOemIdException(oemId, reader.BaseStream.Position);
        }
        
        var bpb = BiosParamsBlock.CreateFromStream(reader);
        var extBpb = ExtendedBpb.CreateFromStream(reader);
        var bootstrap = reader.ReadBytes(426);
        var sectorEnd = reader.ReadUInt16();
        if (sectorEnd != 0xAA55)
        {
            throw new InvalidEndMarkerException(sectorEnd);
        }
        
        return new BootSector(jmp, oemId, bpb, extBpb, bootstrap, sectorEnd);
    }
}