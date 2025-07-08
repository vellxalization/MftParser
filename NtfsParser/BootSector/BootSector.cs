namespace NtfsParser.BootSector;

/// <summary>
/// Structure that represents an NTFS boot sector
/// </summary>
/// <param name="OemId">Volume id (ASCII-encoded string "NTFS    ")</param>
/// <param name="Bpb">BIOS parameters block</param>
/// <param name="ExtBpb">Extended BIOS parameters block</param>
/// <param name="Bootstrap">Boot code</param>
public readonly record struct BootSector(ulong OemId, BiosParamsBlock Bpb, ExtendedBpb ExtBpb, byte[] Bootstrap)
{
    /// <summary>
    /// Size of a single sector in bytes
    /// </summary>
    public int SectorSize => Bpb.BytesPerSector;
    /// <summary>
    /// Size of a single cluster in bytes
    /// </summary>
    public int ClusterSize => Bpb.BytesPerSector * Bpb.SectorsPerCluster;
    /// <summary>
    /// Size of a single MFT record in bytes
    /// </summary>
    public int MftRecordSize => GetMftRecordByteSize();
    /// <summary>
    /// Size of a single INDX record in bytes
    /// </summary>
    public int IndexRecordSize => GetIndexRecordByteSize();
    /// <summary>
    /// Byte offset to the start of the volume's MFT
    /// </summary>
    public long MftStartOffset => ExtBpb.MftCluster * ClusterSize;
    
    public static BootSector Parse(Span<byte> rawBootSector)
    {
        var reader = new SpanBinaryReader(rawBootSector);
        var jmp = reader.ReadBytes(3);
        if (jmp is not [0xEB, 0x52, 0x90])
            throw new InvalidJmpException(jmp, reader.Position);
        
        var oemId = reader.ReadUInt64();
        if (oemId != 0x202020205346544E) // "    SFTN"
            throw new InvalidOemIdException(oemId, reader.Position);

        var rawBpb = reader.ReadBytes(25);
        var bpb = BiosParamsBlock.Parse(rawBpb);
        var rawExtBpb = reader.ReadBytes(48);
        var extBpb = ExtendedBpb.Parse(rawExtBpb);
        var bootstrap = reader.ReadBytes(426);
        var sectorEnd = reader.ReadUInt16();
        if (sectorEnd != 0xAA55)
            throw new InvalidEndMarkerException(sectorEnd);
        
        return new BootSector(oemId, bpb, extBpb, bootstrap.ToArray());
    }
    
    private int GetMftRecordByteSize() => ExtBpb.ClustersPerMftRecord > 0
        ? ExtBpb.ClustersPerMftRecord * ClusterSize
        : 1 << -ExtBpb.ClustersPerMftRecord; // 2^abs(value)

    private int GetIndexRecordByteSize() => ExtBpb.ClustersPerIndexRecord > 0
        ? ExtBpb.ClustersPerIndexRecord * ClusterSize 
        : 1 << -ExtBpb.ClustersPerMftRecord; // 2^abs(value)
}