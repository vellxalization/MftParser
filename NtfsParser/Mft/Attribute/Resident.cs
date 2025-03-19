namespace NtfsParser.Mft.Attribute;

public record struct Resident(uint Size, ushort Offset, byte IndexedFlag)
{
    public static Resident Parse(Span<byte> rawResident)
    {
        var reader = new SpanBinaryReader(rawResident);
        var size = reader.ReadUInt32();
        var offset = reader.ReadUInt16();
        var indexedFlag = reader.ReadByte();
        // 1 spare byte for 8-byte alignment
        
        return new Resident(size, offset, indexedFlag);
    }
}