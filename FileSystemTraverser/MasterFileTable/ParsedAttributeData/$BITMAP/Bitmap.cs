namespace FileSystemTraverser.MasterFileTable.ParsedAttributeData._BITMAP;

public record struct Bitmap(byte[] Data)
{
    public static Bitmap CreateFromData(ref byte[] data)
    {
        return new Bitmap(data);
    }

    public bool IsSet(int bitIndex)
    {
        var byteIndex = bitIndex / 8;
        var bitIndexInByte = bitIndex % 8;
        var bitMask = 1 << bitIndexInByte; // read from the least significant bit
        return (Data[byteIndex] & bitMask) != 0;
    }
}