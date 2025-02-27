using System.Text;
using NtfsParser.MasterFileTable.AttributeRecord;

namespace NtfsParser.MasterFileTable.ParsedAttributeData;

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