using Microsoft.Win32.SafeHandles;
using NtfsParser.MasterFileTable.Attribute;
using NtfsParser.MasterFileTable.MftRecord;
using NtfsParser.MasterFileTable.ParsedAttributeData;

namespace NtfsParser;

public class RawVolume : IDisposable
{
    public BootSector.BootSector? BootSector { get; private set; }
    
    private FileStream? _stream;
    private char _volumeLetter;
    private SafeFileHandle? _volumeHandle;

    public RawVolume(char volumeLetter)
    {
        _volumeLetter = volumeLetter;
    }
    
    public VolumeReader? Initialize()
    {
        var fileHandle = OpenFile($@"\\.\{_volumeLetter}:");
        if (fileHandle is null)
        {
            return null;
        }
        
        _volumeHandle = fileHandle;
        _stream = new FileStream(_volumeHandle, FileAccess.Read);
        var bootSector = ReadBootSector();
        if (bootSector is null)
        {
            return null;
        }

        BootSector = bootSector;
        var mftDataRuns = GetMftDataRuns(bootSector.Value.ExtBpb.LogicalClusterForMft * bootSector.Value.GetClusterByteSize(),
            bootSector.Value.GetMftRecordByteSize(), bootSector.Value.Bpb.BytesPerSector);
        var reader = new VolumeReader(_stream, bootSector.Value.Bpb.BytesPerSector, 
            bootSector.Value.GetClusterByteSize(), bootSector.Value.GetMftRecordByteSize(), mftDataRuns);
        return reader;
    }

    private DataRun[] GetMftDataRuns(long mftByteOffset, int mftRecordByteSize, int sectorByteSize)
    {
        _stream!.Seek(mftByteOffset, SeekOrigin.Begin);
        var buffer = new Span<byte>(new byte[mftRecordByteSize]);
        _stream.ReadExactly(buffer);
        var parsed = MftRecord.Parse(buffer, sectorByteSize);
        var dataAttribute = parsed.Attributes.First(attr => attr.Header.Type == AttributeType.Data);
        var rawDataRuns = dataAttribute.Value;
        
        return DataRun.ParseDataRuns(rawDataRuns);
    }
    
    private BootSector.BootSector? ReadBootSector()
    {
        if (_stream is null)
        {
            return null;
        }
        
        var rawBootSector = new Span<byte>(new byte[512]);
        _stream.Seek(0, SeekOrigin.Begin);
        _stream.ReadExactly(rawBootSector);
        try
        {
            var bootSector = NtfsParser.BootSector.BootSector.Parse(rawBootSector);
            return bootSector;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }
    
    private SafeFileHandle? OpenFile(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        try
        {
            var handle = File.OpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (!handle.IsInvalid)
            {
                return handle;
            }
            
            handle.Close();
            handle.Dispose();
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error opening file:");
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public void Dispose()
    {
        _volumeHandle?.Dispose();
        _stream?.Dispose();
    }
}