using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft;

public partial class MasterFileTable
{
    public int MftTableByteSize { get; }
    public int MftRecordByteSize { get; }
    
    private int _sectorByteSize;
    private int _clusterByteSize;
    private readonly FileStream _volumeStream;
    private readonly DataRun[] _mftDataRuns;
    private MftReader? _reader;

    public MasterFileTable(FileStream volumeStream, DataRun[] mftDataRuns, int sectorByteSize, int clusterByteSize, int mftTableByteSize, int mftRecordByteSize)
    {
        _volumeStream = volumeStream;
        _mftDataRuns = mftDataRuns;
        _sectorByteSize = sectorByteSize;
        _clusterByteSize = clusterByteSize;
        MftTableByteSize = mftTableByteSize;
        MftRecordByteSize = mftRecordByteSize;
    }
    
    public MftReader GetReader()
    {
        if (_reader is not null)
        {
            return _reader;
        }

        var reader = new MftReader(this);
        _reader = reader;
        return _reader;
    }
}