using MftParser.Mft.Attribute;

namespace MftParser.Mft.ParsedAttributeData;

/// <summary>
/// An attribute that contains record's content
/// Currently, this has no meaningful functionality and exists only to support the pattern of parsed attributes
/// </summary>
/// <param name="Content">Record's content</param>
public readonly record struct Data(byte[] Content)
{
    public static Data CreateFromRawData(in RawAttributeData rawData) => new(rawData.Data);
}