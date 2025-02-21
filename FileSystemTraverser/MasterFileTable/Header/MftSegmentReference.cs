namespace FileSystemTraverser.MasterFileTable.Header;

public record struct MftSegmentReference(uint SegmentNumberLowPart, ushort SegmentNumberHighPart, ushort SequenceNumber)
{
    public static MftSegmentReference CreateFromStream(BinaryReader reader)
    {
        var segmentNumberLowPart = reader.ReadUInt32();
        var segmentNumberHighPart = reader.ReadUInt16();
        var sequenceNumber = reader.ReadUInt16();
        
        return new MftSegmentReference(segmentNumberLowPart, segmentNumberHighPart, sequenceNumber);
    }
    
    public static MftSegmentReference CreateFromData(ref byte[] data)
    {
        using var reader = new BinaryReader(new MemoryStream(data));
        var segmentNumberLowPart = reader.ReadUInt32();
        var segmentNumberHighPart = reader.ReadUInt16();
        var sequenceNumber = reader.ReadUInt16();
        
        return new MftSegmentReference(segmentNumberLowPart, segmentNumberHighPart, sequenceNumber);
    }

    public ulong GetAddress()
    {
        ulong offset = SegmentNumberLowPart;
        offset |= (ulong)SegmentNumberHighPart << 32;
        return offset;
    }
}