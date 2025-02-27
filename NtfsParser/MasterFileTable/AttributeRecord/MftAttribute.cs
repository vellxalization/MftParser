namespace NtfsParser.MasterFileTable.AttributeRecord;

public record struct MftAttribute(MftAttributeHeader Header, byte[] Name, byte[] Value)
{
    public static MftAttribute Parse(ReadOnlySpan<byte> rawAttribute)
    {
        var reader = new SpanBinaryReader(rawAttribute);
        var header = MftAttributeHeader.Parse(ref reader);
        if (header.Type == AttributeType.EndOfAttributeList)
        {
            return new MftAttribute(header, [], []);
        }

        ReadOnlySpan<byte> name = [];
        if (header.NameSize != 0)
        {
            reader.Position = header.NameOffset; // set position to the name field
            name = reader.ReadBytes(header.NameSize * 2); // utf-16 encoded name requires 2 bytes per char
        }
        
        ReadOnlySpan<byte> data;
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

    private static ReadOnlySpan<byte> ReadDataRun(SpanBinaryReader reader)
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

    public RawAttributeData GetAttributeData(BinaryReader reader, int clusterByteSize)
    {
        return Header.IsNonresident 
            ? new RawAttributeData(GetDataFromDataRun(reader, clusterByteSize)) 
            : new RawAttributeData(Value);
    }

    public ulong GetActualDataSize() =>
        Header.IsNonresident ? Header.Nonresident.DataSize : Header.Resident.Size;
    
    private byte[] GetDataFromDataRun(BinaryReader reader, int clusterByteSize)
    {
        var start = reader.BaseStream.Position;
        
        var dataRuns = CreateDataRunsFromValue();
        var bytes = new byte[dataRuns.Sum(run => (int)run.Length) * clusterByteSize];
        int offset = 0;
        foreach (var run in dataRuns)
        {
            reader.BaseStream.Position = (long)run.Offset * clusterByteSize;
            var length = (int)run.Length * clusterByteSize;
            reader.BaseStream.ReadExactly(bytes, offset, length);
            offset += length;
        }
        reader.BaseStream.Position = start;
        return bytes;
    }

    private DataRun[] CreateDataRunsFromValue()
    {
        var i = 0;
        List<DataRun> runs = new();
        while (i < Value.Length)
        {
            var header = Value[i++];
            var lengthBit = header & 0x0F;
            var offsetBit = (header & 0xF0) >> 4;
            UInt128 length = 0;
            for (int j = 0; j < lengthBit; ++j)
            {
                length |= (UInt128)(Value[j + i] << (8 * j));
            }
            
            i += lengthBit;
            UInt128 offset = 0;
            for (int j = 0; j < offsetBit; ++j)
            {
                offset |= (UInt128)(Value[j + i] << (8 * j));
            }
            
            i += offsetBit;
            runs.Add(new DataRun(header, length, offset));
        }
        
        return runs.ToArray();
    }
}