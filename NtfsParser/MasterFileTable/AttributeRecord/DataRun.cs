namespace FileSystemTraverser.MasterFileTable.AttributeRecord;

public record struct DataRun(byte Header, UInt128 Length, UInt128 Offset)
{
    public static DataRun CreateFromStream(BinaryReader reader)
    {
        var header = reader.ReadByte();
        if (header == 0x00)
        {
            return new DataRun(0, 0, 0);
        }

        UInt128 length = 0;
        var lengthSize = header & 0x0F;
        for (var i = 0; i < lengthSize; ++i)
        {
            var lengthPiece = reader.ReadByte();
            length |= (UInt128)lengthPiece << (8 * i);
        }
        
        UInt128 offset = 0;
        var offsetSize = (header & 0xF0) >> 4;
        for (var i = 0; i < offsetSize; ++i)
        {
            var offsetPiece = reader.ReadByte();
            offset |= (UInt128)offsetPiece << (8 * i);
        }
        
        return new DataRun(header, length, offset);
    }
}