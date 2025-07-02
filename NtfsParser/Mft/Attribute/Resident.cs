namespace NtfsParser.Mft.Attribute;

/// <summary>
/// Structure that describes the content of a resident attribute
/// </summary>
/// <param name="ContentSize">Size of the content in bytes</param>
/// <param name="ContentOffset">Offset at which content starts</param>
/// <param name="IndexedFlag"></param>
public readonly record struct Resident(uint ContentSize, ushort ContentOffset, byte IndexedFlag)
{
    public static Resident Parse(Span<byte> rawResident)
    {
        var reader = new SpanBinaryReader(rawResident);
        var contentSize = reader.ReadUInt32();
        var contentOffset = reader.ReadUInt16();
        var indexedFlag = reader.ReadByte();
        // 1 spare byte for 8-byte alignment
        
        return new Resident(contentSize, contentOffset, indexedFlag);
    }
}