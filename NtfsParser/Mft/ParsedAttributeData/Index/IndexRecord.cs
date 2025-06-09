namespace NtfsParser.Mft.ParsedAttributeData.Index;

public readonly record struct IndexRecord(IndexRecordHeader Header, IndexNodeHeader IndexNodeHeader, FixUp FixUp,
    IndexEntry[] Entries)
{
    public static IndexRecord Parse(Span<byte> rawRecord, int sectorSize)
    {
        var reader = new SpanBinaryReader(rawRecord);
        var rawHeader = reader.ReadBytes(24);
        var header = IndexRecordHeader.Parse(rawHeader);

        var offset = reader.Position; // offsets to entries are relative to the node header so we copy it
        rawHeader = reader.ReadBytes(16);
        var nodeHeader = IndexNodeHeader.Parse(rawHeader);

        reader.Position = header.FixUpOffset;
        var rawFixUp = reader.ReadBytes(header.FixUpLength * 2);
        var fixUp = FixUp.Parse(rawFixUp);
        fixUp.ReverseFixUp(rawRecord, sectorSize);

        reader.Position = offset + (int)nodeHeader.EntryListOffset;
        var entriesLength = offset + (int)nodeHeader.EntryListEndOffset - reader.Position;
        var rawEntries = reader.ReadBytes(entriesLength);
        var entries = IndexEntry.ParseEntries(rawEntries);
        
        fixUp.ReapplyFixUp(rawRecord, sectorSize);
        return new IndexRecord(header, nodeHeader, fixUp, entries);
    }
}

public record struct IndexRecordHeader(ushort FixUpOffset, ushort FixUpLength, ulong LogSequenceNumber, ulong Vcn)
{
    public static IndexRecordHeader Parse(Span<byte> rawHeader)
    {
        var reader = new SpanBinaryReader(rawHeader);
        var signature = reader.ReadBytes(4);
        if (signature is not [(byte)'I', (byte)'N', (byte)'D', (byte)'X'])
            throw new InvalidIndexException(signature);

        var fixUpOffset = reader.ReadUInt16();
        var fixUpLength = reader.ReadUInt16();
        var logSequenceNumber = reader.ReadUInt64();
        var vcn = reader.ReadUInt64();

        return new IndexRecordHeader(fixUpOffset, fixUpLength, logSequenceNumber, vcn);
    }
}