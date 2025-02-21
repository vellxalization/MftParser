namespace FileSystemTraverser.MasterFileTable;

public record struct IndexEntry(byte[] Bytes, ushort EntryLength, ushort ContentLength, IndexEntryFlags Flags, 
    byte[] Content, ulong ChildVcn)
{
    public static IndexEntry CreateFromStream(BinaryReader reader)
    {
        var start = reader.BaseStream.Position;
        var bytes = reader.ReadBytes(8);
        var entryLength = reader.ReadUInt16();
        var contentLength = reader.ReadUInt16();
        var flags = (IndexEntryFlags)reader.ReadUInt32();
        var content = reader.ReadBytes(contentLength);
        if (!flags.HasFlag(IndexEntryFlags.ChildExists))
        {
            reader.BaseStream.Position = start + entryLength;
            return new IndexEntry(bytes, entryLength, contentLength, flags, content, 0);
        }
        
        reader.BaseStream.Position = start + (entryLength - 8);
        var childVcn = reader.ReadUInt64();
        reader.BaseStream.Position = start + entryLength;
        return new IndexEntry(bytes, entryLength, contentLength, flags, content, childVcn);
    }
}

[Flags]
public enum IndexEntryFlags
{
    ChildExists = 0x01,
    LastInList = 0x02
}