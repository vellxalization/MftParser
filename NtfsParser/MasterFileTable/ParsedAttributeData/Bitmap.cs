using NtfsParser.MasterFileTable.Attribute;

namespace NtfsParser.MasterFileTable.ParsedAttributeData;

public record struct Bitmap(byte[] Data)
{
    public static Bitmap CreateFromRawData(RawAttributeData rawData)
    {
        return new Bitmap(rawData.Data);
    }

    public bool IsSet(int bitIndex)
    {
        if (bitIndex < 0)
        {
            return false;
        }
        
        var byteIndex = bitIndex / 8;
        if (byteIndex >= Data.Length)
        {
            return false;
        }
        
        var bitIndexInByte = bitIndex % 8;
        var bitMask = 1 << bitIndexInByte; // read from the least significant bit
        return (Data[byteIndex] & bitMask) != 0;
    }
}