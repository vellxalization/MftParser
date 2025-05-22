using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

public readonly record struct ExtendedAttribute(ExtendedAttributeEntry[] Entries)
{
    public static ExtendedAttribute CreateFromRawData(in RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var entries = new List<ExtendedAttributeEntry>();
        while (data.Length > 0)
        {
            var entry = ExtendedAttributeEntry.Parse(data);
            entries.Add(entry);
            data = data.Slice((int)entry.EntrySize);
        }

        return new ExtendedAttribute(entries.ToArray());
    }
}

public readonly record struct ExtendedAttributeEntry(uint EntrySize, bool NeedEa, byte CharNameLength, short ValueSize,
    AsciiName Name, byte[] Value)
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

        return new ExtendedAttributeEntry(entrySize, flags == 0x80, charNameLength, valueSize,
            new AsciiName(name.ToArray()), value.ToArray());
    }
}