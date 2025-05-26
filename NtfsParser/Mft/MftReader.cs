namespace NtfsParser.Mft;

public partial class MasterFileTable
{
    public class MftReader
    {
        public bool CanRead => _currentDataRunIndex < _mft._mftDataRuns.Length && _currentIndexInsideDataRun < _currentDataRunRecordLength;
        public int MftIndex { get => _mftIndex; set => SetMftIndex(value); }
        private MasterFileTable _mft;

        private int _mftIndex; // global 0-based index of the current mft record
        private long _currentDataRunMftByteOffset; // byte offset to the start of the current data run in the volume's filestream
        private int _currentDataRunIndex; // current data run index in the _mftDataRuns
        private long _currentDataRunRecordLength; // number of mft records stored in current data run
        private int _currentIndexInsideDataRun; // 0-based index of the current mft record relative to current data run
        
        internal MftReader(MasterFileTable mft)
        {
            _mft = mft;
            MftIndex = 0;
        }

        private void SetMftIndex(int mftIndex)
        {
            if (mftIndex < 0 || mftIndex >= _mft.SizeInRecords)
                throw new ArgumentException("Provided index is out of MFT range.");

            long indexCopy = mftIndex;
            long startOffset = 0;
            for (int i = 0; i < _mft._mftDataRuns.Length; ++i)
            {
                var run = _mft._mftDataRuns[i];
                var runRecordLength = run.Length * _mft.ClusterByteSize / _mft.RecordByteSize;
                startOffset += run.Offset;
                if (indexCopy - runRecordLength  >= 0) // index is not in this run
                {
                    indexCopy -= runRecordLength;
                    continue;
                }
                
                _mftIndex = mftIndex;
                _currentDataRunRecordLength = runRecordLength;
                _currentDataRunMftByteOffset = startOffset * _mft.ClusterByteSize;
                _currentDataRunIndex = i;
                _currentIndexInsideDataRun = (int)indexCopy;
                var streamBytePosition = _currentDataRunMftByteOffset + _currentIndexInsideDataRun * _mft.RecordByteSize;
                _mft._volumeStream.Seek(streamBytePosition, SeekOrigin.Begin);
                return;
            }
            
            throw new ArgumentException("Provided index is out of MFT range."); // in theory, this shouldn't ever fire
        }
        
        public MftRecord ReadMftRecord()
        {
            if (!CanRead)
                throw new EndOfMftException();
            
            var buffer = new Span<byte>(new byte[_mft.RecordByteSize]);
            _mft._volumeStream.ReadExactly(buffer);
            var parsed = MftRecord.Parse(buffer, _mft.SectorByteSize);
            AdvanceForward();
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
        
        private void AdvanceForward()
        {
            ++_mftIndex;
            if (++_currentIndexInsideDataRun < _currentDataRunRecordLength)
                return; // there is still data in current data run
            
            if (++_currentDataRunIndex >= _mft._mftDataRuns.Length)
                return; // no more data runs to read data from
            
            var nextDataRun = _mft._mftDataRuns[_currentDataRunIndex];
            _currentIndexInsideDataRun = 0;
            _currentDataRunMftByteOffset += nextDataRun.Offset * _mft.ClusterByteSize;
            _currentDataRunRecordLength = nextDataRun.Length * _mft.ClusterByteSize / _mft.RecordByteSize;
            _mft._volumeStream.Seek(_currentDataRunMftByteOffset, SeekOrigin.Begin);
        }
    }

    public class EndOfMftException() : Exception("Tried to read past MFT");
}