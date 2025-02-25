namespace FileSystemTraverser.MasterFileTable.ParsedAttributeData.ExtendedAttribute;

public record struct ExtendedAttributeEntry(uint EntrySize, bool NeedEa, byte CharNameLength, ushort ValueSize, 
    byte[] Name, byte[] Value)
{
    public static ExtendedAttributeEntry Parse(ReadOnlySpan<byte> rawEntry)
    {
        var reader = new SpanBinaryReader(rawEntry);
        var entrySize = reader.ReadUInt32();
        var flags = reader.ReadByte();
        var charNameLength = reader.ReadByte();
        var valueSize = reader.ReadUInt16();
        var name = reader.ReadBytes(charNameLength * 2); // utf-16 encoded, 2 bytes per char
        // TODO: OR IS IT ENCODED IN UTF-16?
        var value = reader.ReadBytes(valueSize);

        return new ExtendedAttributeEntry(entrySize, flags == 0x80, charNameLength, valueSize, name.ToArray(), 
            value.ToArray());
    }
}