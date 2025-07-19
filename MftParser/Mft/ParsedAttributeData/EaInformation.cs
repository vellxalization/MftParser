using MftParser.Mft.Attribute;

namespace MftParser.Mft.ParsedAttributeData;

/// <summary>
/// Attribute that contains information about extended attribute. Used to support HPFS within NTFS
/// </summary>
/// <param name="PackedSize">Packed size of entries in extended attribute in bytes</param>
/// <param name="NeedEaFlagsCount">Number of entries that have "NeedEa" flag set</param>
/// <param name="UnpackedSize">Unpacked size of entries in extended attribute in bytes. Extended attributes use this value as their size</param>
public readonly record struct EaInformation(ushort PackedSize, ushort NeedEaFlagsCount, uint UnpackedSize)
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