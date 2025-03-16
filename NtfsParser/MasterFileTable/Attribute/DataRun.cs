namespace NtfsParser.MasterFileTable.Attribute;

public record struct DataRun(byte Header, UInt128 Length, Int128 Offset)
{
    public static DataRun[] ParseDataRuns(byte[] values)
    {
        var i = 0;
        List<DataRun> runs = new();
        while (i < values.Length)
        {
            var header = values[i++];
            var lengthBit = header & 0x0F;
            var offsetBit = (header & 0xF0) >> 4;
            UInt128 length = 0;
            for (int j = 0; j < lengthBit; ++j)
            {
                length |= (UInt128)(values[j + i] << (8 * j));
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
                offset |= values[j + i] << (8 * j);
            }
            
            i += offsetBit;
            runs.Add(new DataRun(header, length, offset));
        }
        
        return runs.ToArray();
    }
}