using System.Collections.ObjectModel;
using Microsoft.Win32.SafeHandles;
using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft;

/// <summary>
/// A struct that contains crucial information about volume's MFT as well as a reader to read data from the MFT
/// </summary>
public readonly struct MasterFileTable
{
    /// <summary>
    /// Mft reader
    /// </summary>
    public MftReader Reader { get; }
    /// <summary>
    /// Size of a single MFT record in bytes
    /// </summary>
    public int RecordSize { get; }
    /// <summary>
    /// Size of a single sector in bytes
    /// </summary>
    public int SectorSize { get; }
    /// <summary>
    /// Size of a single cluster in bytes
    /// </summary>
    public int ClusterSize { get; }
    /// <summary>
    /// Data runs that describe MFT's data location
    /// </summary>
    public ReadOnlyCollection<DataRun> MftDataRuns { get; }
    
    public MasterFileTable(int recordSize, int sectorSize, int clusterSize, DataRun[] mftDataRuns, SafeFileHandle handle)
    {
        SectorSize = sectorSize;
        ClusterSize = clusterSize;
        RecordSize = recordSize;
        MftDataRuns = new(mftDataRuns);
        var mftStream = new MftStream(this, handle);
        Reader = mftStream.Reader;
    }
}