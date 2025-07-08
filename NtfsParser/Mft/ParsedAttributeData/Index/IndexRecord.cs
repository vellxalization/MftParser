namespace NtfsParser.Mft.ParsedAttributeData.Index;

/// <summary>
/// A collection of index entries. Each index record is a single node in an index tree.
/// Always stored in an index allocation. Multiple records can be stored in a single allocation
/// </summary>
/// <param name="RecordHeader">Header with information about the record</param>
/// <param name="NodeHeader">Header of an index node</param>
/// <param name="FixUp">Fix up values of the record</param>
/// <param name="Entries">Index entries</param>
public readonly record struct IndexRecord(IndexRecordHeader RecordHeader, IndexNodeHeader NodeHeader, FixUp FixUp,
    IndexEntry[] Entries)
{
    public static IndexRecord Parse(Span<byte> rawRecord, int sectorSize)
    {
        var reader = new SpanBinaryReader(rawRecord);
        var rawHeader = reader.ReadBytes(24);
        var recordHeader = IndexRecordHeader.Parse(rawHeader);

        var offset = reader.Position; // offsets to entries are relative to the record header so we copy it
        rawHeader = reader.ReadBytes(16);
        var nodeHeader = IndexNodeHeader.Parse(rawHeader);

        reader.Position = recordHeader.FixUpOffset;
        var rawFixUp = reader.ReadBytes(recordHeader.FixUpSize * 2);
        var fixUp = FixUp.Parse(rawFixUp);
        fixUp.ReverseFixUp(rawRecord, sectorSize);

        reader.Position = offset + (int)nodeHeader.EntryListOffset;
        var entriesLength = offset + (int)nodeHeader.EntryListEndOffset - reader.Position;
        var rawEntries = reader.ReadBytes(entriesLength);
        var entries = IndexEntry.ParseEntries(rawEntries);
        
        fixUp.ApplyFixUp(rawRecord, sectorSize);
        return new IndexRecord(recordHeader, nodeHeader, fixUp, entries);
    }
}

/// <summary>
/// Header of an index record
/// </summary>
/// <param name="FixUpOffset">Offset to the fixup array from the end of the header</param>
/// <param name="FixUpSize">Size of the fixup array in bytes. Single fix up value is 2-bytes long</param>
/// <param name="LogSequenceNumber">LSN. Used by $LogFile</param>
/// <param name="Vcn">Virtual Cluster Number. Relative to the index allocation where this record is stored</param>
public record struct IndexRecordHeader(ushort FixUpOffset, ushort FixUpSize, ulong LogSequenceNumber, ulong Vcn)
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