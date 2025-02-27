using NtfsParser.MasterFileTable.AttributeRecord;

namespace NtfsParser.MasterFileTable.ParsedAttributeData;

// TODO: out of all of the attributes I couldn't find any MFT record with this attribute and properly test it

public record struct ReparsePoint(ReparseFlags ReparseType, ushort DataSize, ushort TargetNameOffset, ushort TargetNameLength,
    ushort PrintNameOffset, ushort PrintNameLength, byte[] TargetName, byte[] PrintName)
{
    public static ReparsePoint CreateFromRawData(RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var reparseTypeFlags = reader.ReadUInt32();
        var reparseDataSize = reader.ReadUInt16();
        reader.Skip(2); // unused
        var targetNameOffset = reader.ReadUInt16(); // offset is relative to the 16th byte
        var targetNameLength = reader.ReadUInt16();
        var printNameOffset = reader.ReadUInt16(); // offset is relative to the 16th byte
        var printNameLength = reader.ReadUInt16();
        reader.Position = 16 + targetNameOffset;
        var targetName = reader.ReadBytes(targetNameLength);
        reader.Position = 16 + printNameOffset;
        var printName = reader.ReadBytes(printNameLength);

        return new ReparsePoint((ReparseFlags)reparseTypeFlags, reparseDataSize, targetNameOffset, targetNameLength, 
            printNameOffset, printNameLength, targetName.ToArray(), printName.ToArray());
    }
}

[Flags]
public enum ReparseFlags : uint
{
    IsAlias = 0x20000000,
    IsHighLatency = 0x40000000,
    IsMicrosoft = 0x80000000,
    Nss = 0x68000005,
    NssRecover = 0x68000006,
    Sis = 0x68000007,
    Dfs = 0x68000008,
    MountPoint = 0x88000003,
    Hsm = 0xA8000004,
    SymbolicLink = 0xE8000000
}