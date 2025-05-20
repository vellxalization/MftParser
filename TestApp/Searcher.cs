using NtfsParser;
using NtfsParser.Mft;
using NtfsParser.Mft.Attribute;
using NtfsParser.Mft.MftRecord;
using NtfsParser.Mft.ParsedAttributeData;
using NtfsParser.Mft.ParsedAttributeData.AttributeList;

namespace TestApp;

public class Searcher
{
    private RawVolume _volume;
    private Queue<IndexEntry> _unscannedIndices = new();

    public Searcher(RawVolume volume)
    {
        _volume = volume;
    }

    public MftRecord? FindByName(string name)
    {
        _unscannedIndices.Clear();
        var volumeRoot = GetVolumeIndexRoot();
        var rootResult = ScanMftRecordIndices(volumeRoot, name);
        if (rootResult is not null)
        {
            return rootResult;
        }

        while (_unscannedIndices.Count > 0)
        {
            var scanResult = ScanLevel(name);
            if (scanResult is not null)
            {
                return scanResult;
            }
        }
        
        return null;
    }
    
    private MftRecord? ScanMftRecordIndices(MftRecord volumeRoot, string name)
    {
        var attrListAttribute = volumeRoot.Attributes.FirstOrDefault(attr => attr.Header.Type == AttributeType.AttributeList);
        if (attrListAttribute != default)
        {
            var attrList = attrListAttribute.GetAttributeData(_volume.VolumeReader).ToAttributeList();
            var scanResult = ScanAttributeList(attrList, name);
            if (scanResult is not null)
            {
                return scanResult;
            }

            return null;
        }
        
        var rootAttribute = volumeRoot.Attributes.FirstOrDefault(attr => attr.Header.Type == AttributeType.IndexRoot);
        if (rootAttribute == default)
        {
            throw new Exception("Root node not found");
        }

        var root = rootAttribute.GetAttributeData(_volume.VolumeReader).ToIndexRoot();
        var searchResult = ScanIndexEntries(root.Entries, name);
        if (searchResult is not null)
        {
            return searchResult;
        }

        var allocAttribute = volumeRoot.Attributes.FirstOrDefault(attr => attr.Header.Type == AttributeType.IndexAllocation);
        if (allocAttribute != default)
        {
            var alloc = allocAttribute.GetAttributeData(_volume.VolumeReader).ToIndexAllocation(_volume.BootSector.IndexRecordByteSize, _volume.BootSector.SectorByteSize);
            foreach (var record in alloc.Records)
            {
                searchResult = ScanIndexEntries(record.Entries, name);
                if (searchResult is not null)
                {
                    return searchResult;
                }
            }
        }

        return null;
    }
    
    private MftRecord? ScanAttributeList(AttributeList attrList, string name)
    {
        var rootAttributeInList = attrList.Entries.FirstOrDefault(attrListEntry => attrListEntry.AttributeType == AttributeType.IndexRoot);
        if (rootAttributeInList == default)
        {
            throw new Exception("Root node not found");
        }

        var rootOwner = GetRecordAtAddress(rootAttributeInList.FileReference);
        var rootAttribute = rootOwner.Attributes.FirstOrDefault(attr => attr.Header.Type == AttributeType.IndexRoot);
        var root = rootAttribute.GetAttributeData(_volume.VolumeReader).ToIndexRoot();
        var searchResult = ScanIndexEntries(root.Entries, name);
        if (searchResult is not null)
        {
            return searchResult;
        }
        
        var allocAttributeInList = attrList.Entries.FirstOrDefault(attrListEntry => attrListEntry.AttributeType == AttributeType.IndexAllocation);
        if (allocAttributeInList == default)
        {
            return null;
        }

        var allocAttributeOwner = GetRecordAtAddress(allocAttributeInList.FileReference);
        var allocAttribute = allocAttributeOwner.Attributes.FirstOrDefault(attr => attr.Header.Type == AttributeType.IndexAllocation);
        var alloc = allocAttribute.GetAttributeData(_volume.VolumeReader).ToIndexAllocation(_volume.BootSector.IndexRecordByteSize, _volume.BootSector.SectorByteSize);
        foreach (var record in alloc.Records)
        {
            searchResult = ScanIndexEntries(record.Entries, name);
            if (searchResult is not null)
            {
                return searchResult;
            }
        }
        
        return null;
    }
    
    private MftRecord? ScanLevel(string searchable)
    {
        var recordsCount = _unscannedIndices.Count;
        for (int i = 0; i < recordsCount; ++i)
        {
            var index = _unscannedIndices.Dequeue();
            var reference = FileReference.Parse(index.Bytes);
            var mftRecord = GetRecordAtAddress(reference);
            var scanResult = ScanMftRecordIndices(mftRecord, searchable);
            if (scanResult is not null)
            {
                return scanResult;
            }
        }
        
        return null;
    }

    private MftRecord? ScanIndexEntries(IndexEntry[] entries, string searchable)
    {
        foreach (var entry in entries)
        {
            if (entry.Content.Length == 0)
            {
                continue;
            }
            
            var filename = new RawAttributeData(entry.Content).ToFileName();
            var stringName = filename.Name.ToString();
            if (stringName == searchable)
            {
                var reference = FileReference.Parse(entry.Bytes);
                var foundEntry = GetRecordAtAddress(reference);
                return foundEntry;
            }

            if ((filename.Flags & FileNameFlags.Directory) != 0 && stringName != ".")
            {
                _unscannedIndices.Enqueue(entry);
            }
        }

        return null;
    }

    private MftRecord GetRecordAtAddress(FileReference reference)
    {
        var reader = _volume.MftReader;
        reader.MftIndex = (int)reference.MftOffset;
        return reader.ReadMftRecord();
    }
    
    private MftRecord GetVolumeIndexRoot()
    {
        var mftReader = _volume.MftReader;
        var volumeReader = _volume.VolumeReader;
        foreach (var record in mftReader.StartReadingMft())
        {
            var attributes = record.Attributes;
            foreach (var attr in attributes)
            {
                if (attr.Header.Type != AttributeType.FileName)
                {
                    continue;
                }

                var fileName = attr.GetAttributeData(volumeReader).ToFileName();
                if (fileName.Name.ToString() == ".")
                {
                    return record;
                }
            }
        }
        
        throw new Exception("Root not found. Shouldn't ever happen' tho");
    }
}