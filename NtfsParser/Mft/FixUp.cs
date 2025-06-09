namespace NtfsParser.Mft;

public readonly record struct FixUp(byte[] Placeholder, byte[] Values)
{
    public static FixUp Parse(Span<byte> rawFixUp)
    {
        var reader = new SpanBinaryReader(rawFixUp);
        var fixUpPlaceholder = reader.ReadBytes(2);
        var fixUpValue = reader.ReadBytes(rawFixUp.Length - 2);
        
        return new FixUp(fixUpPlaceholder.ToArray(), fixUpValue.ToArray());
    }

    public void ReverseFixUp(Span<byte> entry, int sectorSize)
    {
        var fixUpLength = Values.Length / 2;
        for (int i = 0; i < fixUpLength; ++i)
        {
            var lastBytesOffset = (i + 1) * sectorSize - 2;
            if (entry[lastBytesOffset] != Placeholder[0])
                throw new FixUpMismatchException(Placeholder[0], entry[lastBytesOffset]);

            if (entry[lastBytesOffset + 1] != Placeholder[1])
                throw new FixUpMismatchException(Placeholder[1], entry[lastBytesOffset + 1]);

            var valuesOffset = i * 2;
            entry[lastBytesOffset] = Values[valuesOffset];
            entry[lastBytesOffset + 1] = Values[valuesOffset + 1];
        }
    }
    
    public void ReapplyFixUp(Span<byte> entry, int sectorSize)
    {
        var fixUpLength = Values.Length / 2;
        for (int i = 0; i < fixUpLength; ++i)
        {
            var lastBytesOffset = (i + 1) * sectorSize - 2;
            entry[lastBytesOffset] = Placeholder[0];
            entry[lastBytesOffset + 1] = Placeholder[1];
        }
    }
}