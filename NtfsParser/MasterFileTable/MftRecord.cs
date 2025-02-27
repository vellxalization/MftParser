using NtfsParser.MasterFileTable.AttributeRecord;
using NtfsParser.MasterFileTable.Header;

namespace NtfsParser.MasterFileTable;

public record struct MftRecord(MftRecordHeader RecordHeader, MftAttribute[] Attributes)
{
    public static MftRecord Parse(ReadOnlySpan<byte> rawMftRecord)
    {
        var reader = new SpanBinaryReader(rawMftRecord);
        var header = MftRecordHeader.CreateFromStream(ref reader);
        reader.Position = header.AttributesOffset;
        var attributes = new List<MftAttribute>(1);
        var attributesSpan = reader.ReadBytes((int)header.UsedEntrySize - reader.Position);
        var attribute = MftAttribute.Parse(attributesSpan);
        while (attribute.Header.Type != AttributeType.EndOfAttributeList)
        {
            attributes.Add(attribute);
            var splitPoint = (int)attribute.Header.Size; 
            // ^ using this to avoid headache of constantly caching ^
            // starting position of the reader
            attributesSpan = attributesSpan.Slice(splitPoint);
            attribute = MftAttribute.Parse(attributesSpan);
        }
        
        // rest is unused bytes
        return new MftRecord(header, attributes.ToArray());
    }
}