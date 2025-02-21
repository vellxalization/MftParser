namespace FileSystemTraverser;

public static class BinaryReaderExtensions
{
    public static ushort ReadWord(this BinaryReader reader) => reader.ReadUInt16();
    public static uint ReadDword(this BinaryReader reader) => reader.ReadUInt32();
    public static long ReadLongLong(this BinaryReader reader) => reader.ReadInt64();
    public static ushort ReadUShort(this BinaryReader reader) => reader.ReadUInt16();
    public static ulong ReadULongLong(this BinaryReader reader) => reader.ReadUInt64();
    public static uint ReadULong(this BinaryReader reader) => reader.ReadUInt32();
    public static byte ReadUChar(this BinaryReader reader) => reader.ReadByte();
}