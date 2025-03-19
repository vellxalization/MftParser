namespace NtfsParser.Mft.ParsedAttributeData.ExtendedAttribute;

public record struct ExtendedAttributeEntry(uint EntrySize, bool NeedEa, byte CharNameLength, short ValueSize, 
    byte[] Name, byte[] Value)
{
    public static ExtendedAttributeEntry Parse(Span<byte> rawEntry)
    {
        var reader = new SpanBinaryReader(rawEntry);
        var entrySize = reader.ReadUInt32();
        var flags = reader.ReadByte();
        var charNameLength = reader.ReadByte();
        var valueSize = reader.ReadInt16();
        var name = reader.ReadBytes(charNameLength);
        var value = reader.ReadBytes(valueSize); 
        
        return new ExtendedAttributeEntry(entrySize, flags == 0x80, charNameLength, valueSize, name.ToArray(), 
            value.ToArray());
    }
}