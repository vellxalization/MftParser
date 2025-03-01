using System.Text;

namespace NtfsParser.MasterFileTable.ParsedAttributeData.ReparsePoint;

public record struct MountPoint(ushort SubstituteNameOffset, ushort SubstituteNameSize, ushort PrintNameOffset, 
    ushort PrintNameSize, byte[] SubstituteName, byte[] PrintName)
{
    public static MountPoint CreateFromRawData(RawReparseData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var substituteNameOffset = reader.ReadUInt16(); // relative to the start of the data
        var substituteNameSize = reader.ReadUInt16();
        var printNameOffset = reader.ReadUInt16(); // relative to the start of the data
        var printNameSize = reader.ReadUInt16();
        var dataStart = reader.Position;
        reader.Position = dataStart + substituteNameOffset;
        var diff = printNameOffset + dataStart - reader.Position - 2; // subtract 2 because of the string terminator
        // using it in case something went wrong when reading size
        var substituteName = reader.ReadBytes(substituteNameSize == diff ? substituteNameSize : diff);
        reader.Position = dataStart + printNameOffset;
        diff = data.Length - reader.Position - 2;
        var printName = reader.ReadBytes(printNameSize == diff ? printNameSize : diff);
        
        return new MountPoint(substituteNameOffset, substituteNameSize, printNameOffset, printNameSize, 
            substituteName.ToArray(), printName.ToArray());
    }

    public string GetStringSubstituteName() => Encoding.Unicode.GetString(SubstituteName);
    public string GetStringPrintName() => Encoding.Unicode.GetString(PrintName);
}