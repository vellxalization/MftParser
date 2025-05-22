using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft;

public readonly record struct MftRecord(MftRecordHeader RecordHeader, MftAttribute[] Attributes)
{
    public static MftRecord Parse(Span<byte> rawMftRecord, int sectorSize)
    {
        var reader = new SpanBinaryReader(rawMftRecord);
        var header = MftRecordHeader.CreateFromStream(ref reader);
        if (header.MultiSectorHeader.Signature == MftSignature.Empty)
            return default;

        header.FixUp.ReverseFixUp(rawMftRecord, sectorSize);
        reader.Position = header.AttributesOffset;
        var rawAttributes = reader.ReadBytes((int)header.UsedEntrySize - reader.Position);
        var attributes = MftAttribute.ParseAttributes(rawAttributes);
        // rest is unused bytes
        return new MftRecord(header, attributes);
    }
}

public readonly record struct MftRecordHeader(MultiSectorHeader MultiSectorHeader, ulong LogFileSequenceNumber, ushort SequenceNumber,
    ushort ReferenceCount, ushort AttributesOffset, MftRecordHeaderFlags EntryFlags, uint UsedEntrySize, uint AllocatedEntrySize,
    FileReference BaseRecordReference, ushort FirstAttributeId, FixUp FixUp)
{
    public static MftRecordHeader CreateFromStream(ref SpanBinaryReader reader)
    {
        var rawHeader = reader.ReadBytes(8);
        var header = MultiSectorHeader.Parse(rawHeader);
        if (header.Signature == MftSignature.Empty)
            return default;

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
        reader.Position = header.FixUpOffset;
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

public readonly record struct MultiSectorHeader(MftSignature Signature, ushort FixUpOffset, ushort FixUpLength)
{
    public static MultiSectorHeader Parse(Span<byte> rawHeader)
    {
        var reader = new SpanBinaryReader(rawHeader);
        var signature = reader.ReadBytes(4);
        var enumSignature = signature switch
        {
            [0, 0, 0, 0] => MftSignature.Empty,
            [(byte)'F', (byte)'I', (byte)'L', (byte)'E'] => MftSignature.File,
            [(byte)'B', (byte)'A', (byte)'A', (byte)'D'] => MftSignature.Baad,
            _ => throw new InvalidMftRecordException(signature)
        };

        var fixUpOffset = reader.ReadUInt16();
        var fixUpLength = reader.ReadUInt16();

        return new MultiSectorHeader(enumSignature, fixUpOffset, fixUpLength);
    }
}

public enum MftSignature
{
    Empty,
    File,
    Baad
}