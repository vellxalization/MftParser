namespace NtfsParser.MasterFileTable.ParsedAttributeData.IndexAllocation;

public record struct IndexRecord(IndexRecordHeader Header, IndexNodeHeader IndexNodeHeader, byte[] FixUp, 
    IndexEntry[] Entries)
{
    public static IndexRecord Parse(ReadOnlySpan<byte> rawRecord)
    {
        var reader = new SpanBinaryReader(rawRecord);
        var rawHeader = reader.ReadBytes(24);
        var header = IndexRecordHeader.Parse(rawHeader);
        var beforeNodeHeader = reader.Position; // offsets to entries are relative to the node header so we copy it
        rawHeader = reader.ReadBytes(16);
        var nodeHeader = IndexNodeHeader.Parse(rawHeader);
        reader.Position = header.FixUpOffset;
        var fixUp = reader.ReadBytes(header.FixUpLength);
        List<IndexEntry> entries = new();
        var entriesBoundary = beforeNodeHeader + (int)nodeHeader.EntryListEndOffset;
        reader.Position = beforeNodeHeader + (int)nodeHeader.EntryListOffset;
        var rawEntries = reader.ReadBytes(entriesBoundary - reader.Position);
        var entry = IndexEntry.Parse(rawEntries);
        while (!entry.Flags.HasFlag(IndexEntryFlags.LastInList))
        {
            entries.Add(entry);
            rawEntries = rawEntries.Slice(entry.EntryLength);
            entry = IndexEntry.Parse(rawEntries);
        }
        
        entries.Add(entry); // add last entry
        
        return new IndexRecord(header, nodeHeader, fixUp.ToArray(), entries.ToArray());
    }
}