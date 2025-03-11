using NtfsParser.MasterFileTable.Attribute;

namespace NtfsParser.MasterFileTable.ParsedAttributeData.ExtendedAttribute;

public record struct ExtendedAttribute(ExtendedAttributeEntry[] Entries)
{
    public static ExtendedAttribute CreateFromRawData(RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan(); 
        var entries = new List<ExtendedAttributeEntry>();
        while (data.Length > 0)
        {
            var entry = ExtendedAttributeEntry.Parse(data);
            entries.Add(entry);
            data = data.Slice((int)entry.EntrySize);
        }
        
        return new ExtendedAttribute(entries.ToArray());
    }
}