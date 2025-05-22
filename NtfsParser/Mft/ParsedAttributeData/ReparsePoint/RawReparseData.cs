namespace NtfsParser.Mft.ParsedAttributeData.ReparsePoint;

public readonly record struct RawReparseData(byte[] Data)
{
    public MountPoint ToMountPoint() => MountPoint.CreateFromRawData(this);
    public SymbolicLink ToSymbolicLink() => SymbolicLink.CreateFromRawData(this);
}