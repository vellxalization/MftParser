namespace FileSystemTraverser.MasterFileTable.ParsedAttributeData._INDEX_ALLOCATION;

public record struct IndexRecordHeader(byte[] Signature, ushort FixUpOffset, ushort FixUpLength, ulong LogSequenceNumber,
    ulong Vcn)
{
    public static IndexRecordHeader CreateFromStream(BinaryReader reader)
    {
        var signature = reader.ReadBytes(4);
        var fixUpOffset = reader.ReadUInt16();
        var fixUpLength = reader.ReadUInt16();
        var logSequenceNumber = reader.ReadUInt64();
        var vcn = reader.ReadUInt64();

        return new IndexRecordHeader(signature, fixUpOffset, fixUpLength, logSequenceNumber, vcn);
    }
}