namespace NtfsParser.Mft.Attribute;

public record struct MftAttribute(MftAttributeHeader Header, UnicodeName Name, byte[] Value)
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
        var header = MftAttributeHeader.Parse(ref reader);
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
    
    public RawAttributeData GetAttributeData(VolumeReader volumeReader)
    {
        var strategy = PickBestStrategy();
        return strategy.GetDataFromDataRuns(volumeReader, ref this);
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