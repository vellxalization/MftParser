using FileSystemTraverser.MasterFileTable.AttributeRecord;
using FileSystemTraverser.MasterFileTable.Header;

namespace FileSystemTraverser.MasterFileTable.ParsedAttributeData.AttributeList;

public record struct AttributeListEntry(AttributeType AttributeType, ushort RecordSize, byte NameSize, byte NameOffset, 
    ulong Vcn, FileReference FileReference, ushort AttributeId, byte[] Name)
{
    public static AttributeListEntry Parse(ReadOnlySpan<byte> rawData)
    {
        var reader = new SpanBinaryReader(rawData);
        var attributeType = reader.ReadUInt32();
        var recordSize = reader.ReadUInt16();
        var nameSize = reader.ReadByte();
        var nameOffset = reader.ReadByte();
        var vcn = reader.ReadUInt64();
        var fileReference = FileReference.Parse(reader.ReadBytes(8));
        var attributeId = reader.ReadUInt16();
        if (nameSize == 0)
        {
            return new AttributeListEntry((AttributeType)attributeType, recordSize, nameSize, nameOffset, vcn, 
                fileReference, attributeId, []);
        }
        
        reader.Position = nameOffset;
        var name = reader.ReadBytes(nameSize * 2); // utf-16 encoded, 2 bytes per char

        return new AttributeListEntry((AttributeType)attributeType, recordSize, nameSize, nameOffset, vcn, fileReference, attributeId, name.ToArray());
    }
}