namespace FileSystemTraverser.MasterFileTable.AttributeRecord;

public record struct MftAttribute(MftAttributeHeader Header, byte[] Name, byte[] Value)
{
    public static MftAttribute CreateFromStream(BinaryReader reader)
    {
        var startPos = reader.BaseStream.Position;
        var header = MftAttributeHeader.CreateFromStream(reader);
        if (header.Type == AttributeType.Unknown)
        {
            return new MftAttribute(header, [], []);
        }
        
        reader.BaseStream.Position = startPos + header.NameOffset; // set position to the name field
        var name = reader.ReadBytes(header.NameSize * 2); // utf-16 encoded name requires 2 bytes per char
        byte[] data;
        if (header.NonresidentFlag != 0)
        {
            var beforeDataRun = reader.BaseStream.Position;
            reader.BaseStream.Position = startPos + header.Nonresident.DataRunsOffset;
            data = ReadDataRun(reader);
            reader.BaseStream.Position = beforeDataRun;
        }
        else
        {
            data = reader.ReadBytes((int)header.Resident.Size);
        }
        // var data = header.NonresidentFlag != 0 ? ReadDataRun(reader) : reader.ReadBytes((int)header.Resident.Size);
        var padding = reader.ReadBytes((int)(header.Size - (reader.BaseStream.Position - startPos)));
        return new MftAttribute(header, name, data);
    }

    private static byte[] ReadDataRun(BinaryReader reader)
    {
        var startPos = reader.BaseStream.Position;
        var header = reader.ReadByte();
        while (header != 0x00)
        {
            var length = header & 0x0F;
            var offset = (header & 0xF0) >> 4;
            reader.BaseStream.Position += length + offset;
            header = reader.ReadByte();
        }
        
        var dataRunSize = (int)(reader.BaseStream.Position - startPos - 1); // subtract 1 to compensate 0x00 header
        reader.BaseStream.Position = startPos;
        var bytes = reader.ReadBytes(dataRunSize);
        reader.BaseStream.Position += 1; // add 1 to move pointer back to the 0x00 header
        
        return bytes;
    }

    public byte[] GetAttributeData(BinaryReader reader, int clusterByteSize)
    {
        return Header.NonresidentFlag == 0x01 ? GetDataFromDataRun(reader, clusterByteSize) : Value;
    }

    public ulong GetActualDataSize() =>
        Header.NonresidentFlag == 0x01 ? Header.Nonresident.DataSize : Header.Resident.Size;
    
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