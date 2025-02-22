using FileSystemTraverser.MasterFileTable.AttributeRecord;
using FileSystemTraverser.MasterFileTable.Header;

namespace FileSystemTraverser.MasterFileTable.AttributeData._ATTRIBUTE_LIST;

public record struct AttributeListEntry(AttributeType AttributeType, ushort Size, byte NameSize, byte NameOffset, 
    ulong Vcn, MftSegmentReference FileReference, ushort AttributeId, byte[] Name)
{
    public static AttributeListEntry CreateFromStream(BinaryReader reader)
    {
        var start = reader.BaseStream.Position;
        var attributeType = reader.ReadUInt32();
        var size = reader.ReadUInt16();
        var nameSize = reader.ReadByte();
        var nameOffset = reader.ReadByte();
        var vcn = reader.ReadUInt64();
        var fileReference = MftSegmentReference.CreateFromStream(reader);
        var attributeId = reader.ReadUInt16();
        byte[] name = [];
        if (nameSize != 0)
        {
            reader.BaseStream.Position = nameOffset;
            name = reader.ReadBytes(nameSize * 2); // utf-16 encoded, 2 bytes per char
        }

        reader.BaseStream.Position = start + Math.Abs(reader.BaseStream.Length - size);
        return new AttributeListEntry((AttributeType)attributeType, size, nameSize, nameOffset, vcn, fileReference, attributeId, name);
    }
}