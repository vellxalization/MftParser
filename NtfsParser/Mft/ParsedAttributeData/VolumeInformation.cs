using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

/// <summary>
/// An attribute that contains information about volume. Used only by $Volume meta file
/// </summary>
/// <param name="MajorVersion">Major version of NTFS. Starting from Windows XP, we're at the version 3.1, so this field is set to 3</param>
/// <param name="MinorVersion">Minor version of NTFS. Starting from Windows XP, we're at the version 3.1, so this field is set to 1</param>
/// <param name="Flags">Volume's flags</param>
public readonly record struct VolumeInformation(byte MajorVersion, byte MinorVersion, VolumeInformationFlags Flags)
{
    public static VolumeInformation CreateFromRawData(in RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        reader.Skip(8); // supposedly zeroes
        var majorVer = reader.ReadByte();
        var minorVer = reader.ReadByte();
        var flags = reader.ReadUInt16();

        return new VolumeInformation(majorVer, minorVer, (VolumeInformationFlags)flags);
    }
}

[Flags]
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