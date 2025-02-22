namespace FileSystemTraverser.MasterFileTable.AttributeData._ATTRIBUTE_LIST;

public record struct AttributeList()
{
    public static AttributeList CreateFromData(ref byte[] data)
    {
        using var reader = new BinaryReader(new MemoryStream(data));
        var entry = AttributeListEntry.CreateFromStream(reader);
        return new AttributeList();
    }
}