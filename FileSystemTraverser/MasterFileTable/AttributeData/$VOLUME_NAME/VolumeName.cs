using System.Text;

namespace FileSystemTraverser.MasterFileTable.AttributeData._VOLUME_NAME;

public record struct VolumeName(byte[] Name)
{
    public static VolumeName CreateFromData(ref byte[] data, int size)
    {
        using var reader = new BinaryReader(new MemoryStream(data));
        return new VolumeName(reader.ReadBytes(size));
    }

    public string GetStringName() => Encoding.Unicode.GetString(Name);
}