namespace NtfsParser.Mft.Attribute;

public record struct MftAttribute(MftAttributeHeader Header, byte[] Name, byte[] Value)
{
    public static MftAttribute Parse(Span<byte> rawAttribute)
    {
        var reader = new SpanBinaryReader(rawAttribute);
        var header = MftAttributeHeader.Parse(ref reader);
        if (header.Type == AttributeType.EndOfAttributeList)
        {
            return new MftAttribute(header, [], []);
        }
        Span<byte> name = Span<byte>.Empty;
        if (header.NameSize != 0)
        {
            reader.Position = header.NameOffset; // set position to the name field
            name = reader.ReadBytes(header.NameSize * 2); // utf-16 encoded name requires 2 bytes per char
        }
        
        Span<byte> data;
        if (header.IsNonresident)
        {
            reader.Position = header.Nonresident.DataRunsOffset;
            data = ReadDataRun(reader);
            reader.Position += data.Length;
        }
        else
        {
            reader.Position = header.Resident.Offset;
            data = reader.ReadBytes((int)header.Resident.Size);
        }
        
        return new MftAttribute(header, name.ToArray(), data.ToArray());
    }

    private static Span<byte> ReadDataRun(SpanBinaryReader reader)
    {
        var beforeDataRun = reader.Position;
        var header = reader.ReadByte();
        while (header != 0x00)
        {
            var length = header & 0x0F;
            var offset = (header & 0xF0) >> 4;
            reader.Position += length + offset;
            header = reader.ReadByte();
        }

        var dataRunSize = reader.Position - beforeDataRun - 1; // subtract 1 to compensate 0x00 header
        reader.Position = beforeDataRun;
        
        return reader.ReadBytes(dataRunSize);
    }

    public RawAttributeData GetAttributeData(VolumeReader volumeReader)
    {
        return Header.IsNonresident 
            ? new RawAttributeData(GetDataFromDataRun(volumeReader)) 
            : new RawAttributeData(Value);
    }
    
    private byte[] GetDataFromDataRun(VolumeReader volumeReader)
    {
        if (Header.Nonresident.ValidDataSize == 0)
        {
            return [];
        }
        
        var start = volumeReader.GetPosition();
        var dataRuns = DataRun.ParseDataRuns(Value);
        var dataBytes = new byte[Header.Nonresident.ValidDataSize];
        long clusterOffset = 0;
        var copyOffset = 0;
        for (var i = 0; i < dataRuns.Length; ++i)
        {
            var dataRun = dataRuns[i];
            if (dataRun.Offset == 0)
            {
                copyOffset += (int)dataRun.Length * volumeReader.ClusterByteSize;
                continue;
            }
            
            clusterOffset += dataRun.Offset;
            volumeReader.SetPosition(clusterOffset, SetStrategy.Cluster);
            var buffer = new byte[(int)dataRun.Length * volumeReader.ClusterByteSize];
            volumeReader.ReadBytes(buffer, 0, buffer.Length);
            var copyLength = i < dataRuns.Length - 1 ? buffer.Length : dataBytes.Length - copyOffset;
            Array.Copy(buffer, 0, dataBytes, copyOffset, copyLength);
            copyOffset += copyLength;
        } 
        
        volumeReader.SetPosition(start, SetStrategy.Byte);
        return dataBytes;
    }
}