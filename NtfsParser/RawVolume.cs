using Microsoft.Win32.SafeHandles;
using NtfsParser.Mft;
using NtfsParser.Mft.Attribute;
using NtfsParser.Mft.MftRecord;

namespace NtfsParser;

public class RawVolume : IDisposable
{
    public BootSector.BootSector? BootSector { get; private set; }
    public MasterFileTable MasterFileTable { get; private set; }
    public MasterFileTable.MftReader MftReader => MasterFileTable.GetReader();
    public VolumeReader VolumeReader { get; }
    
    private SafeFileHandle _volumeHandle;

    public RawVolume(char volumeLetter)
    {
        var fileHandle = OpenFile($@"\\.\{volumeLetter}:");
        _volumeHandle = fileHandle;
        var mft = CreateMft();
        var volumeReader = CreateVolumeReader();
        MasterFileTable = mft;
        VolumeReader = volumeReader;
    }

    private VolumeReader CreateVolumeReader()
    {
        var volumeStream = new FileStream(_volumeHandle, FileAccess.Read);
        if (BootSector is null)
        {
            volumeStream.Seek(0, SeekOrigin.Begin);
            var bootSector = ReadBootSector(volumeStream);
            BootSector = bootSector;
        }
        
        var volumeReader = new VolumeReader(volumeStream, BootSector.Value.SectorByteSize, 
            BootSector.Value.ClusterByteSize, BootSector.Value.IndexRecordByteSize);
        return volumeReader;
    }
    
    private MasterFileTable CreateMft()
    {
        var volumeStream = new FileStream(_volumeHandle, FileAccess.Read);
        if (BootSector is null)
        {
            volumeStream.Seek(0, SeekOrigin.Begin);
            var bootSector = ReadBootSector(volumeStream);
            BootSector = bootSector;
        }

        var mftFile = ReadMftFile(volumeStream);
        var dataAttribute = mftFile.Attributes.First(attribute => attribute.Header.Type == AttributeType.Data);
        var dataRuns = DataRun.ParseDataRuns(dataAttribute.Value);
        var mft = new MasterFileTable(volumeStream, dataRuns, BootSector.Value.SectorByteSize, BootSector.Value.ClusterByteSize,
            (int)dataAttribute.Header.Nonresident.ValidDataSize, BootSector.Value.MftRecordByteSize);
        return mft;
    }

    private MftRecord ReadMftFile(FileStream volumeStream)
    {
        // first file of the table is the $MFT
        var buffer = new Span<byte>(new byte[BootSector!.Value.MftRecordByteSize]);
        volumeStream.Seek(BootSector.Value.MftStartByteOffset, SeekOrigin.Begin);
        volumeStream.ReadExactly(buffer);
        var parsedMftFile = MftRecord.Parse(buffer, BootSector.Value.SectorByteSize);
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
        {
            return handle;
        }
        
        handle.Close();
        handle.Dispose();
        throw new Exception("Handle is invalid"); // TODO: temp
    }

    public void Dispose()
    {
        _volumeHandle.Dispose();
    }
}