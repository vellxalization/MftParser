namespace NtfsParser.Mft.Attribute;

public record struct MftAttributeHeader(AttributeType Type, uint Size, bool IsNonresident, byte NameSize, 
    ushort NameOffset, AttributeHeaderFlags DataFlags, ushort AttributeId, Resident Resident, Nonresident Nonresident)
{
    public static MftAttributeHeader Parse(ref SpanBinaryReader reader)
    {
        var type = reader.ReadUInt32();
        if (type == 0xffffffff)
        {
            return new MftAttributeHeader(AttributeType.EndOfAttributeList, 0, false, 0, 0, 0, 0, default, default);
        }
        
        var size = reader.ReadUInt32();
        var nonresidentFlag = reader.ReadByte();
        var nameSize = reader.ReadByte();
        var nameOffset = reader.ReadUInt16();
        var dataFlags = reader.ReadUInt16();
        var attributeId = reader.ReadUInt16();
        if (nonresidentFlag == 0)
        {
            var rawResident = reader.ReadBytes(8);
            var resident = Resident.Parse(rawResident);
            return new MftAttributeHeader((AttributeType)type, size, false, nameSize, nameOffset,
                (AttributeHeaderFlags)dataFlags, attributeId, resident, default);
        }

        var rawNonresident = reader.ReadBytes(56); // it can be either 48 or 56 bytes in size. 
        var nonresident = Nonresident.Parse(rawNonresident);
        return new MftAttributeHeader((AttributeType)type, size, true, nameSize, nameOffset, 
            (AttributeHeaderFlags)dataFlags, attributeId, default, nonresident);
    }
}

[Flags]
public enum AttributeHeaderFlags
{
    IsCompressed = 0x0001,
    FlagCompressionMask = 0x00ff,
    IsEncrypted = 0x4000,
    IsSparse = 0x8000
}

public enum AttributeType
{
    StandardInformation = 0x10,
    AttributeList = 0x20,
    FileName = 0x30,
    ObjectId = 0x40,
    SecurityDescriptor = 0x50,
    VolumeName = 0x60,
    VolumeInformation = 0x70,
    Data = 0x80,
    IndexRoot = 0x90,
    IndexAllocation = 0xA0,
    Bitmap = 0xB0,
    ReparsePoint = 0xC0,
    EaInformation = 0xD0,
    ExtendedAttribute = 0xE0,
    LoggedUtilityStream = 0x100,
    EndOfAttributeList = 0xFF
}