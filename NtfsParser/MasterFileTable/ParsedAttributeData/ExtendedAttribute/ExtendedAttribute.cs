using FileSystemTraverser.MasterFileTable.AttributeRecord;

namespace FileSystemTraverser.MasterFileTable.ParsedAttributeData.ExtendedAttribute;

public record struct ExtendedAttribute()
{
    public static ExtendedAttribute CreateFromRawData(RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var entries = new List<ExtendedAttributeEntry>();
        var entry = ExtendedAttributeEntry.Parse(data);

        return new ExtendedAttribute();
    }
}