using System.Runtime.InteropServices;
using NtfsParser.MasterFileTable.Attribute;

namespace NtfsParser.MasterFileTable.ParsedAttributeData.ReparsePoint;

public record struct ReparsePoint(ReparseTag ReparseTag, ushort DataSize, RawReparseData Data, Guid ThirdPartyGuid)
{
    public static ReparsePoint CreateFromRawData(RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var reparseTag = new ReparseTag(reader.ReadUInt32());
        var reparseDataSize = reader.ReadUInt16();
        reader.Skip(2); // unused
        Guid thirdPartyGuid = Guid.Empty;
        if (!reparseTag.GetFlags().HasFlag(ReparseFlags.IsMicrosoft))
        {
            var guidBytes = reader.ReadBytes(16);
            thirdPartyGuid = Guid.Parse(MemoryMarshal.Cast<byte, char>(guidBytes));
        }
        var reparseData = reader.ReadBytes(reparseDataSize);
        
        return new ReparsePoint(reparseTag, reparseDataSize, new RawReparseData(reparseData.ToArray()), thirdPartyGuid);
    }
}
