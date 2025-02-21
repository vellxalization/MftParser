using FileSystemTraverser.MasterFileTable.AttributeRecord;
using FileSystemTraverser.MasterFileTable.Header;

namespace FileSystemTraverser.MasterFileTable;

public record struct MftRecord(MftRecordHeader RecordHeader, MftAttribute[] Attributes)
{
    public static MftRecord CreateFromStream(BinaryReader reader, int mftRecordSize)
    {
        var startPos = reader.BaseStream.Position;
        
        var header = MftRecordHeader.CreateFromStream(reader);
        if (header == default)
        {
            reader.BaseStream.Position = startPos + mftRecordSize;
            return default;
        }
        
        var attrs = new List<MftAttribute>(1);
        var attribute = MftAttribute.CreateFromStream(reader);
        while (attribute.Header.Type != AttributeType.Unknown)
        {
            attrs.Add(attribute);
            attribute = MftAttribute.CreateFromStream(reader);
        }
        var bytesRead = (int)(reader.BaseStream.Position - startPos);
        _ = reader.BaseStream.Position += mftRecordSize - bytesRead; // padding
        
        return new MftRecord(header, attrs.ToArray());
    }
}