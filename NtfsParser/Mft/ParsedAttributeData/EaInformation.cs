using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

/// <summary>
/// Attribute that contains information about EXTENDED_ATTRIBUTE
/// </summary>
/// <param name="EaEntrySize">Packed size of entries in extended attribute in bytes</param>
/// <param name="NeedEaFlagsCount">Number of entries that have "NeedEa" flag set</param>
/// <param name="EaDataSize">Unpacked size of entries in extended attribute in bytes</param>
public readonly record struct EaInformation(ushort EaEntrySize, ushort NeedEaFlagsCount, uint EaDataSize)
{
    public static EaInformation CreateFromRawData(in RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var eaEntrySize = reader.ReadUInt16();
        var needEaFlagsCount = reader.ReadUInt16();
        var eaDataSize = reader.ReadUInt32();
        return new EaInformation(eaEntrySize, needEaFlagsCount, eaDataSize);
    }
}