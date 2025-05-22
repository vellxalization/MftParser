namespace NtfsParser.Mft.ParsedAttributeData.Index;

public readonly record struct IndexEntry(byte[] Bytes, ushort EntryLength, ushort ContentLength, IndexEntryFlags Flags,
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
        var bytes = reader.ReadBytes(8);
        var entryLength = reader.ReadUInt16();
        var contentLength = reader.ReadUInt16();
        var flags = (IndexEntryFlags)reader.ReadUInt32();
        var content = reader.ReadBytes(contentLength);
        if (!flags.HasFlag(IndexEntryFlags.ChildExists))
            return new IndexEntry(bytes.ToArray(), entryLength, contentLength, flags, content.ToArray(), 0);

        reader.Position = entryLength - 8;
        var childVcn = reader.ReadUInt64();
        return new IndexEntry(bytes.ToArray(), entryLength, contentLength, flags, content.ToArray(), childVcn);
    }
}

[Flags]
public enum IndexEntryFlags
{
    ChildExists = 0x01,
    LastInList = 0x02
}