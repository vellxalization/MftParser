using System.Diagnostics;
using System.Text;
using MftParser;
using MftParser.Mft;
using MftParser.Mft.Attribute;
using MftParser.Mft.ParsedAttributeData;
using MftParser.Mft.ParsedAttributeData.Index;
using FileAttributes = MftParser.Mft.ParsedAttributeData.FileAttributes;

namespace FileSearcher;

public class DfsFileSearcher : IDisposable
{
    private readonly RawVolume _volume;
    private readonly int _sectorSize;
    private readonly int _indexRecordSize;
    private readonly VolumeDataReader _dataReader;
    private readonly MftReader _mftReader;
    private readonly byte[] _rootName;
    
    public DfsFileSearcher(RawVolume volume)
    {
        var sectorSize = volume.BootSector.SectorSize;
        var indexRecordSize = volume.BootSector.IndexRecordSizeInBytes;
        _volume = volume;
        _sectorSize = sectorSize;
        _indexRecordSize = indexRecordSize;
        _dataReader = volume.VolumeReader;
        _mftReader = volume.MftReader;
        _rootName = Encoding.Unicode.GetBytes(".");
    }

    public void FindSingleMatch(string name)
    {
        var nameBytes = Encoding.Unicode.GetBytes(name);
        (MftRecord root, long rootIndex) root;
        try
        {
            root = GetRootDirectory();
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
            return;
        }

        var fn = new FileName()
        {
            Name = new UnicodeName(Encoding.Unicode.GetBytes([_volume.VolumeLetter])),
        };
        var path = new Stack<FileName>();
        path.Push(fn);
        SearchRoutineSingle(root.root, root.rootIndex, nameBytes, path);
    }
    
    private bool SearchRoutineSingle(in MftRecord startingPoint, long recordMftIndex, byte[] name, Stack<FileName> path)
    {
        Debug.Assert((startingPoint.RecordHeader.EntryFlags & MftRecordHeaderFlags.IsDirectory) != 0);

        var index = GetIndexFromMftRecord(startingPoint, recordMftIndex);
        var indexSearch = TryFindFileInIndex(index.root, index.allocation, name);
        if (indexSearch is not null)
        {
            var fullPath = $"{StackToString(path)}{indexSearch.Value.Name}";
            Console.WriteLine(fullPath);
            return true;
        }
        
        foreach (var entry in index.root.Entries)
        {
            if ((entry.Flags & IndexEntryFlags.LastInList) != 0)
                break;
                
            var fn = FileName.CreateFromRawData(new RawAttributeData(entry.Content));
            if (((fn.Flags & FileAttributes.DirectoryAlt) == 0) && (fn.Flags & FileAttributes.Directory) == 0) 
                continue;
                
            if (fn.Name.Name.SequenceEqual(_rootName))
                continue;

            path.Push(fn);
            var baseRecordReference = FileReference.Parse(entry.RawStructure);
            var baseRecord = _mftReader.RandomReadAt(baseRecordReference.MftIndex);
            var subfolderSearch = SearchRoutineSingle(baseRecord, baseRecordReference.MftIndex, name, path);
            if (subfolderSearch)
                return true;
            _ = path.Pop();
        }
        
        if (index.allocation is null)
            return false;
        
        foreach (var record in index.allocation!.Value.Records)
        {
            foreach (var entry in record.Entries)
            {
                if ((entry.Flags & IndexEntryFlags.LastInList) != 0)
                    break;
                
                var fn = FileName.CreateFromRawData(new RawAttributeData(entry.Content));
                if (((fn.Flags & FileAttributes.DirectoryAlt) == 0) && (fn.Flags & FileAttributes.Directory) == 0) 
                    continue;
                
                if (fn.Name.Name.SequenceEqual(_rootName))
                    continue;
                
                path.Push(fn);
                var baseRecordReference = FileReference.Parse(entry.RawStructure);
                var baseRecord = _mftReader.RandomReadAt(baseRecordReference.MftIndex);
                var subfolderSearch = SearchRoutineSingle(baseRecord, baseRecordReference.MftIndex, name, path);
                if (subfolderSearch)
                    return true;
                _ = path.Pop();
            }
        }
            
        return false;
    }

    public void FindMultiple(string name)
    {
        var nameBytes = Encoding.Unicode.GetBytes(name);
        (MftRecord root, long rootIndex) root;
        try
        {
            root = GetRootDirectory();
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
            return;
        }
        
        var fn = new FileName()
        {
            Name = new UnicodeName(Encoding.Unicode.GetBytes([_volume.VolumeLetter])),
        };
        var path = new Stack<FileName>();
        path.Push(fn);
        SearchRoutineMultiple(root.root, root.rootIndex, nameBytes, path);
    }
    
    private void SearchRoutineMultiple(in MftRecord startingPoint, long recordMftIndex, byte[] name, Stack<FileName> path)
    {
        Debug.Assert((startingPoint.RecordHeader.EntryFlags & MftRecordHeaderFlags.IsDirectory) != 0);
        
        var index = GetIndexFromMftRecord(startingPoint, recordMftIndex);
        var searchResult = TryFindFileInIndex(index.root, index.allocation, name);
        if (searchResult is not null)
        {
            var fullPath = $"{StackToString(path)}{searchResult.Value.Name}";
            Console.WriteLine(fullPath);
        }
        
        foreach (var entry in index.root.Entries)
        {
            if ((entry.Flags & IndexEntryFlags.LastInList) != 0)
                break;
                
            var fn = FileName.CreateFromRawData(new RawAttributeData(entry.Content));
            if (((fn.Flags & FileAttributes.DirectoryAlt) == 0) && (fn.Flags & FileAttributes.Directory) == 0) 
                continue;
                
            if (fn.Name.Name.SequenceEqual(_rootName))
                continue;
            
            path.Push(fn);
            var baseRecordReference = FileReference.Parse(entry.RawStructure);
            var baseRecord = _mftReader.RandomReadAt(baseRecordReference.MftIndex); 
            SearchRoutineMultiple(baseRecord, baseRecordReference.MftIndex, name, path);
            _ = path.Pop();
        }

        if (index.allocation is null)
            return;
        
        foreach (var record in index.allocation!.Value.Records)
        {
            foreach (var entry in record.Entries)
            {
                if ((entry.Flags & IndexEntryFlags.LastInList) != 0)
                    break;
                
                var fn = FileName.CreateFromRawData(new RawAttributeData(entry.Content));
                if (((fn.Flags & FileAttributes.DirectoryAlt) == 0) && (fn.Flags & FileAttributes.Directory) == 0) 
                    continue;
                
                if (fn.Name.Name.SequenceEqual(_rootName))
                    continue;
                
                path.Push(fn);
                var baseRecordReference = FileReference.Parse(entry.RawStructure);
                var baseRecord = _mftReader.RandomReadAt(baseRecordReference.MftIndex);
                SearchRoutineMultiple(baseRecord, baseRecordReference.MftIndex, name, path);
                _ = path.Pop();
            }
        }
    }
    
    private FileName? TryFindFileInIndex(in IndexRoot root, in IndexAllocation? allocation, byte[] name)
    {
        if (allocation is null)
            return BinarySearch(root.Entries, name);

        var alloc = allocation.Value;
        return TreeSearch(root, alloc, name);
    }
    
    private (IndexRoot root, IndexAllocation? allocation) GetIndexFromMftRecord(in MftRecord record, long recordIndex)
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
        in AttributeList attrList, long baseRecordIndex)
    {
        var rootListEntry = attrList.Entries.First(entry => entry.AttributeType == AttributeType.IndexRoot);
        var rootMftRecord = rootListEntry.RecordReference.MftIndex == baseRecordIndex
            ? baseRecord
            : _mftReader.RandomReadAt(rootListEntry.RecordReference.MftIndex);
        
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
            allocMftRecord = _mftReader.RandomReadAt(allocMftIndex);
        
        var allocData = allocMftRecord.Attributes.First(attr => attr.Header.Type == AttributeType.IndexAllocation)
            .GetAttributeData(_dataReader).ToIndexAllocation(_indexRecordSize, _sectorSize);
        return (rootData, allocData);
    }
    
    private FileName? TreeSearch(in IndexRoot root, in IndexAllocation allocation, byte[] name)
    {
        if ((root.Entries[0].Flags & IndexEntryFlags.LastInList) != 0
            && (root.Entries[0].Flags & IndexEntryFlags.ChildExists) == 0)
        {
            return null;
        }

        var startingVcn = 0UL;
        foreach (var entry in root.Entries)
        {
            if ((entry.Flags & IndexEntryFlags.LastInList) != 0)
            {
                startingVcn = entry.ChildVcn;
                break;
            }
            
            var fn = FileName.CreateFromRawData(new RawAttributeData(entry.Content));
            // Console.WriteLine(fn.Name);
            var comparisonResult = name.AsSpan().SequenceCompareTo(fn.Name.Name.AsSpan());
            if (comparisonResult == 0)
                return fn;

            if (comparisonResult < 0)
            {
                startingVcn = entry.ChildVcn;
                break;
            }
        }
        
        var alloc = LookupRecordByVcn(in allocation, startingVcn);
        while (alloc.NodeHeader.HasChildren)
        {
            foreach (var entry in alloc.Entries)
            {
                if ((entry.Flags & IndexEntryFlags.LastInList) != 0)
                {
                    alloc = LookupRecordByVcn(in allocation, entry.ChildVcn);
                    break;
                }
                
                var fn = FileName.CreateFromRawData(new RawAttributeData(entry.Content));
                // Console.WriteLine(fn.Name);
                var comparisonResult = name.AsSpan().SequenceCompareTo(fn.Name.Name.AsSpan());
                if (comparisonResult == 0)
                    return fn;

                if (comparisonResult < 0)
                {
                    alloc = LookupRecordByVcn(in allocation, entry.ChildVcn);
                    break;
                }
            }
        }

        return BinarySearch(alloc.Entries, name);
    }

    private static IndexRecord LookupRecordByVcn(in IndexAllocation allocation, ulong startingVcn)
    {
        return allocation.Records[startingVcn].RecordHeader.Vcn == startingVcn 
            ? allocation.Records[startingVcn]
            : allocation.Records.First(rec => rec.RecordHeader.Vcn == startingVcn);
    }
    
    // private FileName? LinearSearch(IndexEntry[] entries, byte[] name)
    // {
    //     foreach (var entry in entries)
    //     {
    //         if ((entry.Flags & IndexEntryFlags.LastInList) != 0)
    //             return null;
    //
    //         var fileName = FileName.CreateFromRawData(new RawAttributeData(entry.Content));
    //         // Console.WriteLine(fileName.Name);
    //         if (fileName.Name.Name.SequenceEqual(name))
    //             return fileName;
    //     }
    //     
    //     return null;
    // }

    private FileName? BinarySearch(IndexEntry[] entries, byte[] name)
    {
        if (entries.Length is 0 or 1)
            return null;

        if (entries.Length == 2)
        {
            var firstEntry = entries[0];
            var firstFn = FileName.CreateFromRawData(new RawAttributeData(firstEntry.Content));
                return firstFn.Name.Name.SequenceEqual(name) ? firstFn : null;
        }

        var low = 0;
        var high = entries.Length - 2; // we don't need the last entry because it doesn't contain any data
        while (low <= high)
        {
            var mid = low + ((high - low) >> 1); // found this in Array.BinarySearch source
            var entry = entries[mid];
            var fn = FileName.CreateFromRawData(new RawAttributeData(entry.Content));
            var comparisonResult = name.AsSpan().SequenceCompareTo(fn.Name.Name.AsSpan());
            switch (comparisonResult)
            {
                case < 0:
                    high = mid - 1;
                    break;
                case > 0:
                    low = mid + 1;
                    break;
                default:
                    return fn;
            }
        }

        return null;
    }
    
    private (MftRecord root, long rootIndex) GetRootDirectory()
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

    private string StackToString(Stack<FileName> path)
    {
        var sb = new StringBuilder();
        var firstNode = true;
        foreach (var node in path.Reverse())
        {
            sb.Append(node.Name);
            if (firstNode)
            {
                sb.Append(":\\");
                firstNode = false;   
            }
            else
            {
                sb.Append('\\');
            }
        }
        
        return sb.ToString();
    }

    public void Dispose()
    {
        _volume.Dispose();
    }
}