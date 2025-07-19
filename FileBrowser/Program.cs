using System.Text;
using System.Text.RegularExpressions;
using MftParser;
using MftParser.Mft.ParsedAttributeData;
using FileAttributes = MftParser.Mft.ParsedAttributeData.FileAttributes;

if (args.Length == 0 || !char.TryParse(args[0], out var letter))
{
    Console.WriteLine("Please specify a volume letter");
    return;
}

RawVolume volume;
try
{
    volume = new RawVolume(letter);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    return;
}

var browser = new FileBrowser.FileBrowser(volume);
var argsRegex = new Regex("""("[^"]+"|\S+)""");
while (true)
{
    Console.Write($"{browser.CurrentPath}>");
    var command = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(command))
        continue;

    var commandSeparated = argsRegex.Matches(command);
    switch (commandSeparated[0].ToString())
    {
        case "cd":
        {
            var clearPath = commandSeparated[1].ToString().Trim('"');
            var path = clearPath.Split('\\', '/');
            var result = browser.TryChangeFolder(path);
            if (!result.Item1)
                Console.WriteLine(result.Item2);
            
            Console.WriteLine();
            break;
        }
            
        case "ls":
        case "list":
        {
            DisplayCurrentFolder();
            break;
        }

        default:
        {
            Console.WriteLine("Unknown command");
            Console.WriteLine();
            break;
        }
    }
}
void DisplayCurrentFolder()
{
    Console.Clear();
    foreach (var folder in browser.CurrentFolder.InnerFiles)
        Console.WriteLine(FormatFileData(folder.Data));
    Console.WriteLine();
}

string FormatFileData(FileName file)
{
    var sb = new StringBuilder();
    sb.Append(file.FileAltered.ToDateTimeOffset().ToString("MM/dd/yyyy @ hh:mm:ss"));
    sb.Append(' ');
    FormatFlags(sb, file.Flags);
    sb.Append(' ');
    sb.Append(file.Name.ToString());
    return sb.ToString();
}

void FormatFlags(StringBuilder sb, FileAttributes attributes)
{
    sb.Append((attributes & FileAttributes.Readonly) != 0 ? 'R' : '-');
    sb.Append((attributes & FileAttributes.Hidden) != 0 ? 'H' : '-');
    sb.Append((attributes & FileAttributes.System) != 0 ? 'S' : '-');
    sb.Append((attributes & (FileAttributes.Directory | FileAttributes.DirectoryAlt)) != 0 ? 'D' : '-');
    sb.Append((attributes & FileAttributes.Archive) != 0 ? 'A' : '-');
    sb.Append((attributes & FileAttributes.Temporary) != 0 ? 'T' : '-');
    sb.Append((attributes & FileAttributes.ReparsePoint) != 0 ? 'P' : '-');
    sb.Append(attributes.HasFlag(FileAttributes.Compressed) ? 'C' : '-');
    sb.Append((attributes & FileAttributes.Encrypted) != 0 ? 'E' : '-');
}