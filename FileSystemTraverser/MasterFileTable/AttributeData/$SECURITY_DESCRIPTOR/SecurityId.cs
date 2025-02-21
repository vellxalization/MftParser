namespace FileSystemTraverser.MasterFileTable.AttributeData._SECURITY_DESCRIPTOR;

public record struct SecurityId(byte[] SId)
{
    public static SecurityId CreateFromStream(BinaryReader reader, int length) => new(reader.ReadBytes(length));
}