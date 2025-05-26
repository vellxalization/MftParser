namespace NtfsParser.Mft.Attribute;

public readonly record struct MftAttribute(AttributeHeader Header, UnicodeName Name, byte[] Value)
{
    public static MftAttribute[] ParseAttributes(Span<byte> rawAttributes)
    {
        var attributes = new List<MftAttribute>();
        var attribute = Parse(rawAttributes);
        while (attribute.Header.Type != AttributeType.EndOfAttributeList)
        {
            attributes.Add(attribute);
            var splitPoint = (int)attribute.Header.Size;
            rawAttributes = rawAttributes.Slice(splitPoint);
            // ^ using this to avoid headache of constantly caching ^
            // starting position of the reader
            attribute = Parse(rawAttributes);
        }
        
        return attributes.ToArray();
    }
    
    private static MftAttribute Parse(Span<byte> rawAttribute)
    {
        var reader = new SpanBinaryReader(rawAttribute);
        var header = AttributeHeader.Parse(ref reader);
        if (header.Type == AttributeType.EndOfAttributeList)
            return new MftAttribute(header, UnicodeName.Empty, []);
        
        var name = Span<byte>.Empty;
        if (header.NameSize != 0)
        {
            reader.Position = header.NameOffset;
            name = reader.ReadBytes(header.NameSize * 2); // utf-16 encoded name requires 2 bytes per char
        }
        
        Span<byte> data;
        if (header.IsNonresident)
        {
            reader.Position = header.Nonresident.DataRunsOffset;
            var dataRunsLength = GetDataRunsLength(reader);
            data = reader.ReadBytes(dataRunsLength);
        }
        else
        {
            reader.Position = header.Resident.Offset;
            data = reader.ReadBytes((int)header.Resident.Size);
        }
        
        return new MftAttribute(header, new UnicodeName(name.ToArray()), data.ToArray());
    }
    
    private static int GetDataRunsLength(SpanBinaryReader reader)
    {
        var startIndex = reader.Position;
        var header = reader.ReadByte();
        while (header != 0x00)
        {
            var lengthNibble = header & 0x0F;
            var offsetNibble = (header & 0xF0) >> 4;
            reader.Position += lengthNibble + offsetNibble;
            header = reader.ReadByte();
        }
        // subtract 1 to compensate end of the attribute list marker
        return reader.Position - 1 - startIndex;
    }
    
    public RawAttributeData GetAttributeData(VolumeDataReader volumeReader)
    {
        var strategy = PickBestStrategy();
        return strategy.GetDataFromDataRuns(volumeReader, in this);
    }

    private IDataReadStrategy PickBestStrategy()
    {
        if (!Header.IsNonresident)
            return new ResidentStrategy();

        if ((Header.DataFlags & AttributeHeaderFlags.IsCompressed) != 0)
            return new NonresidentCompressedStrategy();

        if ((Header.DataFlags & AttributeHeaderFlags.IsSparse) != 0)
            return new NonresidentSparseStrategy();

        return new NonresidentNoSparseStrategy();
    }
}

public readonly record struct AttributeHeader(AttributeType Type, uint Size, bool IsNonresident, byte NameSize,
    ushort NameOffset, AttributeHeaderFlags DataFlags, ushort AttributeId, Resident Resident, Nonresident Nonresident)
{
    public static AttributeHeader Parse(ref SpanBinaryReader reader)
    {
        var type = reader.ReadUInt32();
        if (type == 0xffffffff)
            return new AttributeHeader(AttributeType.EndOfAttributeList, 0, false, 0, 0, 0, 0, default, default);

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
            return new AttributeHeader((AttributeType)type, size, false, nameSize, nameOffset,
                (AttributeHeaderFlags)dataFlags, attributeId, resident, default);
        }

        var rawNonresident = reader.ReadBytes(56);
        var nonresident = Nonresident.Parse(rawNonresident);
        return new AttributeHeader((AttributeType)type, size, true, nameSize, nameOffset,
            (AttributeHeaderFlags)dataFlags, attributeId, default, nonresident);
    }
}

[Flags]
public enum AttributeHeaderFlags
{
    IsCompressed = 0x0001,
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