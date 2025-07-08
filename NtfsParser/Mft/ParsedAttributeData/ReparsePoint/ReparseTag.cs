namespace NtfsParser.Mft.ParsedAttributeData.ReparsePoint;

/// <summary>
/// Reparse point's tag. First four bits from the most significant are flags, next twelve - reserved, last sixteen describe the type.
/// Together they can be interpreted as one of the predefined tags
/// </summary>
/// <param name="Value">Raw tag value</param>
public readonly record struct ReparseTag(uint Value)
{
    /// <summary>
    /// Returns first four most significant bits
    /// </summary>
    /// <returns>Tag's flags</returns>
    public ReparseFlags GetFlags() => (ReparseFlags)(Value & FlagsMask);
    private const uint FlagsMask = 0b_1111_0000_00000000_00000000_00000000;

    /// <summary>
    /// Returns sixteen least significant bits
    /// </summary>
    /// <returns></returns>
    public ushort GetRawType() => (ushort)(Value & TypeMask);
    private const ushort TypeMask = ushort.MaxValue;
    
    /// <summary>
    /// Interprets the tag as one of the predefined tags 
    /// </summary>
    /// <returns>Predefined tag</returns>
    public PredefinedTags AsPredefinedTag() => (PredefinedTags)Value;
}

/// <summary>
/// Predefined reparse tags.
/// Taken from https://learn.microsoft.com/en-us/windows/win32/fileio/reparse-point-tags
/// </summary>
public enum PredefinedTags : uint
{
    AfUnix = 0x80000023,
    Appexeclink = 0x8000001B,
    Cloud = 0x9000001A,
    Cloud1 = 0x9000101A,
    Cloud2 = 0x9000201A,
    Cloud3 = 0x9000301A,
    Cloud4 = 0x9000401A,
    Cloud5 = 0x9000501A,
    Cloud6 = 0x9000601A,
    Cloud7 = 0x9000701A,
    Cloud8 = 0x9000801A,
    Cloud9 = 0x9000901A,
    CloudA = 0x9000A01A,
    CloudB = 0x9000B01A,
    CloudC = 0x9000C01A,
    CloudD = 0x9000D01A,
    CloudE = 0x9000E01A,
    CloudF = 0x9000F01A,
    CloudMask = 0xF000,
    Csv = 0x80000009,
    Dedup = 0x80000013,
    Dfs = 0x8000000A,
    Dfsr = 0x80000012,
    FilePlaceholder = 0x80000015,
    GlobalReparse = 0xA0000019,
    Hsm = 0xC0000004,
    Hsm2 = 0x80000006,
    MountPoint = 0xA0000003,
    Nfs = 0x80000014,
    Onedrive = 0x80000021,
    Projfs = 0x9000001C,
    ProjfsTombstone = 0xA0000022,
    Sis = 0x80000007,
    StorageSync = 0x8000001E,
    Symlink = 0xA000000C,
    Unhandled = 0x80000020,
    Wci = 0x80000018,
    Wci1 = 0x90001018,
    WciLink = 0xA0000027,
    WciLink1 = 0xA0001027,
    WciTombstone = 0xA000001F,
    Wim = 0x80000008,
    Wof = 0x80000017,
}

[Flags]
public enum ReparseFlags
{
    /// <summary>
    /// Represents another named entity in the system
    /// </summary>
    IsAlias = 1 << 29,
    /// <summary>
    /// Reserved
    /// </summary>
    IsHighLatency = 1 << 30,
    /// <summary>
    /// Tag is owned by Microsoft
    /// </summary>
    IsMicrosoft = 1 << 31
}