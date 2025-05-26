using Microsoft.Win32.SafeHandles;
using NtfsParser.Mft;
using NtfsParser.Mft.Attribute;

namespace NtfsParser;

public class RawVolume : IDisposable
{
    public BootSector.BootSector BootSector { get; private set; }
    public MasterFileTable MasterFileTable { get; private set; }
    public MasterFileTable.MftReader MftReader => MasterFileTable.Reader;
    public VolumeDataReader VolumeReader { get; }
    
    private SafeFileHandle _volumeHandle;

    public RawVolume(char volumeLetter)
    {
        var fileHandle = OpenFile($@"\\.\{volumeLetter}:");
        _volumeHandle = fileHandle;

        var volumeReader = CreateVolumeReader();
        VolumeReader = volumeReader;
        var mft = CreateMft();
        MasterFileTable = mft;
    }

    private VolumeDataReader CreateVolumeReader()
    {
        // this method will be called first in ctor so we also use it to create boot sector since we need
        // a filestream
        var volumeStream = new FileStream(_volumeHandle, FileAccess.Read);
        var bootSector = ReadBootSector(volumeStream);
        BootSector = bootSector;
        
        var volumeReader = new VolumeDataReader(volumeStream, BootSector.SectorByteSize, 
            BootSector.ClusterByteSize, BootSector.IndexRecordByteSize);
        
        return volumeReader;
    }
    
    private MasterFileTable CreateMft()
    {
        var volumeStream = new FileStream(_volumeHandle, FileAccess.Read, BootSector.MftRecordByteSize * 8);
        var mftFile = ReadMftFile(volumeStream);
        var dataAttribute = mftFile.Attributes.First(attribute => attribute.Header.Type == AttributeType.Data);
        var dataRuns = DataRun.ParseDataRuns(dataAttribute.Value);
        var mft = new MasterFileTable(volumeStream, dataRuns, BootSector.SectorByteSize, BootSector.ClusterByteSize,
            (int)dataAttribute.Header.Nonresident.AllocatedSizeByte, BootSector.MftRecordByteSize);
        
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

    public void Dispose() => _volumeHandle.Dispose();
}
public class InvalidHandleException() : Exception("File handle is invalid"); // TODO: i should put this somewhere