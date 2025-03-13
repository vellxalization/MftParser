using NtfsParser.MasterFileTable;
using NtfsParser.MasterFileTable.MftRecord;

namespace NtfsParser;

public class VolumeReader
{
    public int SectorByteSize { get; }
    public int ClusterByteSize { get; }
    public int MftRecordSize { get; }
    public long MftOffset { get; }
    
    private readonly FileStream _stream;

    public VolumeReader(FileStream stream, int sectorByteSize, int clusterByteSize, int mftRecordSize, long mftOffset)
    {
        _stream = stream;
        SectorByteSize = sectorByteSize;
        ClusterByteSize = clusterByteSize;
        MftRecordSize = mftRecordSize;
        MftOffset = mftOffset;
    }
    
    public void SetPosition(long position) => _stream.Seek(position, SeekOrigin.Begin);
    public void SetPositionToMftStart() => _stream.Seek(MftOffset * ClusterByteSize, SeekOrigin.Begin);
    public void SetLcnPosition(int lcn) => _stream.Seek((long)lcn * ClusterByteSize, SeekOrigin.Begin);
    public void SetVcnPosition(int vcn) => _stream.Seek((long)vcn * ClusterByteSize, SeekOrigin.Current);

    public long GetPosition() => _stream.Position;
    
    public Span<byte> ReadBytes(int length)
    {
        var buff = new Span<byte>(new byte[length]);
        _stream.ReadExactly(buff);
        return buff;
    }
    
    public void ReadBytes(byte[] buffer, int offset, int length) => _stream.ReadExactly(buffer, offset, length);
    
    public MftRecord ReadMftRecord()
    {
        var rawRecord = new Span<byte>(new byte[MftRecordSize]);
        _stream.ReadExactly(rawRecord);
        if (rawRecord[..4] is [0, 0, 0, 0])
        {
            return new MftRecord();
        }
        
        var parsedRecord = MftRecord.Parse(rawRecord, SectorByteSize);
        return parsedRecord;
    }
}