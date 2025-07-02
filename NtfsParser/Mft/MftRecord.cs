using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft;

/// <summary>
/// Structure that represents single MFT record
/// </summary>
/// <param name="RecordHeader">Record's header that contains information about itself</param>
/// <param name="Attributes">Attributes associated with the record.
/// Additional attributes might be stored in other MFT records if the $ATTRIBUTE_LIST attribute is preset.</param>
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
        
        header.FixUp.ReapplyFixUp(rawMftRecord, sectorSize);
        return new MftRecord(header, attributes);
    }
}

/// <summary>
/// MFT record's header
/// </summary>
/// <param name="MultiSectorHeader">Part of the header containing a signature and information about fix up values</param>
/// <param name="LogFileSequenceNumber">Sequence number in $LogFile. Changed every time this record is modified</param>
/// <param name="SequenceNumber">Sequence value. Number of times this record was reused (incremented when the file is deleted)</param>
/// <param name="HardLinkCount">Number of hard links. Each hard link creates one additional FILE_NAME attribute</param>
/// <param name="AttributesOffset">Offset at which attributes start</param>
/// <param name="EntryFlags">Record's flags</param>
/// <param name="UsedEntrySize">Size of the record in bytes</param>
/// <param name="AllocatedEntrySize">Size that the record takes up on disk.
/// Should be equal to the MFT record size in the boot sector (Typically, 1024 bytes)</param>
/// <param name="BaseRecordReference">A reference to the base MFT record.
/// Only used when a record is allocated to store additional attributes for the base record's $ATTRIBUTE_LIST</param>
/// <param name="NextAttributeId">Attribute that will be assigned to the next attribute</param>
/// <param name="FixUp">Fix up values of the record</param>
public readonly record struct MftRecordHeader(MultiSectorHeader MultiSectorHeader, ulong LogFileSequenceNumber, ushort SequenceNumber,
    ushort HardLinkCount, ushort AttributesOffset, MftRecordHeaderFlags EntryFlags, uint UsedEntrySize, uint AllocatedEntrySize,
    FileReference BaseRecordReference, ushort NextAttributeId, FixUp FixUp)
{
    public static MftRecordHeader CreateFromStream(ref SpanBinaryReader reader)
    {
        var rawHeader = reader.ReadBytes(8);
        var header = MultiSectorHeader.Parse(rawHeader);
        if (header.Signature == MftSignature.Empty)
            return default;

        var logfileSequenceNumber = reader.ReadUInt64();
        var sequenceNumber = reader.ReadUInt16();
        var hardLinkCount = reader.ReadUInt16();
        var attributesOffset = reader.ReadUInt16();
        var entryFlags = reader.ReadUInt16();
        var usedEntrySize = reader.ReadUInt32();
        var allocatedEntrySize = reader.ReadUInt32();
        var rawReference = reader.ReadBytes(8);
        var baseRecordReference = FileReference.Parse(rawReference);
        var nextAttributeId = reader.ReadUInt16();
        reader.Position = header.FixUpOffset;
        var rawFixUp = reader.ReadBytes(header.FixUpLength * 2);
        var fixUp = FixUp.Parse(rawFixUp);
        // TODO: there can be 2 additional fields depending on the OS version but I will ignore them for now
        // Maybe I should pass the size like in the StandardInformation.
        // Kinda irrelevant because these fields aren't used since win 2k

        return new MftRecordHeader(header, logfileSequenceNumber, sequenceNumber, hardLinkCount, attributesOffset,
            (MftRecordHeaderFlags)entryFlags, usedEntrySize, allocatedEntrySize, baseRecordReference, nextAttributeId,
            fixUp);
    }
}

/// <summary>
/// MFT record's flags
/// </summary>
[Flags]
public enum MftRecordHeaderFlags : ushort
{
    /// <summary>
    /// Record is a part of MFT. Most of the records will have this flag.
    /// Absence of one means that the record isn't used by file system and can be overwritten by newer ones
    /// </summary>
    InUse = 0x01,
    /// <summary>
    /// Record is a directory (contains $I30 index)
    /// </summary>
    IsDirectory = 0x02,
    /// <summary>
    /// Record is an extension (used by records in $Extend directory)
    /// </summary>
    IsExtension = 0x04,
    /// <summary>
    /// Is index. Used by other special indices other than $I30 ($Secure, $ObjID, $Quota, $Reparse)
    /// </summary>
    IsViewIndex = 0x08
}

/// <summary>
/// MFT record's header that contains a signature and fix up value
/// https://learn.microsoft.com/en-us/windows/win32/devnotes/multi-sector-header
/// </summary>
/// <param name="Signature">Record's signature</param>
/// <param name="FixUpOffset">Offset inside MFT record from where fix up values start</param>
/// <param name="FixUpLength">Length of the fix up values. Single fix up value is 2-bytes long</param>
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

/// <summary>
/// Record's signature
/// </summary>
public enum MftSignature
{
    /// <summary>
    /// Reserved for internal usage in library to avoid throwing exceptions when we read empty space in a disk 
    /// </summary>
    Empty,
    /// <summary>
    /// Regular record signature
    /// </summary>
    File,
    /// <summary>
    /// An error was found in the record during a check
    /// </summary>
    Baad
}