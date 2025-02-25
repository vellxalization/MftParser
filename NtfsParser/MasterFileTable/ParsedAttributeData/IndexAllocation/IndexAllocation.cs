using FileSystemTraverser.MasterFileTable.AttributeRecord;

namespace FileSystemTraverser.MasterFileTable.ParsedAttributeData.IndexAllocation;

public record struct IndexAllocation(IndexRecord[] Records)
{
    public static IndexAllocation CreateFromRawData(RawAttributeData rawData, uint indexRecordSize)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var size = data.Length / indexRecordSize;
        IndexRecord[] records = new IndexRecord[size];
        for (int i = 0; i < size; ++i)
        {
            var rawRecord = reader.ReadBytes((int)indexRecordSize);
            records[i] = IndexRecord.Parse(rawRecord);
        }
        
        return new IndexAllocation(records);
    }
}