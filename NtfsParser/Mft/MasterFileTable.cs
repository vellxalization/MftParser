using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft;

public partial class MasterFileTable
{
    public MftReader Reader => _reader ?? CreateReader();
    private MftReader? _reader;

    public int SizeInRecords { get; }
    public int SizeInBytes { get; }
    public int RecordByteSize { get; }
    public int SectorByteSize { get; }
    public int ClusterByteSize { get; }

    private readonly FileStream _volumeStream;
    private readonly DataRun[] _mftDataRuns;

    public MasterFileTable(FileStream volumeStream, DataRun[] mftDataRuns, int sectorByteSize, int clusterByteSize, int sizeInBytes, int recordByteSize)
    {
        _volumeStream = volumeStream;
        _mftDataRuns = mftDataRuns;
        SectorByteSize = sectorByteSize;
        SizeInRecords = sizeInBytes / recordByteSize;
        ClusterByteSize = clusterByteSize;
        SizeInBytes = sizeInBytes;
        RecordByteSize = recordByteSize;
    }

    private MftReader CreateReader()
    {
        var reader = new MftReader(this);
        _reader = reader;
        return _reader;
    }
}