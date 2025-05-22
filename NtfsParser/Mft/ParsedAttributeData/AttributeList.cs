using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

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

public record struct AttributeListEntry(AttributeType AttributeType, ushort RecordSize, byte NameSize, byte NameOffset,
    ulong Vcn, FileReference FileReference, ushort AttributeId, UnicodeName Name)
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