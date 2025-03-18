using NtfsParser.MasterFileTable;
using NtfsParser.MasterFileTable.Attribute;
using NtfsParser.MasterFileTable.MftRecord;

namespace NtfsParser;

public class VolumeReader
{
    public int SectorByteSize { get; }
    public int ClusterByteSize { get; }
    public int MftRecordSize { get; }

    private DataRun[] _mftDataRuns;
    
    private readonly FileStream _stream;

    public VolumeReader(FileStream stream, int sectorByteSize, int clusterByteSize, int mftRecordSize, DataRun[] mftDataRuns)
    {
        _stream = stream;
        SectorByteSize = sectorByteSize;
        ClusterByteSize = clusterByteSize;
        MftRecordSize = mftRecordSize;
        _mftDataRuns = mftDataRuns;
    }

    public void SetPosition(long position, SetStrategy strategy)
    {
        switch (strategy)
        {
            case SetStrategy.Byte:
            {
                _stream.Seek(position, SeekOrigin.Begin);
                return;
            }
            case SetStrategy.Cluster:
            {
                var newPos = position * ClusterByteSize;
                _stream.Seek(newPos, SeekOrigin.Begin);
                return;
            }
            case SetStrategy.MftRecord:
            {
                var newPos = CalculateMftRecordByteOffset(position);
                _stream.Seek(newPos, SeekOrigin.Begin);
                return;
            }
        }    
    }

    private long CalculateMftRecordByteOffset(long mftIndex)
    {
        if (mftIndex < 0)
        {
            throw new ArgumentException("Provided index is out of range of the MFT");
        }
        
        long startOffset = 0;
        foreach (var run in _mftDataRuns)
        {
            startOffset += (long)run.Offset;
            var numberOfMftEntries = (long)run.Length * ClusterByteSize / MftRecordSize;
            if (mftIndex <= numberOfMftEntries)
            {
                return startOffset * ClusterByteSize + mftIndex * MftRecordSize;
            }
            
            mftIndex -= (int)numberOfMftEntries;
        }

        throw new ArgumentException("Provided index is out of range of the MFT");
    }
    
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

    public IEnumerable<MftRecord> ReadMftRecords()
    {
        long startOffset = 0;
        foreach (var dataRun in _mftDataRuns)
        {
            startOffset += dataRun.Offset;
            _stream.Seek(startOffset * ClusterByteSize, SeekOrigin.Begin);
            var length = (int)dataRun.Length * ClusterByteSize / MftRecordSize;
            for (int i = 0; i < length; ++i)
            {
                yield return ReadMftRecord();
            }
        }
    }
}

public enum SetStrategy
{
    Byte,
    MftRecord,
    Cluster,
}
