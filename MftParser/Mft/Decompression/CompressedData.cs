using MftParser.Mft.Attribute;

namespace MftParser.Mft.Decompression;

/// <summary>
/// A struct that represents compressed data
/// </summary>
public readonly struct CompressedData
{
    /// <summary>
    /// Raw compressed data
    /// </summary>
    public byte[] Data { get; init; }
    /// <summary>
    /// Compression units
    /// </summary>
    public CompressionUnit[] CompressionUnits { get; init; }
    /// <summary>
    /// Size of a single compression unit in clusters
    /// </summary>
    public int CompressionUnitSize { get; init; }
    /// <summary>
    /// Size of a single cluster in bytes
    /// </summary>
    public int ClusterSize { get; init; }

    public CompressedData(byte[] data, DataRun[] dataRuns, int compressionUnitSize, int clusterSize)
    {
        CompressionUnitSize = compressionUnitSize;
        ClusterSize = clusterSize;
        Data = data;
        CompressionUnits = MapDataRuns(dataRuns, compressionUnitSize, clusterSize);
    }

    private CompressionUnit[] MapDataRuns(DataRun[] dataRuns, int compressionUnitSizeCluster, int clusterSizeByte)
    {
        var amount = (int)Math.Ceiling((dataRuns.Sum(run => run.Length) / (float)compressionUnitSizeCluster));
        var units = new CompressionUnit[amount];
        var rangeStart = 0;
        var unitIndex = 0;
        var runIndex = 0;
        DataRun run;
        for (; runIndex < dataRuns.Length; ++runIndex)
        {
            run = dataRuns[runIndex];
            Skip:
            var leftover = run.IsSparse ? SparseScenario() : DataScenario();
            if (leftover is not null)
            {
                run = leftover.Value;
                goto Skip;
            }
        }
        
        DataRun? SparseScenario()
        {
            if (run.Length < compressionUnitSizeCluster)
                throw new DecompressionException();
            
            units[unitIndex++] = CompressionUnit.SparseUnit;
            return run.Length == compressionUnitSizeCluster ? null : run with { Length = run.Length - compressionUnitSizeCluster };
        }

        DataRun? DataScenario()
        {
            if (run.Length < compressionUnitSizeCluster)
                return DeepDataScenario();
            
            units[unitIndex++] = new CompressionUnit()
            {
                Type = UnitType.Uncompressed,
                Range = new(rangeStart, rangeStart += compressionUnitSizeCluster * clusterSizeByte)
            };
            
            return run.Length == compressionUnitSizeCluster ? null : run with { Length = run.Length - compressionUnitSizeCluster };
        }

        DataRun? DeepDataScenario()
        {
            var dataLength = (int)run.Length;
            var isCompressed = false;
            DataRun? returnValue = null;
            while (++runIndex < dataRuns.Length)
            {
                run = dataRuns[runIndex];
                if (run.IsSparse)
                {
                    if (dataLength + run.Length < compressionUnitSizeCluster && runIndex < dataRuns.Length - 1)
                        throw new DecompressionException();

                    isCompressed = true;
                    returnValue = dataLength + run.Length > compressionUnitSizeCluster 
                        ? run with { Length = run.Length - (compressionUnitSizeCluster - dataLength) }
                        : null;
                    break;
                }

                if (dataLength + run.Length < compressionUnitSizeCluster)
                {
                    dataLength += (int)run.Length;
                    continue;
                }
                
                returnValue = dataLength + run.Length > compressionUnitSizeCluster 
                    ? run with { Length = run.Length - (compressionUnitSizeCluster - dataLength) }
                    : null;
                break;
            }
            
            units[unitIndex++] = new CompressionUnit()
            {
                Type = isCompressed ? UnitType.Compressed : UnitType.Uncompressed,
                Range = new(rangeStart, rangeStart += dataLength * clusterSizeByte)
            };
            return returnValue;
        }

        return units;
    }
}

public readonly struct CompressionUnit
{
    public static CompressionUnit SparseUnit => new() { Type = UnitType.Sparse, Range = default};
    public UnitType Type { get; init; }
    public Range Range { get; init; }
}

public enum UnitType
{
    Uncompressed,
    Compressed,
    Sparse
}