namespace NtfsParser.MasterFileTable.Header;

public record struct FileReference(uint SegmentNumberLowPart, ushort SegmentNumberHighPart, ushort SequenceNumber)
{
    public static FileReference Parse(ReadOnlySpan<byte> rawReference)
    {
        var reader = new SpanBinaryReader(rawReference);
        var segmentNumberLowPart = reader.ReadUInt32();
        var segmentNumberHighPart = reader.ReadUInt16();
        var sequenceNumber = reader.ReadUInt16();
        
        return new FileReference(segmentNumberLowPart, segmentNumberHighPart, sequenceNumber);
    }
    
    public static FileReference CreateFromData(ref byte[] data)
    {
        using var reader = new BinaryReader(new MemoryStream(data));
        var segmentNumberLowPart = reader.ReadUInt32();
        var segmentNumberHighPart = reader.ReadUInt16();
        var sequenceNumber = reader.ReadUInt16();
        
        return new FileReference(segmentNumberLowPart, segmentNumberHighPart, sequenceNumber);
    }

    public ulong GetAddress()
    {
        ulong offset = SegmentNumberLowPart;
        offset |= (ulong)SegmentNumberHighPart << 32;
        return offset;
    }
}