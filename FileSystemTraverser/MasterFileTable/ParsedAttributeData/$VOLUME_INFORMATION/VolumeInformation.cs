namespace FileSystemTraverser.MasterFileTable.ParsedAttributeData._VOLUME_INFORMATION;

public record struct VolumeInformation(byte MajorVersion, byte MinorVersion, VolumeInformationFlags Flags)
{
    public static VolumeInformation CreateFromData(byte[] data)
    {
        using var reader = new BinaryReader(new MemoryStream(data));
        reader.BaseStream.Position += 8; // supposedly zeroes
        var majorVer = reader.ReadByte();
        var minorVer = reader.ReadByte();
        var flags = reader.ReadUInt16();

        return new VolumeInformation(majorVer, minorVer, (VolumeInformationFlags)flags);
    }
}

public enum VolumeInformationFlags : ushort
{
    Dirty = 0x0001,
    ResizeLogFile = 0x0002,
    UpgradeOnMount = 0x0004,
    MountedOnNt4 = 0x0008,
    DeleteUsnUnderway = 0x0010,
    RepairObjectIds = 0x0020,
    ModifiedByChkdsk = 0x8000
}