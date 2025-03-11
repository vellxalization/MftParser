using System.Text;
using NtfsParser.MasterFileTable.Attribute;

namespace NtfsParser.MasterFileTable.ParsedAttributeData;

public record struct VolumeName(byte[] Name)
{
    public static VolumeName CreateFromRawData(RawAttributeData rawData) => new(rawData.Data);

    public string GetStringName() => Encoding.Unicode.GetString(Name);
}