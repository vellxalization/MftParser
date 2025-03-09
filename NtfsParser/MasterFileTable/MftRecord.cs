using NtfsParser.MasterFileTable.AttributeRecord;
using NtfsParser.MasterFileTable.Header;

namespace NtfsParser.MasterFileTable;

public record struct MftRecord(MftRecordHeader RecordHeader, MftAttribute[] Attributes)
{
    public static MftRecord Parse(Span<byte> rawMftRecord, int sectorSize)
    {
        var reader = new SpanBinaryReader(rawMftRecord);
        var header = MftRecordHeader.CreateFromStream(ref reader);
        ReverseFixUp(ref rawMftRecord, sectorSize, header.FixUpPlaceHolder, header.FixUpValues);
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
        var fixUpLength = fixUpValues.Length / 2;
        for (int i = 0; i < fixUpLength; ++i)
        {
            var lastBytesOffset = (i + 1) * sectorSize - 2;
            if (rawMftRecord[lastBytesOffset] != fixUpPlaceholder[0] 
                || rawMftRecord[lastBytesOffset + 1] != fixUpPlaceholder[1])
            {
                throw new Exception("Fixup mismatch. Possibly a corrupted sector!"); // TODO: temp solution
            }

            var valuesOffset = i * 2;
            rawMftRecord[lastBytesOffset] = fixUpValues[valuesOffset];
            rawMftRecord[lastBytesOffset + 1] = fixUpValues[valuesOffset + 1];
        }
    }
}