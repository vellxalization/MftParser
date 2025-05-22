using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

public readonly record struct StandardInformation(FileTime FileCreated, FileTime FileAltered, FileTime MftChanged, FileTime FileRead, 
    DosPermissions DosPermissions, uint MaxVersions, uint VersionNumber, uint ClassId, uint OwnerId, uint SecurityId, 
    ulong QuotaCharged, ulong UpdateSequenceNumber)
{
    public static StandardInformation CreateFromRawData(in RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var fileCreated = reader.ReadUInt64();
        var fileAltered = reader.ReadUInt64();
        var mftChanged = reader.ReadUInt64();
        var fileRead = reader.ReadUInt64();
        var dosPermissions = reader.ReadUInt32();
        var maxVersions = reader.ReadUInt32();
        var versionNumber = reader.ReadUInt32();
        var classId = reader.ReadUInt32();
        if (rawData.Data.Length <= 48)
            return new StandardInformation(new FileTime((long)fileCreated), 
                new FileTime((long)fileAltered), new FileTime((long)mftChanged), 
                new FileTime((long)fileRead), (DosPermissions)dosPermissions, maxVersions, versionNumber,
                classId, 0, 0, 0, 0);
        
        var ownerId = reader.ReadUInt32();
        var securityId = reader.ReadUInt32();
        var quotaCharged = reader.ReadUInt64();
        var updateSequenceNumber = reader.ReadUInt64();
        
        return new StandardInformation(new FileTime((long)fileCreated), 
            new FileTime((long)fileAltered), new FileTime((long)mftChanged), 
            new FileTime((long)fileAltered), (DosPermissions)dosPermissions, maxVersions, versionNumber,
            classId, ownerId, securityId, quotaCharged, updateSequenceNumber);
    }
}

[Flags]
public enum DosPermissions : uint
{
    ReadOnly = 0x0001,
    Hidden = 0x0002,
    System = 0x0004,
    Archive = 0x0020,
    Device = 0x0040,
    Normal = 0x0080,
    Temporary = 0x00100,
    SparseFile = 0x00200,
    ReparsePoint = 0x00400,
    Compressed = 0x00800,
    Offline = 0x01000,
    NotContentIndexed = 0x02000,
    Encrypted = 0x04000,
}