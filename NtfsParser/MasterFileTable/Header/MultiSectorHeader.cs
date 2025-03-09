namespace NtfsParser.MasterFileTable.Header;

public record struct MultiSectorHeader(MftSignature Signature, ushort FixUpOffset, ushort FixUpLength)
{
    public static MultiSectorHeader Parse(ReadOnlySpan<byte> rawHeader)
    {
        var reader = new SpanBinaryReader(rawHeader);
        var signature = reader.ReadBytes(4);
        var enumSignature = signature switch
        {
            [0, 0, 0, 0] => MftSignature.Empty,
            [(byte)'F', (byte)'I', (byte)'L', (byte)'E'] => MftSignature.File,
            [(byte)'B', (byte)'A', (byte)'A', (byte)'D'] => MftSignature.Baad,
            _ => throw new Exception("Unknown signature") // TODO: temp solution
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