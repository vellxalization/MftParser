using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData.ReparsePoint;

/// <summary>
/// An attribute that represents reparse point. Reparse points can store any user-defined data.
/// The format of the data is specified in the tag. Symbolic links and junctions both use reparse points mechanism
/// </summary>
/// <param name="ReparseTag">Tag</param>
/// <param name="DataSize">Size of the content</param>
/// <param name="Data">Content. Different reparse point types will have different content</param>
/// <param name="ThirdPartyGuid">GUID for the non-Microsoft reparse points.
/// Should only be present if the tag doesn't contain "IsMicrosoft" flag</param>
public readonly record struct ReparsePoint(ReparseTag ReparseTag, ushort DataSize, RawReparseData Data, Guid ThirdPartyGuid)
{
    public static ReparsePoint CreateFromRawData(in RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var reparseTag = new ReparseTag(reader.ReadUInt32());
        var reparseDataSize = reader.ReadUInt16();
        reader.Skip(2); // unused
        Guid thirdPartyGuid = Guid.Empty;
        if ((reparseTag.GetFlags() & ReparseFlags.IsMicrosoft) == 0)
        {
            var guidBytes = reader.ReadBytes(16);
            thirdPartyGuid = new Guid(guidBytes);
        }
        var reparseData = reader.ReadBytes(reparseDataSize);
        
        return new ReparsePoint(reparseTag, reparseDataSize, new RawReparseData(reparseData.ToArray()), thirdPartyGuid);
    }
}
