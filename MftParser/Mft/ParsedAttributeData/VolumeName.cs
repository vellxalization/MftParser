using MftParser.Mft.Attribute;

namespace MftParser.Mft.ParsedAttributeData;

/// <summary>
/// An attribute that contains volume name. Used only by $Volume meta file
/// </summary>
/// <param name="Name">Name of the volume</param>
public readonly record struct VolumeName(UnicodeName Name)
{
    public static VolumeName CreateFromRawData(in RawAttributeData rawData) => new(new UnicodeName(rawData.Data));
}