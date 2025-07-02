namespace NtfsParser.Mft;

/// <summary>
/// Reference to an MFT record. Represents an offset in MFT in records.
/// E.g. value 32 means that the desired record is 32nd in the MFT (0-based)
/// </summary>
/// <param name="SegmentNumberLowPart">Reference is a 48-bit number. This is it's lowest part (first 32 bits)</param>
/// <param name="SegmentNumberHighPart">Reference is a 48-bit number. This is it's highest part (last 16 bits)</param>
/// <param name="SequenceNumber">Should be equal to the parent's record sequence number.
/// If it isn't, then it's an old/corrupted reference</param>
public readonly record struct FileReference(uint SegmentNumberLowPart, ushort SegmentNumberHighPart, ushort SequenceNumber)
{
    /// <summary>
    /// Offset in records inside the MFT
    /// </summary>
    public ulong MftIndex => GetMftOffset();
    
    public static FileReference Parse(Span<byte> rawReference)
    {
        var reader = new SpanBinaryReader(rawReference);
        var segmentNumberLowPart = reader.ReadUInt32();
        var segmentNumberHighPart = reader.ReadUInt16();
        var sequenceNumber = reader.ReadUInt16();
        
        return new FileReference(segmentNumberLowPart, segmentNumberHighPart, sequenceNumber);
    }
    
    private ulong GetMftOffset()
    {
        ulong offset = SegmentNumberLowPart;
        offset |= (ulong)SegmentNumberHighPart << 32;
        return offset;
    }
}