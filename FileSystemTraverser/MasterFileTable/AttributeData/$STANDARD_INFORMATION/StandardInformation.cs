namespace FileSystemTraverser.MasterFileTable.AttributeData._STANDARD_INFORMATION;

public record struct StandardInformation(ulong FileCreation, ulong FileAltered, ulong MftChanged, ulong FileRead, 
    DosPermissions DosPermissions, uint MaxVersions, uint VersionNumber, uint ClassId, uint OwnerId, uint SecurityId, 
    ulong QuotaChanged, ulong UpdateSequenceNumber)
{
    public static StandardInformation CreateFromData(ref byte[] data, int size)
    {
        if (size is not (48 or 72))
        { throw new ArgumentException("Expected a size of 48 or 72 bytes"); }
        
        using var reader = new BinaryReader(new MemoryStream(data));
        var fileCreation = reader.ReadUInt64();
        var fileAltered = reader.ReadUInt64();
        var mftChanged = reader.ReadUInt64();
        var fileRead = reader.ReadUInt64();
        var dosPermissions = GetEnumPerms(reader.ReadUInt32());
        var maxVersions = reader.ReadUInt32();
        var versionNumber = reader.ReadUInt32();
        var classId = reader.ReadUInt32();
        if (size == 48)
        {
            return new StandardInformation(fileCreation, fileAltered, mftChanged, fileRead, dosPermissions, maxVersions,
                versionNumber, classId, 0, 0, 0, 0);
        }
        
        var ownerId = reader.ReadUInt32();
        var securityId = reader.ReadUInt32();
        var quotaChanged = reader.ReadUInt64();
        var updateSequenceNumber = reader.ReadUInt64();
        
        return new StandardInformation(fileCreation, fileAltered, mftChanged, fileRead, dosPermissions, maxVersions, versionNumber, classId, ownerId, securityId, quotaChanged, updateSequenceNumber);
    }

    private static DosPermissions GetEnumPerms(uint value)
    {
        return value switch
        {
            0x0001 => DosPermissions.ReadOnly,
            0x0002 => DosPermissions.Hidden,
            0x0004 => DosPermissions.System,
            0x0020 => DosPermissions.Archive,
            0x0040 => DosPermissions.Device,
            0x0080 => DosPermissions.Normal,
            0x0100 => DosPermissions.Temporary,
            0x0200 => DosPermissions.SparseFile,
            0x0400 => DosPermissions.ReparsePoint,
            0x0800 => DosPermissions.Compressed,
            0x01000 => DosPermissions.Offline,
            0x02000 => DosPermissions.NotContentIndexed,
            0x04000 => DosPermissions.Encrypted,
            _ => throw new AggregateException($"Unknown DOS flag: {value}")
        };
    }
}

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