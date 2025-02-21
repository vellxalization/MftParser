using System.Text;
using FileSystemTraverser.MasterFileTable.Header;

namespace FileSystemTraverser.MasterFileTable.AttributeData._FILE_NAME;

public record struct FileName(MftSegmentReference ReferenceToParentDirectory, ulong FileCreation, ulong FileAltered,
    ulong MftChanged, ulong FileRead, ulong AllocatedFileSize, ulong RealFileSize, FileNameFlags Flags, uint EaReparse,
    byte FilenameLength, byte FilenameNamespace, byte[] UnicodeFilename)
{
    public static FileName CreateFromData(ref byte[] data)
    {
        using var reader = new BinaryReader(new MemoryStream(data));
        var referenceToParentDirectory = MftSegmentReference.CreateFromStream(reader);
        var fileCreation = reader.ReadUInt64();
        var fileAltered = reader.ReadUInt64();
        var mftChanged = reader.ReadUInt64();
        var fileRead = reader.ReadUInt64();
        var allocatedFileSize = reader.ReadUInt64();
        var realFileSize = reader.ReadUInt64();
        var flags = reader.ReadUInt32();
        var eaReparse = reader.ReadUInt32();
        var filenameLength = reader.ReadByte();
        var filenameNamespace = reader.ReadByte();
        var filename = reader.ReadBytes(filenameLength * 2); // utf-16 encoded name. 2 bytes/char
        
        return new FileName(referenceToParentDirectory, fileCreation, fileAltered, mftChanged, fileRead,
            allocatedFileSize, realFileSize, (FileNameFlags)flags, eaReparse, filenameLength, filenameNamespace, 
            filename);
    }

    public string GetStringFileName() => Encoding.Unicode.GetString(UnicodeFilename);
}

[Flags]
public enum FileNameFlags : uint
{
    ReadOnly = 0x0001,
    Hidden = 0x0002,
    System = 0x0004,
    Archive = 0x0020,
    Device = 0x0040,
    Temporary = 0x0100,
    Sparse = 0x0200,
    ReparsePoint = 0x0400,
    Compressed = 0x0800,
    Offline = 0x1000,
    NotContentIndexed = 0x2000,
    Encrypted = 0x4000,
    Directory = 0x10000000,
    IndexView = 0x20000000
}