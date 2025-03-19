namespace NtfsParser.Mft.ParsedAttributeData.ReparsePoint;

public record struct ReparseTag(uint Value)
{
    public ReparseFlags GetFlags() => (ReparseFlags)(Value & 0b_1111_0000_00000000_00000000_00000000);
    public PredefinedTags AsPredefinedTag() => (PredefinedTags)Value;
}

public enum PredefinedTags : uint
{
    DriveExtender = 0x80000005,
    Hsm2 = 0x80000006,
    Sis = 0x80000007,
    Wim = 0x80000008,
    Csv = 0x80000009,
    Dfs = 0x8000000A,
    FilterManager = 0x8000000B,
    Dfsr = 0x80000012,
    Dedup = 0x80000013,
    Nfs = 0x80000014,
    FilePlaceholder = 0x80000015,
    Dfm = 0x80000016,
    Wof = 0x80000017,
    Wci = 0x80000018,
    Appexeclink = 0x8000001B,
    StorageSync = 0x8000001E,
    Unhandled = 0x80000020,
    Onedrive = 0x80000021,
    AfUnix = 0x80000023,
    LxFifo = 0x80000024,
    LxChr = 0x80000025,
    LxBlk = 0x80000036,
    Projfs = 0x9000001C,
    Wci1 = 0x90001018,
    Cloud1 = 0x9000101A,
    Cloud2 = 0x9000201A,
    Cloud3 = 0x9000301A,
    Cloud4 = 0x9000401A,
    Cloud5 = 0x9000501A,
    Cloud6 = 0x9000601A,
    Cloud7 = 0x9000701A,
    Cloud8 = 0x9000801A,
    Cloud9 = 0x9000901A,
    CloudA = 0x9000a01A,
    CloudB = 0x9000b01A,
    CloudC = 0x9000c01A,
    CloudD = 0x9000d01A,
    CloudE = 0x9000e01A,
    CloudF = 0x9000f01A,
    MountPoint = 0xA0000003,
    Symlink = 0xA000000C,
    IisCache = 0xA0000010,
    GlobalReparse = 0xA0000019,
    Cloud = 0xA000001A,
    LxSymlink = 0xA000001D,
    WciTombstone = 0xA000001f,
    ProjfsTombstone = 0xA0000022,
    WciLink = 0xA0000027,
    WciLink1 = 0xA0001027,
    Hsm = 0xC0000004,
    Appxstrm = 0xC0000014,
}

[Flags]
public enum ReparseFlags
{
    IsAlias = 1 << 29,
    IsHighLatency = 1 << 30,
    IsMicrosoft = 1 << 31
}