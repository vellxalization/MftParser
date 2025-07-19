namespace MftParser.Mft;

/// <summary>
/// Fixup. Used to protect structs from incomplete multisector transfers. Important structs that use multiple sectors to store data
/// (e.g. MFT and INDX records) use this mechanism. It works by replacing the last 2 bytes of each sector (bytes 510 and 511 for the standard sector size)
/// with the fixup value and storing the original bytes in the array
/// </summary>
/// <param name="FixUpValue">Value that replaces last two byte of each sector</param>
/// <param name="ActualValues">Values that were replaced with the fixup value</param>
public readonly record struct FixUp(byte[] FixUpValue, byte[] ActualValues)
{
    public static FixUp Parse(Span<byte> rawFixUp)
    {
        var reader = new SpanBinaryReader(rawFixUp);
        var fixUpPlaceholder = reader.ReadBytes(2);
        var fixUpValue = reader.ReadBytes(rawFixUp.Length - 2);
        
        return new FixUp(fixUpPlaceholder.ToArray(), fixUpValue.ToArray());
    }
    
    /// <summary>
    /// Replaces fixup values with the actual bytes
    /// </summary>
    /// <param name="entry">Raw record</param>
    /// <param name="sectorSize">Size of a single sector in bytes</param>
    /// <exception cref="FixUpMismatchException">Last bytes of a sector are not equal to the fixup value</exception>
    public void ReverseFixUp(Span<byte> entry, int sectorSize)
    {
        var fixUpLength = ActualValues.Length / 2;
        for (int i = 0; i < fixUpLength; ++i)
        {
            var lastBytesOffset = (i + 1) * sectorSize - 2;
            if (entry[lastBytesOffset] != FixUpValue[0])
                throw new FixUpMismatchException(FixUpValue[0], entry[lastBytesOffset]);

            if (entry[lastBytesOffset + 1] != FixUpValue[1])
                throw new FixUpMismatchException(FixUpValue[1], entry[lastBytesOffset + 1]);

            var valuesOffset = i * 2;
            entry[lastBytesOffset] = ActualValues[valuesOffset];
            entry[lastBytesOffset + 1] = ActualValues[valuesOffset + 1];
        }
    }
    
    /// <summary>
    /// Replaces last two bytes of the entry with the fixup value. Used because we don't want to mess with MFT stream's buffer.
    /// </summary>
    /// <param name="entry">Raw entry</param>
    /// <param name="sectorSize">Size of a single sector in bytes</param>
    public void ApplyFixUp(Span<byte> entry, int sectorSize)
    {
        var fixUpLength = ActualValues.Length / 2;
        for (int i = 0; i < fixUpLength; ++i)
        {
            var lastBytesOffset = (i + 1) * sectorSize - 2;
            entry[lastBytesOffset] = FixUpValue[0];
            entry[lastBytesOffset + 1] = FixUpValue[1];
        }
    }
}