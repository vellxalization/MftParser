using System.Text;

namespace NtfsParser.MasterFileTable.ParsedAttributeData.ReparsePoint;

public record struct SymbolicLink(ushort SubstituteNameOffset, ushort SubstituteNameSize, ushort PrintNameOffset,
    ushort PrintNameSize, bool IsRelative, byte[] SubstituteName, byte[] PrintName)
{
    public static SymbolicLink CreateFromRawData(RawReparseData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var substituteNameOffset = reader.ReadUInt16(); // relative to the start of the data
        var substituteNameSize = reader.ReadUInt16();
        var printNameOffset = reader.ReadUInt16(); // relative to the start of the data
        var printNameSize = reader.ReadUInt16();
        var flags = reader.ReadUInt32();
        var dataStart = reader.Position;
        reader.Position = dataStart + substituteNameOffset;
        var substituteName = reader.ReadBytes(substituteNameSize);
        reader.Position = dataStart + printNameOffset;
        var printName = reader.ReadBytes(printNameSize);
        
        return new SymbolicLink(substituteNameOffset, substituteNameSize, printNameOffset, printNameSize, 
            flags == 1, substituteName.ToArray(), printName.ToArray());
    }
    
    public string GetStringSubstituteName() => Encoding.Unicode.GetString(SubstituteName);
    public string GetStringPrintName() => Encoding.Unicode.GetString(PrintName);
}