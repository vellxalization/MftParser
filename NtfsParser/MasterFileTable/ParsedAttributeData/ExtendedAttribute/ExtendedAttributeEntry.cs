namespace NtfsParser.MasterFileTable.ParsedAttributeData.ExtendedAttribute;

public record struct ExtendedAttributeEntry(uint EntrySize, bool NeedEa, byte CharNameLength, short ValueSize, 
    byte[] Name, byte[] Value)
{
    public static ExtendedAttributeEntry Parse(ReadOnlySpan<byte> rawEntry)
    {
        var reader = new SpanBinaryReader(rawEntry);
        var entrySize = reader.ReadUInt32();
        var flags = reader.ReadByte();
        var charNameLength = reader.ReadByte();
        var valueSize = reader.ReadInt16();
        var name = reader.ReadBytes(charNameLength);
        var diff = (int)(entrySize - reader.Position);
        var value = reader.ReadBytes(valueSize > diff ? diff : valueSize); 
        // sometimes we can get an inadequate size of the value so we do this. This WILL grab some unused bytes
        // TODO: maybe I should mark the entry as a non valid
        
        return new ExtendedAttributeEntry(entrySize, flags == 0x80, charNameLength, valueSize, name.ToArray(), 
            value.ToArray());
    }
}