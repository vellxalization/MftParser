using System.Buffers.Binary;

namespace NtfsParser;

public ref struct SpanBinaryReader(Span<byte> span)
{
    public int Position { get; set; } = 0;
    public int Length => InternalSpan.Length;
    public Span<byte> InternalSpan { get; } = span;

    public void Skip(int bytes) => Position += bytes;
    
    public byte ReadByte() => InternalSpan[Position++];
    
    public sbyte ReadSByte() => unchecked((sbyte)InternalSpan[Position++]);

    public Span<byte> ReadBytes(int length)
    {
        var slice = InternalSpan.Slice(Position, length);
        Position += length;
        return slice;
    }

    public short ReadInt16(Endianness endianness = Endianness.LittleEndian)
    {
        var span = ReadBytes(2);
        return endianness == Endianness.LittleEndian 
            ? BinaryPrimitives.ReadInt16LittleEndian(span) 
            : BinaryPrimitives.ReadInt16BigEndian(span);
    }

    public ushort ReadUInt16(Endianness endianness = Endianness.LittleEndian)
    {
        var span = ReadBytes(2);
        return endianness == Endianness.LittleEndian 
            ? BinaryPrimitives.ReadUInt16LittleEndian(span) 
            : BinaryPrimitives.ReadUInt16BigEndian(span);
    }

    public int ReadInt32(Endianness endianness = Endianness.LittleEndian)
    {
        var span = ReadBytes(4);
        return endianness == Endianness.LittleEndian
            ? BinaryPrimitives.ReadInt32LittleEndian(span)
            : BinaryPrimitives.ReadInt32BigEndian(span);
    }

    public uint ReadUInt32(Endianness endianness = Endianness.LittleEndian)
    {
        var span = ReadBytes(4);
        return endianness == Endianness.LittleEndian
            ? BinaryPrimitives.ReadUInt32LittleEndian(span)
            : BinaryPrimitives.ReadUInt32BigEndian(span);
    }

    public long ReadInt64(Endianness endianness = Endianness.LittleEndian)
    {
        var span = ReadBytes(8);
        return endianness == Endianness.LittleEndian
            ? BinaryPrimitives.ReadInt64LittleEndian(span)
            : BinaryPrimitives.ReadInt64BigEndian(span);
    }
    
    public ulong ReadUInt64(Endianness endianness = Endianness.LittleEndian)
    {
        var span = ReadBytes(8);
        return endianness == Endianness.LittleEndian
            ? BinaryPrimitives.ReadUInt64LittleEndian(span)
            : BinaryPrimitives.ReadUInt64BigEndian(span);
    }
}

public enum Endianness
{
    LittleEndian = 0,
    BigEndian = 1
}