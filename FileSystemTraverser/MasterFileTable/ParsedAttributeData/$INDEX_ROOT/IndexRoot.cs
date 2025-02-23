namespace FileSystemTraverser.MasterFileTable.ParsedAttributeData._INDEX_ROOT;

public record struct IndexRoot(IndexRootHeader Header, IndexNodeHeader NodeHeader, IndexEntry[] Entries)
{
    public static IndexRoot CreateFromData(ref byte[] data)
    {
        using var reader = new BinaryReader(new MemoryStream(data));
        var header = IndexRootHeader.CreateFromStream(reader);
        var nodeHeader = IndexNodeHeader.CreateFromStream(reader);
        List<IndexEntry> entries = new();
        var entry = IndexEntry.CreateFromStream(reader);
        while (!entry.Flags.HasFlag(IndexEntryFlags.LastInList))
        {
            entries.Add(entry);
            entry = IndexEntry.CreateFromStream(reader);
        }
        
        entries.Add(entry); // add last entry
        
        return new IndexRoot(header, nodeHeader, entries.ToArray());
    }
}