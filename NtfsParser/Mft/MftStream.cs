using Microsoft.Win32.SafeHandles;
using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft;

public class MftStream
{
    private const int DefaultBufferSizeInRecords = 8;
    
    public bool CanRead => _currentDataRunIndex < _mftBoundaries.Length 
                           && _position + _mftRecordSizeInBytes <= _mftBoundaries[^1].end;
    public int BufferSizeInRecords { get => _buffer.Length; set => SetBufferSize(value); }
    public long Position { get => _position; set => SetPosition(value); }
    
    private readonly SafeFileHandle _volumeHandle;
    private readonly (long start, long end)[] _mftBoundaries;
    private readonly int _mftRecordSizeInBytes;

    private byte[] _randomReadBuffer;
    private byte[] _buffer;
    private int _offsetInBuffer; // current offset in the buffer to read records from
    private int _validDataInBufferSize;
    
    private long _position;
    private int _currentDataRunIndex;

    public MftStream(MasterFileTable mft, SafeFileHandle volumeHandle)
    {
        _volumeHandle = volumeHandle;
        _mftBoundaries = GetMftBoundaries(mft.MftDataRuns, mft.ClusterByteSize);
        _mftRecordSizeInBytes = mft.RecordByteSize;
        _buffer = [];
        _randomReadBuffer = new byte[mft.RecordByteSize];
        Position = _mftBoundaries[0].start;
        SetBufferSize(DefaultBufferSizeInRecords);
    }
    
    private void SetBufferSize(int sizeInRecords)
    {
        if (sizeInRecords < 0)
            throw new ArgumentException("Buffer size must be greater than zero");
        
        sizeInRecords = Math.Clamp(sizeInRecords, 1, int.MaxValue); // having 0-length buffer is the same as having a 1-length buffer so we clamp it to 1
        var sizeInBytes = sizeInRecords * _mftRecordSizeInBytes;
        if (sizeInBytes == _buffer.Length)
            return;
        
        var newBuffer = new byte[sizeInBytes];
        var unreadBytes = _validDataInBufferSize - _offsetInBuffer;
        if (unreadBytes != 0)
        {
            var bytesToCopy = Math.Clamp(unreadBytes, 0, newBuffer.Length);
            Array.Copy(_buffer, _offsetInBuffer, newBuffer, 0, bytesToCopy);
            _validDataInBufferSize = bytesToCopy;
        }
        else
            _validDataInBufferSize = 0;

        _offsetInBuffer = 0;
        _buffer = newBuffer;
    }

    public Span<byte> ReadRawRecordAt(long position)
    {
        _ = ValidatePosition(position);
        RandomAccess.Read(_volumeHandle, _randomReadBuffer, position);
        return _randomReadBuffer.AsSpan();
    }
    
    public Span<byte> ReadRawRecord()
    {
        if (!CanRead)
            throw new EndOfMftException();
        
        return ReadRecordFromBuffer();
    }

    private Span<byte> ReadRecordFromBuffer()
    {
        if (_offsetInBuffer >= _validDataInBufferSize)
            ReadRecordsToBuffer();

        var record = _buffer.AsSpan().Slice(_offsetInBuffer, _mftRecordSizeInBytes);
        Advance();
        return record;
    }
    
    private void ReadRecordsToBuffer()
    {
        InvalidateBuffer();
        var bytesAvailable = _mftBoundaries[_currentDataRunIndex].end - _position;
        var bytesToRead = (int)Math.Clamp(bytesAvailable, 0, _buffer.Length);
        var span = _buffer.AsSpan()[..bytesToRead];
        RandomAccess.Read(_volumeHandle, span, _position);
        _validDataInBufferSize = bytesToRead;
    } 
    
    private void Advance()
    {
        _offsetInBuffer += _mftRecordSizeInBytes;
        _position += _mftRecordSizeInBytes;

        if (_position < _mftBoundaries[_currentDataRunIndex].end) 
            return; // we're still in the current data run
        
        InvalidateBuffer();
        ++_currentDataRunIndex;
        if (!CanRead)
            return; // no more data runs are available
        
        _position = _mftBoundaries[_currentDataRunIndex].start; // we still have more data runs
    }
    
    private void SetPosition(long position)
    {
        if (_position == position)
            return;
        
        var dataRunIndex = ValidatePosition(position);
        var positionDifference = Math.Abs((position - _position));
        if (dataRunIndex != _currentDataRunIndex)
            InvalidateBuffer();
        else if (position > _position && positionDifference < _validDataInBufferSize - _offsetInBuffer)
            _offsetInBuffer += (int)positionDifference; // we moved forward and still can use some of the records in the buffer
        else if (position < _position && positionDifference <= _offsetInBuffer) 
            _offsetInBuffer -= (int)positionDifference; // we moved backwards and can reuse older buffered recrods
        else
            InvalidateBuffer(); // we moved too far and can't reuse any of the buffered records
        
        _currentDataRunIndex = dataRunIndex;
        _position = position;
    }

    private int ValidatePosition(long position)
    {
        for (var i = 0; i < _mftBoundaries.Length; ++i)
        {
            var boundary = _mftBoundaries[i];
            if (position < boundary.start || position > boundary.end)
                continue;
            
            if (position % _mftRecordSizeInBytes != 0)
                throw new ArgumentException("Position must align with the size of MFT record");
            
            return i;
        }
        
        throw new ArgumentException("New position is located outside of the MFT");
    }

    private (long start, long end)[] GetMftBoundaries(ICollection<DataRun> mftDataRuns, int clusterSizeInBytes)
    {
        // we shouldn't EVER have a sparse MFT file. but JUST IN CASE we will skip any sparse blocks inside
        var boundaries = new (long start, long end)[mftDataRuns.Count(run => !run.IsSparse)];
        var start = 0L;
        var counter = 0;
        foreach (var run in mftDataRuns)
        {
            if (run.IsSparse)
                continue;
            
            start += run.Offset * clusterSizeInBytes;
            var end = start + run.Length * clusterSizeInBytes;
            boundaries[counter++] = (start, end);
        }
        
        return boundaries;
    }

    public void InvalidateBuffer()
    {
        _validDataInBufferSize = 0;
        _offsetInBuffer = 0;
    }
}

public class EndOfMftException() : Exception("Tried to read past MFT");