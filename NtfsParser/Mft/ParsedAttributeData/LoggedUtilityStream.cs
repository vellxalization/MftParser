using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;
/// <summary>
/// An attribute that is used to store additional information. All encrypted files should have this attribute. Can contain anything
/// </summary>
/// <param name="Data">Attribute's raw data</param>
public readonly record struct LoggedUtilityStream(byte[] Data)
{
    public static LoggedUtilityStream CreateFromRawData(in RawAttributeData rawData) => new(rawData.Data);
}