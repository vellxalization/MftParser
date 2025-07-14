using Microsoft.Win32.SafeHandles;
using NtfsParser.Mft;
using NtfsParser.Mft.Attribute;

namespace NtfsParser;

/// <summary>
/// A struct that represents an NTFS volume
/// </summary>
public class RawVolume : IDisposable
{
    /// <summary>
    /// Volume's bootsector
    /// </summary>
    public BootSector.BootSector BootSector { get; }
    /// <summary>
    /// Volume's MFT
    /// </summary>
    public MasterFileTable MasterFileTable { get; }
    /// <summary>
    /// MFT reader
    /// </summary>
    public MftReader MftReader => MasterFileTable.Reader;
    /// <summary>
    /// Data reader
    /// </summary>
    public VolumeDataReader VolumeReader { get; }
    /// <summary>
    /// Volume's letter
    /// </summary>
    public char VolumeLetter { get; }
    
    private readonly SafeFileHandle _volumeHandle;

    public RawVolume(char volumeLetter)
    {
        var fileHandle = OpenFile($@"\\.\{volumeLetter}:");
        VolumeLetter = volumeLetter;
        _volumeHandle = fileHandle;
        var dataReaderStream = new FileStream(_volumeHandle, FileAccess.Read); // will be reused in data reader
        
        var bootSector = ReadBootSector(dataReaderStream);
        BootSector = bootSector;
        
        var mft = CreateMft(dataReaderStream);
        MasterFileTable = mft;

        dataReaderStream.Position = 0;
        VolumeReader = new VolumeDataReader(dataReaderStream, BootSector.SectorSize, BootSector.ClusterSize, 
            BootSector.IndexRecordSize);
    }
    
    private MasterFileTable CreateMft(FileStream volumeStream)
    {
        var mftFile = ReadMftFile(volumeStream);
        var dataAttribute = mftFile.Attributes.First(attribute => attribute.Header.Type == AttributeType.Data);
        var dataRuns = DataRun.ParseDataRuns(dataAttribute.Value);
        var mft = new MasterFileTable(BootSector.MftRecordSize, BootSector.SectorSize, BootSector.ClusterSize, dataRuns, _volumeHandle);
        
        return mft;
    }

    private MftRecord ReadMftFile(FileStream volumeStream)
    {
        // first file of the table is the $MFT
        var buffer = new Span<byte>(new byte[BootSector.MftRecordSize]);
        volumeStream.Seek(BootSector.MftStartOffset, SeekOrigin.Begin);
        volumeStream.ReadExactly(buffer);
        var parsedMftFile = MftRecord.Parse(buffer, BootSector.SectorSize);
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
        _volumeHandle.Dispose();
    }
}
public class InvalidHandleException() : Exception("File handle is invalid"); // TODO: i should put this somewhere