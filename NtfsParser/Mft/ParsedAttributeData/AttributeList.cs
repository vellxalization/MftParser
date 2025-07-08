using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

/// <summary>
/// A list of attributes. Used when a single MFT record is too small to contain all the attributes.
/// A new MFT record is created to fit the rest of the attributes.
/// </summary>
/// <param name="Entries">A list of the entries</param>
public readonly record struct AttributeList(AttributeListEntry[] Entries)
{
    public static AttributeList CreateFromRawData(in RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var entries = new List<AttributeListEntry>();
        while (data.Length > 0)
        {
            var entry = AttributeListEntry.Parse(data);
            entries.Add(entry);
            data = data.Slice(entry.RecordSize);
            // ^ doing this to save myself from the headache of not having 0-based indecies
        }

        return new AttributeList(entries.ToArray());
    }
}

/// <summary>
/// An entry that contains basic information about an attribute and contains reference to the MFT record that contains it
/// </summary>
/// <param name="AttributeType">Attribute type</param>
/// <param name="RecordSize">Size of the record in bytes</param>
/// <param name="NameSize">Size of the name in Unicode characters</param>
/// <param name="NameOffset">Offset to the name from the start of the struct</param>
/// <param name="Vcn">Virtual Cluster Number</param>
/// <param name="RecordReference">Reference to the record that contains the attribute</param>
/// <param name="AttributeId">ID of the attribute</param>
/// <param name="Name">Name</param>
public record struct AttributeListEntry(AttributeType AttributeType, ushort RecordSize, byte NameSize, byte NameOffset,
    ulong Vcn, FileReference RecordReference, ushort AttributeId, UnicodeName Name)
{
    public static AttributeListEntry Parse(Span<byte> rawData)
    {
        var reader = new SpanBinaryReader(rawData);
        var attributeType = reader.ReadUInt32();
        var recordSize = reader.ReadUInt16();
        var nameSize = reader.ReadByte();
        var nameOffset = reader.ReadByte();
        var vcn = reader.ReadUInt64();
        var fileReference = FileReference.Parse(reader.ReadBytes(8));
        var attributeId = reader.ReadUInt16();
        if (nameSize == 0)
            return new AttributeListEntry((AttributeType)attributeType, recordSize, nameSize, nameOffset, vcn, fileReference, attributeId, UnicodeName.Empty);

        reader.Position = nameOffset;
        var name = reader.ReadBytes(nameSize * 2); // utf-16 encoded, 2 bytes per char

        return new AttributeListEntry((AttributeType)attributeType, recordSize, nameSize, nameOffset, vcn, fileReference,
            attributeId, new UnicodeName(name.ToArray()));
    }
}