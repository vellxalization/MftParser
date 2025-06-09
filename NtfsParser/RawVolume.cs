using Microsoft.Win32.SafeHandles;
using NtfsParser.Mft;
using NtfsParser.Mft.Attribute;

namespace NtfsParser;

public class RawVolume : IDisposable
{
    public BootSector.BootSector BootSector { get; }
    public MasterFileTable MasterFileTable { get; }
    public MftReader MftReader => MasterFileTable.Reader;
    public VolumeDataReader VolumeReader { get; }
    
    public readonly SafeFileHandle VolumeHandle;

    public RawVolume(char volumeLetter)
    {
        var fileHandle = OpenFile($@"\\.\{volumeLetter}:");
        VolumeHandle = fileHandle;
        var dataReaderStream = new FileStream(VolumeHandle, FileAccess.Read); // will be reused in data reader
        
        var bootSector = ReadBootSector(dataReaderStream);
        BootSector = bootSector;
        
        var mft = CreateMft(dataReaderStream);
        MasterFileTable = mft;

        dataReaderStream.Position = 0;
        VolumeReader = new VolumeDataReader(dataReaderStream, BootSector.SectorByteSize, BootSector.ClusterByteSize, 
            BootSector.IndexRecordByteSize);
    }
    
    private MasterFileTable CreateMft(FileStream volumeStream)
    {
        var mftFile = ReadMftFile(volumeStream);
        var dataAttribute = mftFile.Attributes.First(attribute => attribute.Header.Type == AttributeType.Data);
        var dataRuns = DataRun.ParseDataRuns(dataAttribute.Value);
        var mft = new MasterFileTable(BootSector.MftRecordByteSize, BootSector.SectorByteSize, BootSector.ClusterByteSize, VolumeHandle, dataRuns);
        
        return mft;
    }

    private MftRecord ReadMftFile(FileStream volumeStream)
    {
        // first file of the table is the $MFT
        var buffer = new Span<byte>(new byte[BootSector.MftRecordByteSize]);
        volumeStream.Seek(BootSector.MftStartByteOffset, SeekOrigin.Begin);
        volumeStream.ReadExactly(buffer);
        var parsedMftFile = MftRecord.Parse(buffer, BootSector.SectorByteSize);
        return parsedMftFile;
    }
    
    private BootSector.BootSector ReadBootSector(FileStream volumeStream)
    {
        var rawBootSector = new Span<byte>(new byte[512]);
        volumeStream.Seek(0, SeekOrigin.Begin);
        volumeStream.ReadExactly(rawBootSector);
        var bootSector = NtfsParser.BootSector.BootSector.Parse(rawBootSector);
        return bootSector;
    }
    
    private SafeFileHandle OpenFile(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        var handle = File.OpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        if (!handle.IsInvalid)
            return handle;
        
        handle.Close();
        handle.Dispose();
        throw new InvalidHandleException();
    }

    public void Dispose()
    {
        VolumeHandle.Dispose();
    }
}
public class InvalidHandleException() : Exception("File handle is invalid"); // TODO: i should put this somewhere