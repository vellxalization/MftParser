namespace NtfsParser.Mft;

public readonly record struct FileReference(uint SegmentNumberLowPart, ushort SegmentNumberHighPart, ushort SequenceNumber)
{
    public ulong MftOffset => GetMftOffset();
    
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