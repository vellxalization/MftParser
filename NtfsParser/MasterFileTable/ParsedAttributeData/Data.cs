using NtfsParser.MasterFileTable.Attribute;

namespace NtfsParser.MasterFileTable.ParsedAttributeData;

// currently this has no meaningful functionality and exists only to support the pattern of parsed attributes
public record struct Data(byte[] Bytes)
{
    public static Data CreateFromRawData(RawAttributeData rawData) => new Data(rawData.Data);
}