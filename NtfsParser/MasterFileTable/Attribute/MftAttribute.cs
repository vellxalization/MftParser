namespace NtfsParser.MasterFileTable.Attribute;

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

    public ulong GetActualDataSize() =>
        Header.IsNonresident ? Header.Nonresident.DataSize : Header.Resident.Size;
    
    private byte[] GetDataFromDataRun(VolumeReader volumeReader)
    {
        var start = volumeReader.GetPosition();

        bool firstRun = true;
        var dataRuns = CreateDataRunsFromValue();
        var data = new byte[Header.Nonresident.ValidDataSize];
        int offset = 0;
        for (int i = 0; i < dataRuns.Length - 1; ++i)
        {
            var dataRun = dataRuns[i];
            ReadDataFromRun(dataRun, false);
        }

        var lastDataRun = dataRuns[^1];
        ReadDataFromRun(lastDataRun, true);
        volumeReader.SetPosition(start);
        return data;

        void ReadDataFromRun(DataRun dataRun, bool isLastRun)
        {
            if (dataRun.Offset == 0) // sparse
            {
                offset += (int)dataRun.Length * volumeReader.ClusterByteSize;
                return;
            }

            if (firstRun)
            {
                volumeReader.SetLcnPosition((int)dataRun.Offset);
                firstRun = false;
            }
            else
            {
                volumeReader.SetVcnPosition((int)dataRun.Offset);
            }

            var buffer = new byte[(int)dataRun.Length * volumeReader.ClusterByteSize];
            volumeReader.ReadBytes(buffer, 0, buffer.Length);
            var copyLength = isLastRun ? data.Length - offset : buffer.Length;
            Array.Copy(buffer, 0, data, offset, copyLength);
            offset += copyLength;
        }
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
            if (offsetBit == 0)
            {
                runs.Add(new DataRun(header, length, 0));
                continue;
            }
            
            Int128 offset = 0;
            for (int j = 0; j < offsetBit; ++j)
            {
                offset |= Value[j + i] << (8 * j);
            }
            
            i += offsetBit;
            runs.Add(new DataRun(header, length, offset));
        }
        
        return runs.ToArray();
    }
}