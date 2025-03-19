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
            if (mftIndex < 0 || mftIndex >= _mft.MftTableByteSize / _mft.MftRecordByteSize)
            {
                throw new ArgumentException("Provided index is out of MFT range.");
            }

            long indexCopy = mftIndex;
            long startOffset = 0;
            for (int i = 0; i < _mft._mftDataRuns.Length; ++i)
            {
                var run = _mft._mftDataRuns[i];
                var runRecordLength = run.Length * _mft._clusterByteSize / _mft.MftRecordByteSize;
                startOffset += run.Offset;
                if (indexCopy - runRecordLength  >= 0) // index is not in this run
                {
                    indexCopy -= runRecordLength;
                    continue;
                }
                
                _mftIndex = mftIndex;
                _currentDataRunRecordLength = runRecordLength;
                _currentDataRunMftByteOffset = startOffset * _mft._clusterByteSize;
                _currentDataRunIndex = i;
                _currentIndexInsideDataRun = (int)indexCopy;
                return;
            }
            
            throw new ArgumentException("Provided index is out of MFT range."); // in theory, this shouldn't ever fire
        }
        
        public MftRecord.MftRecord ReadMftRecord()
        {
            if (!CanRead)
            {
                throw new Exception("End of the MFT is reached."); // TODO: temp
            }
            
            var buffer = new Span<byte>(new byte[_mft.MftRecordByteSize]);
            _mft._volumeStream.ReadExactly(buffer);
            var parsed = MftRecord.MftRecord.Parse(buffer, _mft._sectorByteSize);
            AdvanceForward();
            return parsed;
        }

        public IEnumerable<MftRecord.MftRecord> ReadMftFromTheStart()
        {
            MftIndex = 0;
            while (CanRead)
            {
                yield return ReadMftRecord();
            }
        }
        
        private void AdvanceForward()
        {
            ++_mftIndex;
            ++_currentIndexInsideDataRun;
            if (_currentIndexInsideDataRun < _currentDataRunRecordLength)
            {
                return; // there is still data in current data run
            }

            ++_currentDataRunIndex;
            if (_currentDataRunIndex >= _mft._mftDataRuns.Length)
            {
                return; // no more data runs to read data from
            }
            
            var nextDataRun = _mft._mftDataRuns[_currentDataRunIndex];
            _currentIndexInsideDataRun = 0;
            _currentDataRunMftByteOffset += nextDataRun.Offset * _mft._clusterByteSize;
            _currentDataRunRecordLength = nextDataRun.Length * _mft._clusterByteSize / _mft.MftRecordByteSize;
            _mft._volumeStream.Position = _currentDataRunMftByteOffset;
        }
    }
}