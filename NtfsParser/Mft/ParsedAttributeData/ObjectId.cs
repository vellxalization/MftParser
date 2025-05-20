using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

public record struct ObjectId(Guid Id, Guid BirthVolumeId, Guid BirthObjectId, Guid DomainId)
{
    public static ObjectId CreateFromRawData(RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var objectId = reader.ReadBytes(16);
        var objectIdGuid = new Guid(objectId);
        if (rawData.Data.Length == 16)
            return new ObjectId(objectIdGuid, Guid.Empty, Guid.Empty, Guid.Empty);
        
        var birthVolumeId = reader.ReadBytes(16);
        var birthVolumeIdGuid = new Guid(birthVolumeId);
        if (rawData.Data.Length == 32)
            return new ObjectId(objectIdGuid, birthVolumeIdGuid, Guid.Empty, Guid.Empty);
        
        var birthObjectId = reader.ReadBytes(16);
        var birthObjectIdGuid = new Guid(birthObjectId);
        if (rawData.Data.Length == 48)
            return new ObjectId(objectIdGuid, birthVolumeIdGuid, birthObjectIdGuid, Guid.Empty);
        
        var domainId = reader.ReadBytes(16);
        var domainIdGuid = new Guid(domainId);
        return new ObjectId(objectIdGuid, birthVolumeIdGuid, birthObjectIdGuid, domainIdGuid);
    }
}