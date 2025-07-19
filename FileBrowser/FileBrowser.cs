using System.Text;
using MftParser;
using MftParser.Mft;
using MftParser.Mft.Attribute;
using MftParser.Mft.ParsedAttributeData;
using MftParser.Mft.ParsedAttributeData.Index;
using FileAttributes = MftParser.Mft.ParsedAttributeData.FileAttributes;

namespace FileBrowser;

public class FileBrowser
{
    private readonly RawVolume _volume;
    private readonly VolumeDataReader _dataReader;
    private readonly MftReader _mftReader;
    private readonly int _indexRecordSize;
    private readonly int _sectorSize;
    private readonly byte[] _rootName;
    
    private readonly Stack<SortedFolder> _pathStack = new();
    private readonly Stack<string> _restoreStack = new();
    private readonly Dictionary<long, SortedFolder> _cache = new(64);
    
    public FileBrowser(RawVolume volume)
    {
        _volume = volume;
        _dataReader = volume.VolumeReader;
        _mftReader = volume.MftReader;
        _indexRecordSize = volume.BootSector.IndexRecordSize;
        _sectorSize = volume.BootSector.SectorSize;
        _rootName = Encoding.Unicode.GetBytes(".");
        InitRoot();
    }
    
    public bool AtRoot => _pathStack.Count == 1;
    public SortedFolder CurrentFolder => _pathStack.Peek();
    public string CurrentPath => PathToString();

    public (bool, string) TryChangeFolder(string[] path)
    {
        _restoreStack.Clear();
        var pathSpan = path.AsSpan(); 
        if (path[0] == $"{_volume.VolumeLetter}:")
        {
            while (!AtRoot)
            {
                _restoreStack.Push(CurrentFolder.Folder.Name.Name.ToString());
                _ = TryMoveUp();
            }

            pathSpan = pathSpan[1..];
        }

        for (var i = 0; i < pathSpan.Length; i++)
        {
            var folderName = pathSpan[i];
            if (string.IsNullOrWhiteSpace(folderName))
                continue;

            if (folderName is "..")
            {
                var cur = CurrentFolder.Folder.Name.Name.ToString();
                var result = TryMoveUp();
                if (result.Item1)
                {
                    _restoreStack.Push(cur);
                    continue;
                }
            
                RestoreOriginalPath(pathSpan[..i]);
                return result;
            }
            else
            {
                var result = TryMoveDown(folderName);
                if (result.Item1) 
                    continue;
            
                RestoreOriginalPath(pathSpan[..i]);
                return result;
            }
        }

        return (true, string.Empty);
    }

    private void RestoreOriginalPath(Span<string> badPath)
    {
        for (var i = badPath.Length - 1; i >= 0; i--)
        {
            var pathPart = badPath[i];
            if (pathPart is "..")
                _ = TryMoveDown(_restoreStack.Pop());
            else
                TryMoveUp();
        }

        while (_restoreStack.TryPop(out var pop))
            TryMoveDown(pop);
    }
    
    private string PathToString()
    {
        var sb = new StringBuilder();
        foreach (var folder in _pathStack.Reverse())
        {
            sb.Append(folder.Folder.Name.Name);
            sb.Append('/');
        }
        
        if (_pathStack.Count > 1)
            sb.Remove(sb.Length - 1, 1); // remove last slash if we're below the root
        return sb.ToString();
    }

    public (bool, string) TryMoveUp()
    {
        if (_pathStack.Count == 1) // can't go above the root
            return (false, "Can't move above of the root");
        
        var folder = _pathStack.Pop();
        AddToCache(folder);
        return (true, string.Empty);
    }

    private void AddToCache(SortedFolder folder)
    {
        if (_cache.Count < _cache.Capacity)
        {
            _cache[folder.Folder.MftIndex] = folder;
            return;
        }

        var oldestElement = _cache.First();
        _cache.Remove(oldestElement.Key);
        _cache[folder.Folder.MftIndex] = folder;
    }
    
    public (bool, string) TryMoveDown(string folderName)
    {
        var folder = CurrentFolder.TryFind(folderName);
        if (folder is null)
            return (false, $"\"{folderName}\" doesn't exist");

        var data = folder.Value.Data;
        if ((data.Flags & FileAttributes.Directory) == 0 && (data.Flags & FileAttributes.DirectoryAlt) == 0)
            return (false, $"\"{folderName}\" is not a directory");

        var newFolder = GetFolderAt((int)folder.Value.MftIndex);
        _pathStack.Push(newFolder);
        return (true, string.Empty);
    }

    private SortedFolder GetFolderAt(int mftIndex)
    {
        if (_cache.TryGetValue(mftIndex, out var data))
            return data;
        
        var mftRecord = _mftReader.RandomReadAt(mftIndex);
        var fn = mftRecord.Attributes.First(attr => attr.Header.Type == AttributeType.FileName)
            .GetAttributeData(_dataReader).ToFileName();
        var index = GetIndexFromMftRecord(mftRecord, mftIndex);
        var newFolder = SortedFolder.Create(fn, mftIndex, index.root, index.allocation);
        return newFolder;
    }
    
    private void InitRoot()
    {
        var root = GetRootDirectory();
        var rootIndex = GetIndexFromMftRecord(root.root, root.rootIndex);
        var fn = root.root.Attributes.First(attr => attr.Header.Type == AttributeType.FileName)
            .GetAttributeData(_dataReader).ToFileName();
        fn = fn with { Name = new UnicodeName(Encoding.Unicode.GetBytes($"{_volume.VolumeLetter}:")) };
        var folder = SortedFolder.Create(fn, root.rootIndex, rootIndex.root, rootIndex.allocation);
        _pathStack.Push(folder);
    }
    
    private (MftRecord root, int rootIndex) GetRootDirectory()
    {
        var options = new MftIteratorOptions()
        {
            IgnoreUnused = true,
            IgnoreEmpty = true,
            StartFrom = 0
        };
        const int limit = 50; // something is seriously wrong if we can't find the root in the first 50 entries
        foreach (var record in _mftReader.StartReadingMft(options))
        {
            var index = _mftReader.MftIndex;
            if (index > limit)
                break;

            var fileNameAttr = record.Attributes.FirstOrDefault(attr => attr.Header.Type == AttributeType.FileName);
            if (fileNameAttr == default)
                continue;

            var fileNameValue = fileNameAttr.GetAttributeData(_dataReader).ToFileName();
            if (fileNameValue.Name.Name.SequenceEqual(_rootName))
                return (record, index);
        }
        
        throw new Exception("""Couldn't find the root (".") of the $I30 index""");
    }
    
    private (IndexRoot root, IndexAllocation? allocation) GetIndexFromMftRecord(in MftRecord record, int recordIndex)
    {
        var attrListAttribute = record.Attributes.FirstOrDefault(attr => attr.Header.Type == AttributeType.AttributeList);
        if (attrListAttribute != default)
        {
            var attrList = attrListAttribute.GetAttributeData(_dataReader).ToAttributeList();
            return GetIndexFromAttributeList(record, attrList, recordIndex);
        }
        
        var rootData = record.Attributes.First(attr => attr.Header.Type == AttributeType.IndexRoot)
            .GetAttributeData(_dataReader).ToIndexRoot();
        if (!rootData.NodeHeader.HasChildren)
            return (rootData, null);
        
        var allocData = record.Attributes.First(attr => attr.Header.Type == AttributeType.IndexAllocation)
            .GetAttributeData(_dataReader).ToIndexAllocation(_indexRecordSize, _sectorSize);
        return (rootData, allocData);
    }
    
    private (IndexRoot root, IndexAllocation? allocation) GetIndexFromAttributeList(in MftRecord baseRecord,
        in AttributeList attrList, int baseRecordIndex)
    {
        var rootListEntry = attrList.Entries.First(entry => entry.AttributeType == AttributeType.IndexRoot);
        var rootMftRecord = rootListEntry.RecordReference.MftIndex == baseRecordIndex
            ? baseRecord
            : _mftReader.RandomReadAt((int)rootListEntry.RecordReference.MftIndex);
        
        var rootData = rootMftRecord.Attributes.First(attr => attr.Header.Type == AttributeType.IndexRoot)
            .GetAttributeData(_dataReader).ToIndexRoot();
        if (!rootData.NodeHeader.HasChildren)
            return (rootData, null);
        
        var allocListEntry = attrList.Entries.First(entry => entry.AttributeType == AttributeType.IndexAllocation);
        var allocMftIndex = allocListEntry.RecordReference.MftIndex;
        MftRecord allocMftRecord;
        if (allocMftIndex == baseRecordIndex)
            allocMftRecord = baseRecord;
        else if (allocMftIndex == rootListEntry.RecordReference.MftIndex)
            allocMftRecord = rootMftRecord;
        else
            allocMftRecord = _mftReader.RandomReadAt((int)allocMftIndex);
        
        var allocData = allocMftRecord.Attributes.First(attr => attr.Header.Type == AttributeType.IndexAllocation)
            .GetAttributeData(_dataReader).ToIndexAllocation(_indexRecordSize, _sectorSize);
        return (rootData, allocData);
    }
}