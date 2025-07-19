using MftParser.Mft.Decompression;

namespace MftParser.Mft.Attribute;

public interface IDataReadStrategy
{
    public RawAttributeData GetDataFromDataRuns(VolumeDataReader reader, in MftAttribute attribute);
}

public class ResidentStrategy : IDataReadStrategy
{
    public RawAttributeData GetDataFromDataRuns(VolumeDataReader reader, in MftAttribute attribute) 
        => new(attribute.Value);
}

public class NonresidentNoSparseStrategy : IDataReadStrategy
{
    public RawAttributeData GetDataFromDataRuns(VolumeDataReader reader, in MftAttribute attribute)
    {
        if (attribute.Header.Nonresident.ActualSize == 0)
            return new RawAttributeData([]);
        
        var dataRuns = DataRun.ParseDataRuns(attribute.Value);
        var data = new byte[attribute.Header.Nonresident.AllocatedSize];
        long clusterOffset = 0;
        int insertOffset = 0;
        foreach (var run in dataRuns)
        {
            clusterOffset += run.Offset;
            reader.SetPosition(clusterOffset, SetStrategy.Cluster);
            var readSize = (int)run.Length * reader.ClusterByteSize;
            reader.ReadBytes(data, insertOffset, readSize);
            insertOffset += readSize;
        }
        
        return new RawAttributeData(data[..(int)(attribute.Header.Nonresident.ActualSize)]);
    }
}

public class NonresidentSparseStrategy : IDataReadStrategy
{
    public RawAttributeData GetDataFromDataRuns(VolumeDataReader reader, in MftAttribute attribute)
    {
        if (attribute.Header.Nonresident.ActualSize == 0)
            return new RawAttributeData([]);
        
        var dataRuns = DataRun.ParseDataRuns(attribute.Value);
        var data = new byte[attribute.Header.Nonresident.AllocatedSize];
        long clusterOffset = 0;
        int insertOffset = 0;
        foreach (var run in dataRuns)
        {
            var readSize = (int)run.Length * reader.ClusterByteSize;
            if (run.IsSparse)
            {
                insertOffset += readSize;
                continue;
            }
            
            clusterOffset += run.Offset;
            reader.SetPosition(clusterOffset, SetStrategy.Cluster);
            reader.ReadBytes(data, insertOffset, readSize);
            insertOffset += readSize;
        }

        return new RawAttributeData(data[..(int)(attribute.Header.Nonresident.ActualSize)]);
    }
}

public class NonresidentCompressedStrategy : IDataReadStrategy
{
    public RawAttributeData GetDataFromDataRuns(VolumeDataReader reader, in MftAttribute attribute)
    {
        if (attribute.Header.Nonresident.ActualSize == 0)
            return new RawAttributeData([]);
        
        var dataRuns = DataRun.ParseDataRuns(attribute.Value);
        var data = new byte[attribute.Header.Nonresident.AllocatedClustersSize];
        long clusterOffset = 0;
        int insertOffset = 0;
        foreach (var run in dataRuns)
        {
            var readSize = (int)run.Length * reader.ClusterByteSize;
            if (run.IsSparse)
                continue;
            
            clusterOffset += run.Offset;
            reader.SetPosition(clusterOffset, SetStrategy.Cluster);
            reader.ReadBytes(data, insertOffset, readSize);
            insertOffset += readSize;
        }

        var compressionUnitSizeCluster = 1 << attribute.Header.Nonresident.CompressionUnitSize; // 2^compUnitSize
        var clusterSizeByte = reader.ClusterByteSize;
        var compressedData = new CompressedData(data, dataRuns, compressionUnitSizeCluster, clusterSizeByte);
        var decompressedData = DataDecompressor.Decompress(compressedData, (int)attribute.Header.Nonresident.AllocatedSize);
        
        return new RawAttributeData(decompressedData[..(int)attribute.Header.Nonresident.ActualSize]);
    }
}