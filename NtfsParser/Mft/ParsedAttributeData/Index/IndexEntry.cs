namespace NtfsParser.Mft.ParsedAttributeData.Index;

/// <summary>
/// Single entry in an index node. Multiple entries can be stored in a single node
/// </summary>
/// <param name="RawStructure">A structure that is specific to the index. $I30 will store here a file reference</param>
/// <param name="EntryLength">Length of the entry in bytes</param>
/// <param name="ContentLength">Length of the entry's content in bytes</param>
/// <param name="Flags">Entry's flags</param>
/// <param name="Content">Entry's content. $I30 will store here a FILE_NAME attribute</param>
/// <param name="ChildVcn">VCN of a child node (if exists) in an index allocation</param>
public readonly record struct IndexEntry(byte[] RawStructure, ushort EntryLength, ushort ContentLength, IndexEntryFlags Flags,
    byte[] Content, ulong ChildVcn)
{
    public static IndexEntry[] ParseEntries(Span<byte> rawEntries)
    {
        if (rawEntries.Length == 0)
            return [];

        var parsedEntries = new List<IndexEntry>();
        var parsedEntry = Parse(rawEntries);
        while (!parsedEntry.Flags.HasFlag(IndexEntryFlags.LastInList))
        {
            parsedEntries.Add(parsedEntry);
            rawEntries = rawEntries[parsedEntry.EntryLength..];
            parsedEntry = Parse(rawEntries);
        }

        parsedEntries.Add(parsedEntry); // add last entry
        return parsedEntries.ToArray();
    }

    private static IndexEntry Parse(Span<byte> rawEntry)
    {
        var reader = new SpanBinaryReader(rawEntry);
        var rawStructure = reader.ReadBytes(8);
        var entryLength = reader.ReadUInt16();
        var contentLength = reader.ReadUInt16();
        var flags = (IndexEntryFlags)reader.ReadUInt32();
        var content = reader.ReadBytes(contentLength);
        if (!flags.HasFlag(IndexEntryFlags.ChildExists))
            return new IndexEntry(rawStructure.ToArray(), entryLength, contentLength, flags, content.ToArray(), 0);

        reader.Position = entryLength - 8;
        var childVcn = reader.ReadUInt64();
        return new IndexEntry(rawStructure.ToArray(), entryLength, contentLength, flags, content.ToArray(), childVcn);
    }
}

[Flags]
public enum IndexEntryFlags
{
    /// <summary>
    /// A child for a node exists
    /// </summary>
    ChildExists = 0x01,
    /// <summary>
    /// Entry is the last in a list. It has no content but can still have a child node
    /// </summary>
    LastInList = 0x02
}