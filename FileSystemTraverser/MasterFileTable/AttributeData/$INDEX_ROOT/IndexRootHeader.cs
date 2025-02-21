using FileSystemTraverser.MasterFileTable.AttributeRecord;

namespace FileSystemTraverser.MasterFileTable.AttributeData._INDEX_ROOT;

public record struct IndexRootHeader(AttributeType AttributeType, CollationRule CollationRule, uint IndexRecordByteSize, 
    byte IndexRecordClusterSize)
{
    public static IndexRootHeader CreateFromStream(BinaryReader reader)
    {
        var attributeType = reader.ReadUInt32();
        var collationRule = reader.ReadUInt32();
        var indexRecordByteSize = reader.ReadUInt32();
        var indexRecordClusterSize = reader.ReadByte();
        reader.BaseStream.Position += 3; // padding

        return new IndexRootHeader((AttributeType)attributeType, (CollationRule)collationRule, indexRecordByteSize, indexRecordClusterSize);
    }
}

public enum CollationRule : uint
{
    Binary = 0x00000000,
    Filename = 0x00000001,
    UnicodeString = 0x00000002,
    NtofsUlong = 0x00000010,
    NtofsSid = 0x00000011,
    NtofsSecurityHash = 0x00000012,
    NtofsUlongs = 0x00000013
}