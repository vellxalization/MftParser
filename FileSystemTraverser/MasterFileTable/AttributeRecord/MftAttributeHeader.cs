namespace FileSystemTraverser.MasterFileTable.AttributeRecord;

public record struct MftAttributeHeader(AttributeType Type, uint Size, byte NonresidentFlag, byte NameSize, ushort NameOffset,
    ushort DataFlags, ushort AttributeId, Resident Resident, Nonresident Nonresident)
{
    public static MftAttributeHeader CreateFromStream(BinaryReader reader)
    {
        var type = reader.ReadUInt32();
        if (type == 0xffffffff)
        {
            return new MftAttributeHeader(AttributeType.Unknown, 0, 0, 0, 0, 0, 0, default, default);
        }
        
        var size = reader.ReadUInt32();
        var nonresidentFlag = reader.ReadByte();
        var nameSize = reader.ReadByte();
        var nameOffset = reader.ReadUInt16();
        var dataFlags = reader.ReadUInt16();
        var attributeId = reader.ReadUInt16();
        if (nonresidentFlag == 0)
        {
            var resident = Resident.CreateFromStream(reader);
            return new MftAttributeHeader((AttributeType)type, size, nonresidentFlag, nameSize, nameOffset, dataFlags, attributeId, resident, default);
        }

        var nonresident = Nonresident.CreateFromStream(reader);
        return new MftAttributeHeader((AttributeType)type, size, nonresidentFlag, nameSize, nameOffset, dataFlags, attributeId, default, nonresident);
    }
}

public enum AttributeType
{
    Unknown = 0,
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
    Ea = 0xE0,
    LoggedUtilityStream = 0x100
}