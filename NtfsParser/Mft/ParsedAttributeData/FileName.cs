using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

/// <summary>
/// An attribute that stores the file's name. Single file can have multiple FILE_NAMEs because of how hard link are implemented in NTFS
/// </summary>
/// <param name="ParentDirectory">Reference to the directory record where the file is stored</param>
/// <param name="FileCreated">The time when the file was created</param>
/// <param name="FileAltered">The time when the file was last changed.
/// This value is updated only when THIS attribute is updated, meaning there is a high chance that it's out-of-date.
/// Please refer to the STANDARD_INFORMATION attribute to get up-to-date time</param>
/// <param name="MftChanged">The time when the file's MFT record was changed.
/// This value is updated only when this attribute is updated, meaning there is a high chance that it's out-of-date.
/// Please refer to the STANDARD_INFORMATION attribute to get up-to-date time</param>
/// <param name="FileRead">The time when the file was last read.
/// This value is updated only when this attribute is updated, meaning there is a high chance that it's out-of-date.
/// Please refer to the STANDARD_INFORMATION attribute to get up-to-date time</param>
/// <param name="AllocatedSize">Total size of all clusters used by the file in bytes.
/// Because NTFS can only allocate data in clusters, this value is a multiple of the cluster size</param>
/// <param name="ActualSize">Actual size of the data stored in bytes</param>
/// <param name="Flags"></param>
/// <param name="ExtendedData">Used by extended attributes and reparse points*
/// If the file have extended attributes, then this field is equal to the size of the attributes.
/// If the file has reparse point flag, then this value is a reparse tag.
/// * Referring to https://flatcap.github.io/linux-ntfs/ntfs/attributes/file_name.html;
/// However, couldn't confirm this during testing</param>
/// <param name="FilenameLength">Length of the name in Unicode characters</param>
/// <param name="FilenameNamespace">Namespace of the name</param>
/// <param name="Name">File's name</param>
public readonly record struct FileName(FileReference ParentDirectory, FileTime FileCreated, FileTime FileAltered,
    FileTime MftChanged, FileTime FileRead, ulong AllocatedSize, ulong ActualSize, FileAttributes Flags, uint ExtendedData,
    byte FilenameLength, FnNamespace FilenameNamespace, UnicodeName Name)
{
    public static FileName CreateFromRawData(in RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var referenceToParentDirectory = FileReference.Parse(reader.ReadBytes(8));
        var fileCreated = reader.ReadUInt64();
        var fileAltered = reader.ReadUInt64();
        var mftChanged = reader.ReadUInt64();
        var fileRead = reader.ReadUInt64();
        var allocatedFileSize = reader.ReadUInt64();
        var realFileSize = reader.ReadUInt64();
        var flags = reader.ReadUInt32();
        var extendedData = reader.ReadUInt32();
        var filenameLength = reader.ReadByte();
        var filenameNamespace = reader.ReadByte();
        var filename = reader.ReadBytes(filenameLength * 2); // utf-16 encoded name. 2 bytes/char
        
        return new FileName(referenceToParentDirectory, new FileTime((long)fileCreated), 
            new FileTime((long)fileAltered), new FileTime((long)mftChanged), 
            new FileTime((long)fileRead), allocatedFileSize, realFileSize, (FileAttributes)flags, extendedData,
            filenameLength, (FnNamespace)filenameNamespace, new UnicodeName(filename.ToArray()));
    }
}

public enum FnNamespace
{
    /// <summary>
    /// Case-sensitive, all Unicode characters are allowed, except for '/' and NULL
    /// </summary>
    Posix,
    /// <summary>
    /// Subset of the POSIX, case-insensitive, all Unicode characters are allowed, except for special characters
    /// </summary>
    Win32,
    /// <summary>
    /// Only uppercase characters and numbers, up to eight characters for the name, up to three characters for the extension, no special characters
    /// (https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-fscc/18e63b13-ba43-4f5f-a5b7-11e871b71f14) 
    /// </summary>
    Dos,
    /// <summary>
    /// Win32 name that follows DOS rules
    /// </summary>
    DosCompatibleWin32
}