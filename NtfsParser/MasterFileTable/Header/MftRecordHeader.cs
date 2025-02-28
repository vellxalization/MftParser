namespace NtfsParser.MasterFileTable.Header;
public record struct MftRecordHeader(MultiSectorHeader Header, ulong LogFileSequenceNumber, ushort SequenceNumber,
    ushort ReferenceCount, ushort AttributesOffset, MftRecordHeaderFlags EntryFlags, uint UsedEntrySize, uint TotalEntrySize,
    FileReference FileReference, ushort FirstAttributeId, byte[] FixUp)
{
    // TODO: ^ "Type with suspicious equality is used as a member of a record type?" :raised_eyebrow: ^
    public static MftRecordHeader CreateFromStream(ref SpanBinaryReader reader)
    {
        var rawHeader = reader.ReadBytes(8);
        var header = MultiSectorHeader.Parse(rawHeader);
        if (header.Signature == MftSignature.Zeroes)
        {
            return new MftRecordHeader { Header = header };
        }
        var logfileSequenceNumber = reader.ReadUInt64();
        var sequenceNumber = reader.ReadUInt16();
        var referenceCount = reader.ReadUInt16();
        var attributesOffset = reader.ReadUInt16();
        var entryFlags = reader.ReadUInt16();
        var usedEntrySize = reader.ReadUInt32();
        var totalEntrySize = reader.ReadUInt32();
        var rawReference = reader.ReadBytes(8);
        var mftSegmentReference = FileReference.Parse(rawReference);
        var firstAttributeId = reader.ReadUInt16();
        reader.Position = header.FixUpOffset; // move position to the update sequence array
        var fixUp = reader.ReadBytes(header.FixUpLength);
        // TODO: there can be 2 additional fields depending on the OS version but I will ignore them for now
        // Maybe I should pass the size like in the StandardInformation
        
        return new MftRecordHeader(header, logfileSequenceNumber, sequenceNumber, referenceCount, attributesOffset, 
            (MftRecordHeaderFlags)entryFlags, usedEntrySize, totalEntrySize, mftSegmentReference, firstAttributeId, 
            fixUp.ToArray());
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