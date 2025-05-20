namespace NtfsParser.Mft.ParsedAttributeData.Index;

public record struct IndexNodeHeader(uint EntryListOffset, uint EntryListEndOffset, uint EntryListBufferEndOffset,
    bool HasChildren)
{
    public static IndexNodeHeader Parse(Span<byte> rawHeader)
    {
        var reader = new SpanBinaryReader(rawHeader);
        var entryListOffset = reader.ReadUInt32();
        var entryListEndOffset = reader.ReadUInt32();
        var entryListBufferEndOffset = reader.ReadUInt32();
        var flag = reader.ReadUInt32();

        return new IndexNodeHeader(entryListOffset, entryListEndOffset, entryListBufferEndOffset, flag == 1);
    }
}