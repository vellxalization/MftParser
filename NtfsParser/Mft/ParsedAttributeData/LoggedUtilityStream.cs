using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

// currently this has no meaningful functionality and exists only to support the pattern of parsed attributes
public record struct LoggedUtilityStream(byte[] Data)
{
    public static LoggedUtilityStream CreateFromRawData(RawAttributeData rawData) => new LoggedUtilityStream(rawData.Data);
}