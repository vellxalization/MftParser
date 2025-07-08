using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

/// <summary>
/// A collection of extended attributes entries. Used to support HPFS within NTFS
/// </summary>
/// <param name="Entries">A collection of key-value pairs</param>
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

/// <summary>
/// Single key-value pair of an extended attribute
/// </summary>
/// <param name="EntrySize">Size of the entry</param>
/// <param name="NeedEa">If true, file's data cannot be interpreted without understanding the extended attributes.
/// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/wdm/ns-wdm-_file_full_ea_information</param>
/// <param name="NameLength">Size of the key in ASCII characters</param>
/// <param name="ValueSize">Size of the value in bytes</param>
/// <param name="Name">Name of the attribute (the key)</param>
/// <param name="Value">Value of the attribute</param>
public readonly record struct ExtendedAttributeEntry(uint EntrySize, bool NeedEa, byte NameLength, short ValueSize,
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