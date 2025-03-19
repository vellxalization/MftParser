namespace NtfsParser.Mft.MftRecord;
public record struct MftRecordHeader(MultiSectorHeader Header, ulong LogFileSequenceNumber, ushort SequenceNumber,
    ushort ReferenceCount, ushort AttributesOffset, MftRecordHeaderFlags EntryFlags, uint UsedEntrySize, uint AllocatedEntrySize,
    FileReference BaseRecordReference, ushort FirstAttributeId, FixUp FixUp)
{
    // TODO: ^ "Type with suspicious equality is used as a member of a record type?" :raised_eyebrow: ^
    public static MftRecordHeader CreateFromStream(ref SpanBinaryReader reader)
    {
        var rawHeader = reader.ReadBytes(8);
        var header = MultiSectorHeader.Parse(rawHeader);
        var logfileSequenceNumber = reader.ReadUInt64();
        var sequenceNumber = reader.ReadUInt16();
        var referenceCount = reader.ReadUInt16();
        var attributesOffset = reader.ReadUInt16();
        var entryFlags = reader.ReadUInt16();
        var usedEntrySize = reader.ReadUInt32();
        var allocatedEntrySize = reader.ReadUInt32();
        var rawReference = reader.ReadBytes(8);
        var baseRecordReference = FileReference.Parse(rawReference);
        var firstAttributeId = reader.ReadUInt16();
        reader.Position = header.FixUpOffset; // move position to the update sequence array
        var rawFixUp = reader.ReadBytes(header.FixUpLength * 2);
        var fixUp = FixUp.Parse(rawFixUp);
        // TODO: there can be 2 additional fields depending on the OS version but I will ignore them for now
        // Maybe I should pass the size like in the StandardInformation
        
        return new MftRecordHeader(header, logfileSequenceNumber, sequenceNumber, referenceCount, attributesOffset, 
            (MftRecordHeaderFlags)entryFlags, usedEntrySize, allocatedEntrySize, baseRecordReference, firstAttributeId,
            fixUp);
    }
}

[Flags]
public enum MftRecordHeaderFlags : ushort
{
    InUse = 0x01,
    IsDirectory = 0x02,
    IsExtension = 0x04,
    IsViewIndex = 0x08
}