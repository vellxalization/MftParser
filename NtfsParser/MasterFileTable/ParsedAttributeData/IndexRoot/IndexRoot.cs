using NtfsParser.MasterFileTable.AttributeRecord;

namespace NtfsParser.MasterFileTable.ParsedAttributeData.IndexRoot;

public record struct IndexRoot(IndexRootHeader Header, IndexNodeHeader NodeHeader, IndexEntry[] Entries)
{
    public static IndexRoot CreateFromRawData(RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var rawHeader = reader.ReadBytes(16);
        var header = IndexRootHeader.Parse(rawHeader);
        var beforeNodeHeader = reader.Position; // offsets to entries are relative to the node header so we copy it
        rawHeader = reader.ReadBytes(16);
        var nodeHeader = IndexNodeHeader.Parse(rawHeader);
        List<IndexEntry> entries = new();
        var entriesBoundary = beforeNodeHeader + (int)nodeHeader.EntryListEndOffset;
        reader.Position = beforeNodeHeader + (int)nodeHeader.EntryListOffset;
        var rawEntries = reader.ReadBytes(entriesBoundary - reader.Position);
        var entry = IndexEntry.Parse(rawEntries);
        while (!entry.Flags.HasFlag(IndexEntryFlags.LastInList))
        {
            entries.Add(entry);
            rawEntries = rawHeader.Slice(entry.EntryLength);
            entry = IndexEntry.Parse(rawEntries);
        }
        
        entries.Add(entry); // add last entry
        
        return new IndexRoot(header, nodeHeader, entries.ToArray());
    }
}