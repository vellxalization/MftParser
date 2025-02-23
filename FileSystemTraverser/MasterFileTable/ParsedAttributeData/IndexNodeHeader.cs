namespace FileSystemTraverser.MasterFileTable.ParsedAttributeData;

public record struct IndexNodeHeader(uint EntryListOffset, uint EntryListEndOffset, uint EntryListBufferEndOffset, 
    uint Flag)
{
    public static IndexNodeHeader CreateFromStream(BinaryReader reader)
    {
        var entryListOffset = reader.ReadUInt32();
        var entryListEndOffset = reader.ReadUInt32();
        var entryListBufferEndOffset = reader.ReadUInt32();
        var flag = reader.ReadUInt32();

        return new IndexNodeHeader(entryListOffset, entryListEndOffset, entryListBufferEndOffset, flag);
    }
}