using System.Text;

namespace MftParser.Mft.ParsedAttributeData.ReparsePoint;

/// <summary>
/// Symbolic link to a file or folder
/// </summary>
/// <param name="SubstituteNameOffset">Offset at which substitute name starts. Relative to the start of the data</param>
/// <param name="SubstituteNameSize">Size of the substitute name in bytes</param>
/// <param name="PrintNameOffset">Offset at which print name starts. Relative to the start of the data</param>
/// <param name="PrintNameSize">Size of the substitute name in bytes</param>
/// <param name="IsRelative">The substitute name is a path name relative to the directory containing the symbolic link</param>
/// <param name="SubstituteName">Target's pathname</param>
/// <param name="PrintName">User-friendly target's pathname</param>
public readonly record struct SymbolicLink(ushort SubstituteNameOffset, ushort SubstituteNameSize, ushort PrintNameOffset,
    ushort PrintNameSize, bool IsRelative, UnicodeName SubstituteName, UnicodeName PrintName)
{
    public static SymbolicLink CreateFromRawData(in RawReparseData rawData)
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
            flags == 1, new UnicodeName(substituteName.ToArray()), new UnicodeName(printName.ToArray()));
    }
}