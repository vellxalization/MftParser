namespace NtfsParser.Mft.ParsedAttributeData.Index;

/// <summary>
/// Header of a single index node
/// </summary>
/// <param name="EntryListOffset">Offset at where index entries start</param>
/// <param name="EntryListEndOffset">Offset at where used portion of the index entries end.
/// Deleted entries might be stored after this offset</param>
/// <param name="EntryListBufferEndOffset">Offset at where allocated portion of the index entries end</param>
/// <param name="HasChildren">Node has a child node</param>
public readonly record struct IndexNodeHeader(uint EntryListOffset, uint EntryListEndOffset, uint EntryListBufferEndOffset,
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