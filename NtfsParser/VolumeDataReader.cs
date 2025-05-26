namespace NtfsParser;

public class VolumeDataReader(FileStream volumeStream, int sectorByteSize, int clusterByteSize, int indexRecordByteSize)
{
    public long Position => volumeStream.Position;
    public int SectorByteSize { get; } = sectorByteSize;
    public int ClusterByteSize { get; } = clusterByteSize;
    public int IndexRecordByteSize { get; } = indexRecordByteSize;

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
    Byte,
    Cluster,
}
