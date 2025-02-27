namespace NtfsParser.MasterFileTable.ParsedAttributeData.IndexAllocation;

public record struct IndexRecordHeader(ushort FixUpOffset, ushort FixUpLength, ulong LogSequenceNumber,
    ulong Vcn)
{
    public static IndexRecordHeader Parse(ReadOnlySpan<byte> rawHeader)
    {
        var reader = new SpanBinaryReader(rawHeader);
        var signature = reader.ReadBytes(4);
        if (signature is not [(byte)'I', (byte)'N', (byte)'D', (byte)'X'])
        {
            throw new Exception("Expected an index signature"); // TODO: temp solution
        } 
        var fixUpOffset = reader.ReadUInt16();
        var fixUpLength = reader.ReadUInt16();
        var logSequenceNumber = reader.ReadUInt64();
        var vcn = reader.ReadUInt64();

        return new IndexRecordHeader(fixUpOffset, fixUpLength, logSequenceNumber, vcn);
    }
}