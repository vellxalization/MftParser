using System.Collections.ObjectModel;
using Microsoft.Win32.SafeHandles;
using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft;

public readonly struct MasterFileTable
{
    public MftReader Reader { get; }
    public int RecordByteSize { get; }
    public int SectorByteSize { get; }
    public int ClusterByteSize { get; }
    public ReadOnlyCollection<DataRun> MftDataRuns { get; }
    
    public MasterFileTable(int recordByteSize, int sectorByteSize, int clusterByteSize, SafeFileHandle handle, DataRun[] mftDataRuns)
    {
        SectorByteSize = sectorByteSize;
        ClusterByteSize = clusterByteSize;
        RecordByteSize = recordByteSize;
        MftDataRuns = new(mftDataRuns);
        Reader = new MftReader(this, handle);
    }
}