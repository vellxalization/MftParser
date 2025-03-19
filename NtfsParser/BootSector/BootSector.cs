namespace NtfsParser.BootSector;

public record struct BootSector(ulong OemId, BiosParamsBlock Bpb, ExtendedBpb ExtBpb, byte[] Bootstrap)
{
    public int SectorByteSize => Bpb.BytesPerSector;
    public int ClusterByteSize => Bpb.BytesPerSector * Bpb.SectorsPerCluster;
    public int MftRecordByteSize => GetMftRecordByteSize();
    public int IndexRecordByteSize => GetIndexRecordByteSize();
    public long MftStartByteOffset => ExtBpb.LogicalClusterForMft * ClusterByteSize;
    
    public static BootSector Parse(Span<byte> rawBootSector)
    {
        var reader = new SpanBinaryReader(rawBootSector);
        var jmp = reader.ReadBytes(3);
        if (jmp is not [0xEB, 0x52, 0x90])
        {
            throw new InvalidJmpException(jmp.ToArray(), reader.Position);
        }
        
        var oemId = reader.ReadUInt64();
        if (oemId != 0x202020205346544E)
        {
            throw new InvalidOemIdException(oemId, reader.Position);
        }

        var rawBpb = reader.ReadBytes(25);
        var bpb = BiosParamsBlock.Parse(rawBpb);
        var rawExtBpb = reader.ReadBytes(48);
        var extBpb = ExtendedBpb.Parse(rawExtBpb);
        var bootstrap = reader.ReadBytes(426);
        var sectorEnd = reader.ReadUInt16();
        if (sectorEnd != 0xAA55)
        {
            throw new InvalidEndMarkerException(sectorEnd);
        }
        
        return new BootSector(oemId, bpb, extBpb, bootstrap.ToArray());
    }
    
    private int GetMftRecordByteSize() => ExtBpb.ClustersPerMftRecord > 0
        ? ExtBpb.ClustersPerMftRecord * ClusterByteSize
        : 1 << -ExtBpb.ClustersPerMftRecord; // 2^abs(value)

    private int GetIndexRecordByteSize() => ExtBpb.ClustersPerIndexRecord > 0
        ? ExtBpb.ClustersPerIndexRecord * ClusterByteSize 
        : 1 << -ExtBpb.ClustersPerMftRecord; // 2^abs(value)
}