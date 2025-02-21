namespace FileSystemTraverser.MasterFileTable.AttributeData._INDEX_ALLOCATION;

public record struct IndexAllocation(IndexRecord[] Records)
{
    public static IndexAllocation CreateFromData(ref byte[] data, uint indexRecordSize)
    {
        using var reader = new BinaryReader(new MemoryStream(data));
        var size = data.Length / indexRecordSize;
        IndexRecord[] records = new IndexRecord[size];
        for (int i = 0; i < size; ++i)
        {
            records[i] = IndexRecord.CreateFromStream(reader);
            reader.BaseStream.Position = (i + 1) * indexRecordSize;
        }
        
        return new IndexAllocation(records);
    }
}