namespace NtfsParser.Mft.ParsedAttributeData;

/// <summary>
/// Files and folders attributes. Taken from: https://learn.microsoft.com/en-us/windows/win32/fileio/file-attribute-constants
/// </summary>
[Flags]
public enum FileAttributes : uint
{
    /// <summary>
    /// File or directory is read-only
    /// </summary>
    Readonly = 0x00000001,
    /// <summary>
    /// A hidden file or directory. Do not appear in an ordinary directory listing
    /// (e.g. via file explorer by default or via "dir" command without extra arguments)
    /// </summary>
    Hidden = 0x00000002,
    /// <summary>
    /// A file or directory used by the operating system
    /// </summary>
    System = 0x00000004,
    /// <summary>
    /// Item is a directory
    /// </summary>
    Directory = 0x00000010,
    /// <summary>
    /// A file or folder that is an archive.
    /// This flag is used to mark files that have been changed since the last backup or that need to be deleted
    /// </summary>
    Archive = 0x00000020,
    /// <summary>
    /// A file is an interface for a hardware
    /// </summary>
    Device = 0x00000040,
    /// <summary>
    /// A file without any other attributes. Must be ignored when the other attributes are present
    /// </summary>
    Normal = 0x00000080,
    /// <summary>
    /// A file that is being used for temporary storage
    /// </summary>
    Temporary = 0x00000100,
    /// <summary>
    /// A file is a sparse file
    /// </summary>
    SparseFile = 0x00000200,
    /// <summary>
    /// A file or directory that has a reparse point
    /// </summary>
    ReparsePoint = 0x00000400,
    /// <summary>
    /// A file or directory that is compressed
    /// </summary>
    Compressed = 0x00000800,
    /// <summary>
    /// File's data is not available immediately. Attribute indicates that the data is physically moved to offline storage.
    /// Used by Remote Storage
    /// </summary>
    Offline = 0x00001000,
    /// <summary>
    /// A file or directory that is not indexed by the content indexing service
    /// </summary>
    NotContentIndexed = 0x00002000,
    /// <summary>
    /// A file or directory that is encrypted
    /// </summary>
    Encrypted = 0x00004000,
    /// <summary>
    /// A directory or user data stream is configured with integrity. Not included in an ordinary directory listing.
    /// ReFS exclusive
    /// </summary>
    IntegrityStream = 0x00008000,
    Virtual = 0x00010000,
    /// <summary>
    /// A file or directory that is excluded from the data integrity scan. ReFS exclusive
    /// </summary>
    NoScrubData = 0x00020000,
    /// <summary>
    /// Microsoft defines two flags with the same value. Depending on the context:
    /// <br/> * If obtained as a part of FILE_DIRECTORY_INFORMATION, FILE_BOTH_DIR_INFORMATION etc.:
    /// a file or directory has ho physical representation on the local system; the item is virtual.
    /// <br/> * Otherwise: a file or directory with extended attributes
    /// </summary>
    EaOrRecallOnOpen = 0x00040000,
    /// <summary>
    /// A file or directory that should be kept fully present locally even when not accessed actively.
    /// Hierarchical storage management exclusive
    /// </summary>
    Pinned = 0x00080000,
    /// <summary>
    /// A file or directory that should NOT be kept fully present locally except when being accessed actively.
    /// Hierarchical storage management exclusive
    /// </summary>
    Unpinned = 0x00100000,
    // RecallOnOpen = 0x00040000,
    /// <summary>
    /// A file or directory is not fully present locally. E.g. only part of the file's data is stored locally
    /// or some of the files in a folder is stored remotely 
    /// </summary>
    RecallOnDataAccess = 0x00400000,
    /// <summary>
    /// Same as the <see cref="Directory"/>. Primarily used by FILE_NAME attributes instead of the 0x00000010
    /// </summary>
    DirectoryAlt = 0x10000000,
    /// <summary>
    /// Item is a part of an index other than $I30
    /// </summary>
    IndexView = 0x20000000,
}