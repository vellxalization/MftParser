using NtfsParser;
using NtfsParser.Mft;
using NtfsParser.Mft.Attribute;

if (args.Length < 1)
{
    Console.WriteLine("Please specify a volume letter");
    goto Exit;
}

if (args[0].Length > 1)
{
    Console.WriteLine("Please specify a valid volume letter");
    goto Exit;
}

RawVolume volume;
try
{
    volume = new RawVolume(char.Parse(args[0]));
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    goto Exit;
}

var ignoreUnused = true ; // ignore records that don't have InUse flag
var ignoreEmpty = true; // ignore non-initialized records 
if (args.Contains("--unused"))
    ignoreUnused = false;
if (args.Contains("--empty"))
    ignoreEmpty = false;
var iteratorOptions = new MftIteratorOptions()
{
    IgnoreEmpty = ignoreEmpty, 
    IgnoreUnused = ignoreUnused 
};
var mftReader = volume.MftReader;
foreach (var mftRecord in mftReader.StartReadingMft(iteratorOptions))
{
    var index = mftReader.MftIndex - 1;
    Console.Write("Record {0}", index);
    if (mftRecord.RecordHeader.MultiSectorHeader.Signature == MftSignature.Empty)
    {
        Console.WriteLine(" (empty)");
        continue;
    }
    
    if ((mftRecord.RecordHeader.EntryFlags & MftRecordHeaderFlags.InUse) == 0)
        Console.WriteLine(" (unused)");
    else
        Console.WriteLine("");
    
    Console.WriteLine("Flags: {0}", ((uint)mftRecord.RecordHeader.EntryFlags & uint.MaxValue) == 0 ? "none" : mftRecord.RecordHeader.EntryFlags);
    var attrList = mftRecord.Attributes.FirstOrDefault(attr => attr.Header.Type == AttributeType.AttributeList);
    if (attrList != default)
    {
        var attrs = attrList.GetAttributeData(volume.VolumeReader).ToAttributeList();
        if (attrs.Entries.Length == 0)
        {
            Console.WriteLine("Attribute: none");
            break;
        }
        
        Console.WriteLine("Attributes:");
        foreach (var attr in attrs.Entries)
        {
            Console.Write("-> {0}", attr.AttributeType);
            Console.WriteLine(attr.RecordReference.MftIndex != index ? " (not in the base record)" : "");
        }
        Console.WriteLine("");
        continue;
    }
    
    if (mftRecord.Attributes.Length == 0)
    {
        Console.WriteLine("Attributes: none");
        continue;
    }
    
    Console.WriteLine("Attributes:");
    foreach (var attr in mftRecord.Attributes)
        Console.WriteLine("-> {0}", attr.Header.Type);
    Console.WriteLine("");
}

volume.Dispose();
Exit:
Console.WriteLine("Press any key to exit...");
Console.ReadKey();