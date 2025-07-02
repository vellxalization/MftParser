using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

/// <summary>
/// Standard information attribute's data
/// </summary>
/// <param name="FileCreated">The time when the file was created</param>
/// <param name="FileAltered">The time when the file was last changed</param>
/// <param name="MftChanged">The time when the file's MFT record was changed</param>
/// <param name="FileRead">The time when the file was last read</param>
/// <param name="DosAttributes">DOS attributes flags</param>
/// <param name="MaxVersions">Maximum number of versions.
/// This is supposedly a remnant of a file versioning system that wasn't implemented.
/// All the files during testing had this field set to zero, no exceptions</param>
/// <param name="VersionNumber">Current version of a file
/// This is supposedly a remnant of a file versioning system that wasn't implemented.
/// Some of the files during testing had this field set to some values, even though max version is always set to 0</param>
/// <param name="ClassId">Class ID for "bidirectional ID index" (https://flatcap.github.io/linux-ntfs/ntfs/attributes/standard_information.html).
/// Every tested record had a value set to 0</param>
/// <param name="OwnerId">ID of the owner. Used by quota's $O and $Q indices. Zero means quotas are disabled</param>
/// <param name="SecurityId">Security ID (not Windows SID). Used by security's $SDH and $SII indices</param>
/// <param name="QuotaCharged">Number of bytes that a file contributes to the user's quota. Zero means quotas are disabled</param>
/// <param name="UpdateSequenceNumber">Last update sequence number of the file. Used by USN journal. Zero means USN journal is disabled</param>
public readonly record struct StandardInformation(FileTime FileCreated, FileTime FileAltered, FileTime MftChanged, FileTime FileRead, 
    DosAttributes DosAttributes, uint MaxVersions, uint VersionNumber, uint ClassId, uint OwnerId, uint SecurityId, 
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
        var dosAttributes = reader.ReadUInt32();
        var maxVersions = reader.ReadUInt32();
        var versionNumber = reader.ReadUInt32();
        var classId = reader.ReadUInt32();
        if (rawData.Data.Length <= 48)
            return new StandardInformation(new FileTime((long)fileCreated), 
                new FileTime((long)fileAltered), new FileTime((long)mftChanged), 
                new FileTime((long)fileRead), (DosAttributes)dosAttributes, maxVersions, versionNumber,
                classId, 0, 0, 0, 0);
        
        var ownerId = reader.ReadUInt32();
        var securityId = reader.ReadUInt32();
        var quotaCharged = reader.ReadUInt64();
        var updateSequenceNumber = reader.ReadUInt64();
        
        return new StandardInformation(new FileTime((long)fileCreated), 
            new FileTime((long)fileAltered), new FileTime((long)mftChanged), 
            new FileTime((long)fileAltered), (DosAttributes)dosAttributes, maxVersions, versionNumber,
            classId, ownerId, securityId, quotaCharged, updateSequenceNumber);
    }
}

[Flags]
public enum DosAttributes : uint
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
    Directory = 0x10000000,
    IndexView = 0x20000000
}