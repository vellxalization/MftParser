namespace NtfsParser.Mft.Attribute;

/// <summary>
/// Structure that represents a single record's attribute
/// </summary>
/// <param name="Header">Attribute's header</param>
/// <param name="Name">Attribute's name. For most of the attributes will be empty. Index ones will have "$I30" name</param>
/// <param name="Value">Attribute's value. For resident attributes, this is the content; for nonresident - data runs</param>
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
            reader.Position = header.Resident.ContentOffset;
            data = reader.ReadBytes((int)header.Resident.ContentSize);
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
    
    /// <summary>
    /// Gets the data of the attribute.
    /// Will handle the sparse and compressed attributes by adding zeroes and uncompressing them, respectively
    /// </summary>
    /// <param name="volumeReader">An instance of volume data reader to read data from the disk in case the attribute is nonresident</param>
    /// <returns>Ready to consume data</returns>
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

/// <summary>
/// Attribute's header
/// </summary>
/// <param name="Type">Type of the attribute, as defined in $AttrDef meta file</param>
/// <param name="Size">Size of the attribute, including the header and content</param>
/// <param name="IsNonresident">Is the attribute nonresident</param>
/// <param name="NameSize">Size of the attribute's name in Unicode characters</param>
/// <param name="NameOffset">Offset to the start of the attribute's name</param>
/// <param name="DataFlags">Attribute's flags</param>
/// <param name="AttributeId">Attribute's ID</param>
/// <param name="Resident">Contains data about resident content of the attribute. If the attribute is nonrsident, set to default</param>
/// <param name="Nonresident">Contains data about nonresident content of the attribute. If the attribute is resident, set to default</param>
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

/// <summary>
/// Attribute's flags
/// </summary>
[Flags]
public enum AttributeHeaderFlags
{
    /// <summary>
    /// Set if the attribute's data is compressed. Should be only used in nonresident $Data attributes
    /// </summary>
    IsCompressed = 0x0001,
    /// <summary>
    /// Set if the attribute's data is encrypted
    /// </summary>
    IsEncrypted = 0x4000,
    /// <summary>
    /// Set if the attribute's data contains sparse blocks
    /// </summary>
    IsSparse = 0x8000
}

/// <summary>
/// Attribute's type as defined in $AttrDef meta file. Currently, we use NTFS 3.1 types
/// </summary>
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