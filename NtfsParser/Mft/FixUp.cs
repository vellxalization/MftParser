namespace NtfsParser.Mft;

/// <summary>
/// Record's fix up. Used for error detection in sectors.
/// Last two bytes of each sector are replaced with the placeholder and stored. Used by MFT and INDX records.
/// </summary>
/// <param name="Placeholder">Two bytes that replace last two byte of each sector</param>
/// <param name="Values">Actual values that were replaced with the placeholder</param>
public readonly record struct FixUp(byte[] Placeholder, byte[] Values)
{
    public static FixUp Parse(Span<byte> rawFixUp)
    {
        var reader = new SpanBinaryReader(rawFixUp);
        var fixUpPlaceholder = reader.ReadBytes(2);
        var fixUpValue = reader.ReadBytes(rawFixUp.Length - 2);
        
        return new FixUp(fixUpPlaceholder.ToArray(), fixUpValue.ToArray());
    }
    
    /// <summary>
    /// Replaces placeholder values of an entry with actual values
    /// </summary>
    /// <param name="entry">Raw record</param>
    /// <param name="sectorSize">Size of a single sector</param>
    /// <exception cref="FixUpMismatchException">Last bytes of a sector are not equal to the placeholder value</exception>
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
    
    /// <summary>
    /// Replaces actual values with the placeholder. Used because we don't want to mess with MFT stream's buffer.
    /// </summary>
    /// <param name="entry">Raw entry</param>
    /// <param name="sectorSize">Size of a single sector</param>
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