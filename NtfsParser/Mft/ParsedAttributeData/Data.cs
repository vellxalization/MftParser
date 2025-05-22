using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

// currently this has no meaningful functionality and exists only to support the pattern of parsed attributes
public readonly record struct Data(byte[] Bytes)
{
    public static Data CreateFromRawData(in RawAttributeData rawData) => new(rawData.Data);
}