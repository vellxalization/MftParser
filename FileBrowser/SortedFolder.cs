using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text;
using MftParser.Mft;
using MftParser.Mft.Attribute;
using MftParser.Mft.ParsedAttributeData;
using MftParser.Mft.ParsedAttributeData.Index;
using FileAttributes = MftParser.Mft.ParsedAttributeData.FileAttributes;

namespace FileBrowser;

public readonly record struct SortedFolder((FileName Name, long MftIndex) Folder, ReadOnlyCollection<(FileName Data, long MftIndex)> InnerFiles)
{
    private static readonly LogicalStringComparer Comparer = new();
    private static readonly byte[] RootName = Encoding.Unicode.GetBytes(".");
    private static readonly Func<FileName, bool> IsRoot = fn => (fn.Flags & FileAttributes.System) != 0 &&
                                                          ((fn.Flags & FileAttributes.Directory) != 0 ||
                                                           (fn.Flags & FileAttributes.DirectoryAlt) != 0)
                                                          && fn.Name.Name.AsSpan().SequenceEqual(RootName);

    public (FileName Data, long MftIndex)? TryFind(string name)
    {
        // readonlycollection doesn't support binary search by default for some reason
        var low = 0;
        var high = InnerFiles.Count - 1;
        while (low <= high)
        {
            var mid = low + ((high - low) >> 1);
            var fn = InnerFiles[mid];
            var comparison = Comparer.Compare(name, fn.Data.Name.ToString());
            switch (comparison)
            {
                case < 0:
                    high = mid - 1;
                    continue;
                case > 0:
                    low = mid + 1;
                    continue;
                default: // 0
                    return fn;
            }
        }

        return null;
    }
    
    public static SortedFolder Create(FileName folderName, long mftIndex, IndexRoot root, IndexAllocation? allocation)
    {
        if (allocation is null)
        {
            var folders = new List<(FileName, long)>();
            AddEntriesToList(root.Entries, folders);
            return new SortedFolder((folderName, mftIndex), new ReadOnlyCollection<(FileName Data, long MftIndex)>(folders.OrderBy(tuple => tuple.Item1.Name.ToString(), Comparer).ToArray()));
        }

        var folder = new SortedFolder((folderName, mftIndex), CombineRootAndAllocationSorted(root, allocation.Value));
        return folder;
    }
    
    private static ReadOnlyCollection<(FileName, long)> CombineRootAndAllocationSorted(IndexRoot root, IndexAllocation allocation)
    {
        var lenSum = (root.Entries.Length - 1) + allocation.Records.Sum(rec => rec.Entries.Length - 1);
        var folders = new List<(FileName, long)>(lenSum);
        AddEntriesToList(root.Entries, folders);
        
        foreach (var record in allocation.Records)
            AddEntriesToList(record.Entries, folders);
        
        return new ReadOnlyCollection<(FileName, long)>(folders.OrderBy(tuple => tuple.Item1.Name.ToString(), Comparer).ToArray());
    }
    
    private static void AddEntriesToList(IndexEntry[] entries, List<(FileName, long)> folders)
    {
        foreach (var entry in entries)
        {
            if (entry.ContentLength == 0)
                break;
            
            var fn = (FileName.CreateFromRawData(new RawAttributeData(entry.Content)));
            if (IsRoot(fn))
                continue;
            
            var mftRef = FileReference.Parse(entry.RawStructure);
            folders.Add((fn, mftRef.MftIndex));
        }
    }
}