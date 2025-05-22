using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

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