namespace FileSystemTraverser.MasterFileTable.AttributeData._AttrDef;

public record struct AttrDefData(AttrDefEntry[] Entries)
{
    public static AttrDefData CreateFromData(ref byte[] data)
    {
        using var reader = new BinaryReader(new MemoryStream(data));
        List<AttrDefEntry> entries = new List<AttrDefEntry>();
        var entry = AttrDefEntry.CreateFromStream(reader);
        while (entry != default)
        {
            entries.Add(entry); 
            entry = AttrDefEntry.CreateFromStream(reader);
        }
        
        return new AttrDefData(entries.ToArray());
    }
}