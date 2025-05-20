namespace NtfsParser;

public class VolumeReader
{
    public long Position => _volumeStream.Position;
    public int SectorByteSize { get; }
    public int ClusterByteSize { get; }
    public int IndexRecordByteSize { get; }
    
    private readonly FileStream _volumeStream;

    public VolumeReader(FileStream volumeStream, int sectorByteSize, int clusterByteSize, int indexRecordByteSize)
    {
        _volumeStream = volumeStream;
        SectorByteSize = sectorByteSize;
        ClusterByteSize = clusterByteSize;
        IndexRecordByteSize = indexRecordByteSize;
    }

    public void SetPosition(long position, SetStrategy strategy)
    {
        switch (strategy)
        {
            case SetStrategy.Byte:
            {
                _volumeStream.Seek(position, SeekOrigin.Begin);
                return;
            }
            case SetStrategy.Cluster:
            {
                var newPos = position * ClusterByteSize;
                _volumeStream.Seek(newPos, SeekOrigin.Begin);
                return;
            }
        }    
    }
    
    public Span<byte> ReadBytes(int length)
    {
        var buff = new Span<byte>(new byte[length]);
        _volumeStream.ReadExactly(buff);
        return buff;
    }
    
    public void ReadBytes(byte[] buffer, int offset, int length) => _volumeStream.ReadExactly(buffer, offset, length);
}

public enum SetStrategy
{
    Byte,
    Cluster,
}
