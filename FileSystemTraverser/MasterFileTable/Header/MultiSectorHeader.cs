namespace FileSystemTraverser.MasterFileTable.Header;

public record struct MultiSectorHeader(byte[] Signature, ushort UpdateSequenceOffset, ushort UpdateSequenceLength)
{
    public static MultiSectorHeader CreateFromStream(BinaryReader reader)
    {
        var signature = reader.ReadBytes(4);
        var updateSequenceOffset = reader.ReadUInt16();
        var updateSequenceLength = reader.ReadUInt16();
        
        return new MultiSectorHeader(signature, updateSequenceOffset, updateSequenceLength);
    }
}