using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData.Index;

/// <summary>
/// An attribute that contains one or more index records
/// </summary>
/// <param name="Records">Index records</param>
public readonly record struct IndexAllocation(IndexRecord[] Records)
{
    public static IndexAllocation CreateFromRawData(in RawAttributeData rawData, int indexRecordSize, int sectorSize)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var size = data.Length / indexRecordSize;
        List<IndexRecord> records = new(size); // use list because there can be empty records
        for (int i = 0; i < size; ++i)
        {
            var rawRecord = reader.ReadBytes(indexRecordSize);
            if (rawRecord[..4] is [0, 0, 0, 0])
                continue;

            records.Add(IndexRecord.Parse(rawRecord, sectorSize));
        }

        return new IndexAllocation(records.ToArray());
    }
}