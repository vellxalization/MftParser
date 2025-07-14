using System.Diagnostics;
using FileSearcher;
using NtfsParser;

if (args.Length < 2)
{
    Console.WriteLine("Please specify a volume letter and a file name");
    goto Exit;
}
var volChar = args[0];
if (volChar.Length is 0 or > 1)
{
    Console.WriteLine("Please specify a valid volume letter");
    goto Exit;
}

var parsedVolChar = char.Parse(volChar);
RawVolume volume;
try
{
    volume = new RawVolume(parsedVolChar);
}
catch(Exception ex)
{
    Console.WriteLine(ex.Message);
    goto Exit;
}

if (args.Length < 2)
{
    Console.WriteLine("Please specify a file name");
    goto ExitWithDispose;
}
var fileName = args[1];
var isSingle = !args.Contains("-m") && !args.Contains("--multiple");
var searcher = new DfsFileSearcher(volume);
if (isSingle)
    searcher.FindSingleMatch(fileName);
else
    searcher.FindMultiple(fileName);

ExitWithDispose:
volume.Dispose();
Exit:
Console.WriteLine("Press any key to exit");
Console.ReadKey();