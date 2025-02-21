using FileSystemTraverser.MasterFileTable.AttributeData._INDEX_ROOT;

namespace FileSystemTraverser.MasterFileTable.AttributeData._INDEX_ALLOCATION;

public record struct IndexRecord(IndexRecordHeader Header, IndexNodeHeader IndexNodeHeader, byte[] FixUp, 
    IndexEntry[] Entries)
{
    public static IndexRecord CreateFromStream(BinaryReader reader)
    {
        var header = IndexRecordHeader.CreateFromStream(reader);
        if (header.Signature is [0, 0, 0, 0])
        {
            Console.WriteLine("");
            return new IndexRecord();
        }
        
        var beforeNodeHeader = reader.BaseStream.Position;
        var nodeHeader = IndexNodeHeader.CreateFromStream(reader);
        reader.BaseStream.Position = header.FixUpOffset;
        var fixUp = reader.ReadBytes(header.FixUpLength);
        reader.BaseStream.Position = beforeNodeHeader + nodeHeader.EntryListOffset;
        List<IndexEntry> entries = new();
        var entry = IndexEntry.CreateFromStream(reader);
        while (!entry.Flags.HasFlag(IndexEntryFlags.LastInList))
        {
            entries.Add(entry);
            entry = IndexEntry.CreateFromStream(reader);
        }
        
        entries.Add(entry); // add last entry
        return new IndexRecord(header, nodeHeader, fixUp, entries.ToArray());
    }
}