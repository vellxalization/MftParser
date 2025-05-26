using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft;

public partial class MasterFileTable(FileStream volumeStream, DataRun[] mftDataRuns, int sectorByteSize, int clusterByteSize, int sizeInBytes, int recordByteSize)
{
    public MftReader Reader => _reader ?? CreateReader();
    private MftReader? _reader;

    public int SizeInRecords { get; } = sizeInBytes / recordByteSize;
    public int SizeInBytes { get; } = sizeInBytes;
    public int RecordByteSize { get; } = recordByteSize;
    public int SectorByteSize { get; } = sectorByteSize;
    public int ClusterByteSize { get; } = clusterByteSize;

    private readonly FileStream _volumeStream = volumeStream;
    private readonly DataRun[] _mftDataRuns = mftDataRuns;

    private MftReader CreateReader()
    {
        var reader = new MftReader(this);
        _reader = reader;
        return _reader;
    }
}