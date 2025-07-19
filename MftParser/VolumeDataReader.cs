namespace MftParser;

/// <summary>
/// A wrapper around FileStream for more convenient volume reading. Use this class to read stuff like attribute's data
/// </summary>
/// <param name="volumeStream">Volume's stream</param>
/// <param name="sectorByteSize">Single sector size in bytes</param>
/// <param name="clusterByteSize">Single cluster size in bytes</param>
/// <param name="indexRecordByteSize">Single INDX records size in bytes</param>
public class VolumeDataReader(FileStream volumeStream, int sectorByteSize, int clusterByteSize, int indexRecordByteSize)
{
    public long Position => volumeStream.Position;
    public int SectorByteSize { get; } = sectorByteSize;
    public int ClusterByteSize { get; } = clusterByteSize;
    public int IndexRecordByteSize { get; } = indexRecordByteSize;

    /// <summary>
    /// Sets the current position to the new one using provided strategy
    /// </summary>
    public void SetPosition(long position, SetStrategy strategy)
    {
        switch (strategy)
        {
            case SetStrategy.Byte:
            {
                volumeStream.Seek(position, SeekOrigin.Begin);
                return;
            }
            case SetStrategy.Cluster:
            {
                var newPos = position * ClusterByteSize;
                volumeStream.Seek(newPos, SeekOrigin.Begin);
                return;
            }
        }    
    }
    
    public Span<byte> ReadBytes(int length)
    {
        var buff = new Span<byte>(new byte[length]);
        volumeStream.ReadExactly(buff);
        return buff;
    }
    
    public void ReadBytes(byte[] buffer, int offset, int length) => volumeStream.ReadExactly(buffer, offset, length);
}


public enum SetStrategy
{
    /// <summary>
    /// Provided position will be treated as a byte position (same as just setting stream's position)
    /// </summary>
    Byte,
    /// <summary>
    /// Provided position will be treated as a cluster number (i.e. multiplied by the cluster size)
    /// </summary>
    Cluster,
}
