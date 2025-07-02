using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData;

/// <summary>
/// An attribute that contains data about storage status. Used by indices and $MFT meta file.
/// Indices use it to indicate what index records are used in an index allocation.
/// $MFT file use it to indicate what file records are used
/// </summary>
/// <param name="Data">Bit field</param>
public readonly record struct Bitmap(byte[] Data)
{
    public static Bitmap CreateFromRawData(in RawAttributeData rawData) => new(rawData.Data);

    /// <summary>
    /// Checks if a bit is set to true. Values outside the range will return false
    /// </summary>
    /// <param name="bitIndex">0-based index of a bit</param>
    /// <returns>Bit is set</returns>
    public bool IsSet(int bitIndex)
    {
        if (bitIndex < 0)
            return false;
        
        var byteIndex = bitIndex / 8;
        if (byteIndex >= Data.Length)
            return false;
        
        var bitIndexInByte = bitIndex % 8;
        var bitMask = 1 << bitIndexInByte; // read from the least significant bit
        return (Data[byteIndex] & bitMask) != 0;
    }
}