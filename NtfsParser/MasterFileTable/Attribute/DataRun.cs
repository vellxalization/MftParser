namespace NtfsParser.MasterFileTable.Attribute;

public record struct DataRun(byte Header, long Length, long Offset)
{
    public static DataRun[] ParseDataRuns(byte[] values)
    {
        var i = 0;
        List<DataRun> runs = new();
        while (i < values.Length)
        {
            var header = values[i++];
            var lengthNibble = header & 0x0F;
            var offsetNibble = (header & 0xF0) >> 4;
            long length = 0;
            int j = 0;
            for (; j < lengthNibble; ++j)
            {
                length |= (long)values[j + i] << (8 * j);
            }
            
            i += lengthNibble;
            if (offsetNibble == 0)
            {
                runs.Add(new DataRun(header, length, 0));
                continue;
            }

            j = 0;
            long offset = 0;
            for (; j < offsetNibble; ++j)
            {
                offset |= (long)values[j + i] << (8 * j);
            }

            if (IsNegative(offset, j - 1))
            {
                OverflowNumber(ref offset, offsetNibble);
            }
            
            i += offsetNibble;
            runs.Add(new DataRun(header, length, offset));
        }
        
        return runs.ToArray();

        bool IsNegative(long number, int msbIndex)
        {
            var mask = 0x80 << (8 * msbIndex);
            return (number & mask) != 0;
        }

        void OverflowNumber(ref long number, int offsetLength)
        {
            // we're using long numbers so we calculate everything with 8 bytes per number in mind
            for (int v = 7; v >= offsetLength; --v)
            {
                number |= (long)0xFF << (8 * v);
            }
        }
    }
}