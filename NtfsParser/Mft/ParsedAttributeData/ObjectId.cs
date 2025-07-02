using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

/// <summary>
/// An attribute that stores MFT record's unique identifier. Every record should have one.
/// If it's absent, Windows will create one next time user opens the file
/// </summary>
/// <param name="Id">Record's ID</param>
/// <param name="BirthVolumeId">ID of the volume where the file was created. Unused or very rare, none of the records had one during tests</param>
/// <param name="BirthObjectId">Original ID of the file in case it was overwritten. Unused or very rare, none of the records had one during tests</param>
/// <param name="DomainId">ID of the domain where the file was created. Unused or very rare, none of the records had one during tests</param>
public readonly record struct ObjectId(Guid Id, Guid BirthVolumeId, Guid BirthObjectId, Guid DomainId)
{
    public static ObjectId CreateFromRawData(in RawAttributeData rawData)
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