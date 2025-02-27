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
            int splitPoint = (int)attribute.Header.Size;
            if (splitPoint > attributesSpan.Length)
            {
                splitPoint = GetAttributeRecordSize(attribute); 
                // some attributes might have an inadequate size in the header so we need to calculate the proper size.
                // no idea why this is happening
                attribute = attribute with
                {
                    Header = attribute.Header with
                    {
                        Size = (uint)splitPoint
                    }
                };
            }
            
            attributes.Add(attribute);
            attributesSpan = attributesSpan.Slice(splitPoint);
            // ^ using this to avoid headache of constantly caching ^
            // starting position of the reader
            attribute = MftAttribute.Parse(attributesSpan);
        }
        
        // rest is unused bytes
        return new MftRecord(header, attributes.ToArray());
    }

    private static int GetAttributeRecordSize(MftAttribute attribute)
    {
        var header = attribute.Header;
        int totalSize;
        int rem;
        if (!header.IsNonresident)
        {
            totalSize = header.Resident.Offset;
            totalSize += (int)header.Resident.Size;
            rem = totalSize % 8;
            if (rem != 0)
            {
                totalSize += 8 - rem; // add 8-byte alignment padding
            }
            
            return totalSize;
        }

        totalSize = header.Nonresident.DataRunsOffset;
        totalSize += attribute.Value.Length + 1; // add 1 to compensate for the not included 0xFF header
        rem = totalSize % 8;
        if (rem != 0)
        {
            totalSize += 8 - rem; // add 8-byte alignment padding
        }
        
        return totalSize;
    }
}