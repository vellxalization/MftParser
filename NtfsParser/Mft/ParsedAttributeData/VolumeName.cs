using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

public readonly record struct VolumeName(UnicodeName Name)
{
    public static VolumeName CreateFromRawData(in RawAttributeData rawData) => new(new UnicodeName(rawData.Data));
}