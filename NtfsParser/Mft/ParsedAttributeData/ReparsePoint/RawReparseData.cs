namespace NtfsParser.Mft.ParsedAttributeData.ReparsePoint;

public record struct RawReparseData(byte[] Data)
{
    public MountPoint ToMountPoint() => MountPoint.CreateFromRawData(this);
    public SymbolicLink ToSymbolicLink() => SymbolicLink.CreateFromRawData(this);
}