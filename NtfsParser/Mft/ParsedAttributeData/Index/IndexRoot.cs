using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData.Index;

/// <summary>
/// An attribute that represents an index tree root. Is a single node in an index tree. Always present in records that are part of an index
/// </summary>
/// <param name="RootHeader">Header with information about index</param>
/// <param name="NodeHeader">Header of a node</param>
/// <param name="Entries">Index entries</param>
public readonly record struct IndexRoot(IndexRootHeader RootHeader, IndexNodeHeader NodeHeader, IndexEntry[] Entries)
{
    public static IndexRoot CreateFromRawData(in RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var rawHeader = reader.ReadBytes(16);
        var header = IndexRootHeader.Parse(rawHeader);
        var offset = reader.Position; // offsets to entries are relative to the node header so we copy it

        rawHeader = reader.ReadBytes(16);
        var nodeHeader = IndexNodeHeader.Parse(rawHeader);

        reader.Position = offset + (int)nodeHeader.EntryListOffset;
        var entriesLength = offset + (int)nodeHeader.EntryListBufferEndOffset - reader.Position;
        var rawEntries = reader.ReadBytes(entriesLength);
        var entries = IndexEntry.ParseEntries(rawEntries);

        return new IndexRoot(header, nodeHeader, entries.ToArray());
    }
}

/// <summary>
/// Header of an index root node
/// </summary>
/// <param name="AttributeType">Type of the attribute that is store in index records. 0 means that the stored data is not an attribute</param>
/// <param name="CollationRule">How records are sorted</param>
/// <param name="IndexRecordByteSize">Size of a single index records in bytes. Typically, is set to 4096</param>
/// <param name="IndexRecordClusterSize">Size of a single index record in clusters. Typically, is set to 1</param>
public record struct IndexRootHeader(AttributeType AttributeType, CollationRule CollationRule, uint IndexRecordByteSize,
    byte IndexRecordClusterSize)
{
    public static IndexRootHeader Parse(Span<byte> rawHeader)
    {
        var reader = new SpanBinaryReader(rawHeader);
        var attributeType = reader.ReadUInt32();
        var collationRule = reader.ReadUInt32();
        var indexRecordByteSize = reader.ReadUInt32();
        var indexRecordClusterSize = reader.ReadByte();
        // last 3 bytes are padding 

        return new IndexRootHeader((AttributeType)attributeType, (CollationRule)collationRule, indexRecordByteSize, indexRecordClusterSize);
    }
}

public enum CollationRule : uint
{
    Binary = 0x00000000,
    Filename = 0x00000001,
    UnicodeString = 0x00000002,
    NtofsUlong = 0x00000010,
    NtofsSid = 0x00000011,
    NtofsSecurityHash = 0x00000012,
    NtofsUlongs = 0x00000013
}