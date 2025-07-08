using Microsoft.Win32.SafeHandles;

namespace NtfsParser.Mft;

/// <summary>
/// A wrapper for the MftStream class. Use this to read MFT records from the MFT
/// </summary>
public class MftReader
{
    /// <summary>
    /// Is there still any remaining data in the stream
    /// </summary>
    public bool CanRead => _stream.CanRead;
    /// <summary>
    /// Stream's buffer size in MFT records. Increasing the buffer size will reduce the amount of IO calls in sequential reading and will be faster.
    /// You might want to set this value lower when you need to manually move the pointer often to reduce the amount of bytes
    /// written everytime. Default size is 8
    /// </summary>
    public int BufferSize { get => _stream.BufferSize; set => _stream.BufferSize = value; }
    /// <summary>
    /// Current 0-based MFT index
    /// </summary>
    public int MftIndex { get => _mftIndex; set => SetMftIndex(value); }
    private int _mftIndex; // global 0-based index of the current mft record
    
    private readonly MasterFileTable _mft;
    private readonly MftStream _stream;
    
    public MftReader(MasterFileTable mft, SafeFileHandle fileHandle)
    {
        _mft = mft;
        _stream = new MftStream(mft, fileHandle);
        MftIndex = 0;
    }

    private void SetMftIndex(int mftIndex)
    {
        var position = MftIndexToStreamOffset(mftIndex);
        if (position == -1)
            throw new ArgumentException("Provided index is out of MFT range.");
        
        _mftIndex = mftIndex;
        _stream.Position = position;
    }

    private long MftIndexToStreamOffset(int mftIndex)
    {
        if (mftIndex < 0)
            throw new ArgumentException("Index should be greater than 0");
        
        var position = 0L;
        foreach (var run in _mft.MftDataRuns)
        {
            if (run.IsSparse)
                continue;

            position += run.Offset * _mft.ClusterSize;
            var runLengthInRecords = (int)run.Length * _mft.ClusterSize / _mft.RecordSize;
            mftIndex -= runLengthInRecords;
            if (mftIndex >= 0)
                continue;
            
            position += (runLengthInRecords - -mftIndex) * _mft.RecordSize;
            return position;
        }
        
        return -1;
    }
    
    /// <summary>
    /// Reads a single record at the specified index. Use when you need to read entries without moving the index and resetting the buffer
    /// (e.g. when you need to read record's base record or a parent directory record of a file)
    /// </summary>
    /// <param name="mftIndex">0-based index of a record</param>
    /// <returns>Parsed record</returns>
    public MftRecord RandomReadAt(int mftIndex)
    {
        var position = MftIndexToStreamOffset(mftIndex);
        var rawRecord = _stream.ReadRawRecordAt(position);
        var parsed = MftRecord.Parse(rawRecord, _mft.SectorSize);
        return parsed;
    }
    
    /// <summary>
    /// Reads single record at the index and moves forward
    /// </summary>
    /// <returns>Parsed record</returns>
    public MftRecord ReadMftRecord()
    {
        var record = _stream.ReadRawRecord();
        var parsed = MftRecord.Parse(record, _mft.SectorSize);
        ++_mftIndex;
        return parsed;
    }
    
    /// <summary>
    /// Creates an iterator that allows to iterate all records 
    /// </summary>
    /// <param name="options">An object that contains simple filter to ignore specific records</param>
    /// <returns>Iterator</returns>
    public IEnumerable<MftRecord> StartReadingMft(MftIteratorOptions? options = null)
    {
        var ignoreEmpty = options?.IgnoreEmpty ?? false;
        var ignoreUnused = options?.IgnoreUnused ?? false;
        MftIndex = options?.StartFrom ?? 0;
        while (CanRead)
        {
            var record = ReadMftRecord();
            var header = record.RecordHeader;
            if (ignoreEmpty && header.MultiSectorHeader.Signature == MftSignature.Empty
                || ignoreUnused && (header.EntryFlags & MftRecordHeaderFlags.InUse) == 0)
                continue;

            yield return record;
        }
    }
}