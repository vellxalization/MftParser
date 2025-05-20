using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

public record struct VolumeName(UnicodeName Name)
{
    public static VolumeName CreateFromRawData(RawAttributeData rawData) => new(new UnicodeName(rawData.Data));
}