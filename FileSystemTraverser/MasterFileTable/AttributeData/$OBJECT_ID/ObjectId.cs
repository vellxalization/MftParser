using System.Runtime.InteropServices;

namespace FileSystemTraverser.MasterFileTable.AttributeData._OBJECT_ID;

public record struct ObjectId(Guid Id, Guid BirthVolumeId, Guid BirthObjectId, Guid DomainId)
{
    public static ObjectId CreateFromData(ref byte[] data)
    {
        using var reader = new BinaryReader(new MemoryStream(data));
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