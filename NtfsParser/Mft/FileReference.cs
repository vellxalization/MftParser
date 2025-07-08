namespace NtfsParser.Mft;

/// <summary>
/// Structure that represents an address in the MFT. Consists of the 48-bit number (0-based index in the MFT) and the 16-bit sequence number.
/// </summary>
/// <param name="SegmentNumberLowPart">First 32 bits of the index</param>
/// <param name="SegmentNumberHighPart">Last 16 bits of the index</param>
/// <param name="SequenceNumber">See <see cref="MftRecordHeader"/>. This value should be equal to the record's value</param>
public readonly record struct FileReference(uint SegmentNumberLowPart, ushort SegmentNumberHighPart, ushort SequenceNumber)
{
    /// <summary>
    /// 0-based index in the MFT
    /// </summary>
    public long MftIndex => GetMftOffset();
    
    public static FileReference Parse(Span<byte> rawReference)
    {
        var reader = new SpanBinaryReader(rawReference);
        var segmentNumberLowPart = reader.ReadUInt32();
        var segmentNumberHighPart = reader.ReadUInt16();
        var sequenceNumber = reader.ReadUInt16();
        
        return new FileReference(segmentNumberLowPart, segmentNumberHighPart, sequenceNumber);
    }
    
    private long GetMftOffset()
    {
        long offset = SegmentNumberLowPart;
        offset |= (long)SegmentNumberHighPart << 32;
        return offset;
    }
}