using NtfsParser.MasterFileTable.AttributeRecord;
using NtfsParser.MasterFileTable.Header;

namespace NtfsParser.MasterFileTable;

public record struct MftRecord(MftRecordHeader RecordHeader, MftAttribute[] Attributes)
{
    public static MftRecord Parse(Span<byte> rawMftRecord, int sectorSize)
    {
        var reader = new SpanBinaryReader(rawMftRecord);
        var header = MftRecordHeader.CreateFromStream(ref reader);
        header.FixUp.ReverseFixUp(rawMftRecord, sectorSize);
        reader.Position = header.AttributesOffset;
        var attributes = new List<MftAttribute>(1);
        var attributesSpan = reader.ReadBytes((int)header.UsedEntrySize - reader.Position);
        var attribute = MftAttribute.Parse(attributesSpan);
        while (attribute.Header.Type != AttributeType.EndOfAttributeList)
        {
            int splitPoint = (int)attribute.Header.Size;
            attributes.Add(attribute);
            attributesSpan = attributesSpan.Slice(splitPoint);
            // ^ using this to avoid headache of constantly caching ^
            // starting position of the reader
            attribute = MftAttribute.Parse(attributesSpan);
        }
        
        // rest is unused bytes
        return new MftRecord(header, attributes.ToArray());
    }

    private static void ReverseFixUp(ref Span<byte> rawMftRecord, int sectorSize, byte[] fixUpPlaceholder, byte[] fixUpValues)
    {
        
    }
}