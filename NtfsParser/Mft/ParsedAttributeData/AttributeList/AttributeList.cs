using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData.AttributeList;

public record struct AttributeList(AttributeListEntry[] Entries)
{
    public static AttributeList CreateFromRawData(RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var entries = new List<AttributeListEntry>();
        while (data.Length > 0)
        {
            var entry = AttributeListEntry.Parse(data);
            entries.Add(entry);
            data = data.Slice(entry.RecordSize);
            // ^ doing this to save myself from the headache of not having 0-based indecies
        } 
        
        return new AttributeList(entries.ToArray());
    }
}