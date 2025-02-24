using System.Text;
using FileSystemTraverser.MasterFileTable.AttributeRecord;

namespace FileSystemTraverser.MasterFileTable.ParsedAttributeData.VolumeName;

public record struct VolumeName(byte[] Name)
{
    public static VolumeName CreateFromRawData(RawAttributeData rawData, int byteSize)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        return new VolumeName(reader.ReadBytes(byteSize).ToArray());
    }

    public string GetStringName() => Encoding.Unicode.GetString(Name);
}