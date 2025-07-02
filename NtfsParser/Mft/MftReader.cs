using Microsoft.Win32.SafeHandles;

namespace NtfsParser.Mft;

public class MftReader
{
    public bool CanRead => _stream.CanRead;
    public int BufferSizeInRecords { get => _stream.BufferSizeInRecords; set => _stream.BufferSizeInRecords = value; }
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

            position += run.Offset * _mft.ClusterByteSize;
            var runLengthInRecords = (int)run.Length * _mft.ClusterByteSize / _mft.RecordByteSize;
            mftIndex -= runLengthInRecords;
            if (mftIndex >= 0)
                continue;
            
            position += (runLengthInRecords - -mftIndex) * _mft.RecordByteSize;
            return position;
        }
        
        return -1;
    }
    
    public MftRecord RandomReadAt(int mftIndex)
    {
        var position = MftIndexToStreamOffset(mftIndex);
        var rawRecord = _stream.ReadRawRecordAt(position);
        var parsed = MftRecord.Parse(rawRecord, _mft.SectorByteSize);
        return parsed;
    }
    
    public MftRecord ReadMftRecord()
    {
        var record = _stream.ReadRawRecord();
        var parsed = MftRecord.Parse(record, _mft.SectorByteSize);
        ++_mftIndex;
        return parsed;
    }

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