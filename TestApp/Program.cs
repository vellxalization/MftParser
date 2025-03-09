using System.Text;
using NtfsParser;
using NtfsParser.MasterFileTable.AttributeRecord;
using NtfsParser.MasterFileTable.Header;
using NtfsParser.MasterFileTable.ParsedAttributeData.ExtendedAttribute;
using NtfsParser.MasterFileTable.ParsedAttributeData.ReparsePoint;

var volume = new RawVolume('C');
var reader = volume.Initialize();
reader!.SetLcnPosition((int)volume.BootSector!.Value.ExtBpb.LogicalClusterForMft);
int counter = 0;
for (int i = 0; i < 8192; ++i)
{
    Console.WriteLine(counter);
    ++counter;
    var mftRecord = reader.ReadMftRecord();
    if (mftRecord.RecordHeader.Header.Signature == MftSignature.Empty)
    {
        --counter;
        Console.WriteLine("Empty");
        continue;
    }
    
    foreach (var attr in mftRecord.Attributes)
    {
        if (attr.Header.Type == AttributeType.ExtendedAttribute)
        {
            var data = attr.GetAttributeData(reader);
            var parsed = ExtendedAttribute.CreateFromRawData(data, (int)attr.GetActualDataSize());
            // var data = attr.GetAttributeData(reader);
        }
        if (attr.Header.Type == AttributeType.ReparsePoint)
        {
            var data = attr.GetAttributeData(reader);
            var parsed = ReparsePoint.CreateFromRawData(data);
            if (parsed.ReparseTag.AsPredefinedTag() == PredefinedTags.MountPoint)
            {
                var mp = parsed.Data.ToMountPoint();
                Console.WriteLine($"sub name: {Encoding.Unicode.GetString(mp.SubstituteName)}");
                Console.WriteLine($"print name: {Encoding.Unicode.GetString(mp.PrintName)}");
            }
            if (parsed.ReparseTag.AsPredefinedTag() == PredefinedTags.Symlink)
            {
                var sl = parsed.Data.ToSymbolicLink();
                Console.WriteLine($"sub name: {Encoding.Unicode.GetString(sl.SubstituteName)}");
                Console.WriteLine($"print name: {Encoding.Unicode.GetString(sl.PrintName)}");
            }
            // var data = attr.GetAttributeData(reader);
        }
    }
}