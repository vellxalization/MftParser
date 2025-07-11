using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

/// <summary>
/// Standard information attribute's data. Always resident and is present in every base record
/// </summary>
/// <param name="FileCreated">The time when the file was created</param>
/// <param name="FileAltered">The time when the file was last changed</param>
/// <param name="MftChanged">The time when the file's MFT record was changed. Not shown by Windows in file's properties</param>
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
/// <param name="SecurityId">Security ID (not Windows SID). Index of a corresponding entry inside $SII and $SDH indices</param>
/// <param name="QuotaCharged">Number of bytes that the file contributes to the user's quota. Zero means quotas are disabled</param>
/// <param name="UpdateSequenceNumber">Last update sequence number of the file. Used by USN journal. Zero means USN journal is disabled</param>
public readonly record struct StandardInformation(FileTime FileCreated, FileTime FileAltered, FileTime MftChanged, FileTime FileRead, 
    FileAttributes DosAttributes, uint MaxVersions, uint VersionNumber, uint ClassId, uint OwnerId, uint SecurityId, 
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
                new FileTime((long)fileRead), (FileAttributes)dosAttributes, maxVersions, versionNumber,
                classId, 0, 0, 0, 0);
        
        var ownerId = reader.ReadUInt32();
        var securityId = reader.ReadUInt32();
        var quotaCharged = reader.ReadUInt64();
        var updateSequenceNumber = reader.ReadUInt64();
        
        return new StandardInformation(new FileTime((long)fileCreated), 
            new FileTime((long)fileAltered), new FileTime((long)mftChanged), 
            new FileTime((long)fileAltered), (FileAttributes)dosAttributes, maxVersions, versionNumber,
            classId, ownerId, securityId, quotaCharged, updateSequenceNumber);
    }
}