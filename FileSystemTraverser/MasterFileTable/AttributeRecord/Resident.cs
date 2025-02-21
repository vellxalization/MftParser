namespace FileSystemTraverser.MasterFileTable.AttributeRecord;

public record struct Resident(uint Size, ushort Offset, byte IndexedFlag)
{
    public static Resident CreateFromStream(BinaryReader reader)
    {
        var size = reader.ReadUInt32();
        var offset = reader.ReadUInt16();
        var indexedFlag = reader.ReadByte();
        _ = reader.ReadByte(); // padding
        
        return new Resident(size, offset, indexedFlag);
    }
}