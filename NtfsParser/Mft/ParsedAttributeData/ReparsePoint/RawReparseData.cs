namespace NtfsParser.Mft.ParsedAttributeData.ReparsePoint;

/// <summary>
/// Reparse data. Use object's methods to convert data to one of the predefined structures
/// </summary>
/// <param name="Data">Raw data as a byte array</param>
public readonly record struct RawReparseData(byte[] Data)
{
    public MountPoint ToMountPoint() => MountPoint.CreateFromRawData(this);
    public SymbolicLink ToSymbolicLink() => SymbolicLink.CreateFromRawData(this);
}