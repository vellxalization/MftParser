using System.Runtime.InteropServices;
using NtfsParser.MasterFileTable.Attribute;

namespace NtfsParser.MasterFileTable.ParsedAttributeData;

public record struct ObjectId(Guid Id, Guid BirthVolumeId, Guid BirthObjectId, Guid DomainId)
{
    public static ObjectId CreateFromRawData(RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var objectId = reader.ReadBytes(16);
        var charSpan = MemoryMarshal.Cast<byte, char>(objectId);
        var objectIdGuid = Guid.Parse(charSpan);
        
        var birthVolumeId = reader.ReadBytes(16);
        charSpan = MemoryMarshal.Cast<byte, char>(birthVolumeId);
        var birthVolumeIdGuid = Guid.Parse(charSpan);
        
        var birthObjectId = reader.ReadBytes(16);
        charSpan = MemoryMarshal.Cast<byte, char>(birthObjectId);
        var birthObjectIdGuid = Guid.Parse(charSpan);
        
        var domainId = reader.ReadBytes(16);
        charSpan = MemoryMarshal.Cast<byte, char>(domainId);
        var domainIdGuid = Guid.Parse(charSpan);
        
        return new ObjectId(objectIdGuid, birthVolumeIdGuid, birthObjectIdGuid, domainIdGuid);
    }
}