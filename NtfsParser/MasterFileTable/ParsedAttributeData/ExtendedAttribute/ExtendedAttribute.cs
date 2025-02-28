using System.Text;
using NtfsParser.MasterFileTable.AttributeRecord;

namespace NtfsParser.MasterFileTable.ParsedAttributeData.ExtendedAttribute;

public record struct ExtendedAttribute(ExtendedAttributeEntry[] Entries)
{
    public static ExtendedAttribute CreateFromRawData(RawAttributeData rawData, int dataSize)
    {
        var data = rawData.Data.AsSpan().Slice(0, dataSize); 
        // anything after datasize might contain some irrelevant data if the attribute is nonresident so we trim it
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