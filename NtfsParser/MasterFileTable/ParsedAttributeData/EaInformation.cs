using NtfsParser.MasterFileTable.Attribute;

namespace NtfsParser.MasterFileTable.ParsedAttributeData;

public record struct EaInformation(ushort EaEntrySize, ushort NeedEaFlagsCount, uint EaDataSize)
{
    public static EaInformation CreateFromRawData(RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var eaEntrySize = reader.ReadUInt16();
        var needEaFlagsCount = reader.ReadUInt16();
        var eaDataSize = reader.ReadUInt32();
        return new EaInformation(eaEntrySize, needEaFlagsCount, eaDataSize);
    }
}