namespace FileSystemTraverser.MasterFileTable.Header;
public record struct MftRecordHeader(MultiSectorHeader MultiSectorHeader, ulong LogFileSequenceNumber, ushort SequenceNumber,
    ushort ReferenceCount, ushort AttributesOffset, ushort EntryFlags, uint UsedEntrySize, uint TotalEntrySize,
    MftSegmentReference MftSegmentReference, ushort FirstAttributeId, byte[] UpdateSequenceArray)
{
    public static MftRecordHeader CreateFromStream(BinaryReader reader)
    {
        var startPosition = reader.BaseStream.Position;
        
        var multiSectorHeader = MultiSectorHeader.CreateFromStream(reader);
        if (multiSectorHeader.Signature is [0, 0, 0, 0])
        {
            return default;
        }
        
        var logfileSequenceNumber = reader.ReadUInt64();
        var sequenceNumber = reader.ReadUInt16();
        var referenceCount = reader.ReadUInt16();
        var attributesOffset = reader.ReadUInt16();
        var entryFlags = reader.ReadUInt16();
        var usedEntrySize = reader.ReadUInt32();
        var totalEntrySize = reader.ReadUInt32();
        var mftSegmentReference = MftSegmentReference.CreateFromStream(reader);
        var firstAttributeId = reader.ReadUInt16();
        reader.BaseStream.Position = startPosition + multiSectorHeader.UpdateSequenceOffset; // move position to the update sequence array
        var updateSequenceArray = reader.ReadBytes(multiSectorHeader.UpdateSequenceLength);
        reader.BaseStream.Position = startPosition + attributesOffset; // move position to the first attribute
        
        return new MftRecordHeader(multiSectorHeader, logfileSequenceNumber, sequenceNumber, referenceCount, attributesOffset, entryFlags, usedEntrySize, totalEntrySize, mftSegmentReference, firstAttributeId, updateSequenceArray);
    }
}