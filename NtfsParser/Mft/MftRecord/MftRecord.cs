using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.MftRecord;

public record struct MftRecord(MftRecordHeader RecordHeader, MftAttribute[] Attributes)
{
    public static MftRecord Parse(Span<byte> rawMftRecord, int sectorSize)
    {
        var reader = new SpanBinaryReader(rawMftRecord);
        var header = MftRecordHeader.CreateFromStream(ref reader);
        if (header.Header.Signature == MftSignature.Empty)
            return default;
        
        header.FixUp.ReverseFixUp(rawMftRecord, sectorSize);
        reader.Position = header.AttributesOffset;
        var rawAttributes = reader.ReadBytes((int)header.UsedEntrySize - reader.Position);
        var attributes = MftAttribute.ParseAttributes(rawAttributes);
        // rest is unused bytes
        return new MftRecord(header, attributes);
    }
}