using System.Text;

namespace NtfsParser.Mft.ParsedAttributeData.ReparsePoint;

/// <summary>
/// Mounted folder or drive
/// </summary>
/// <param name="SubstituteNameOffset">Offset at which substitute name starts. Relative to the start of the data</param>
/// <param name="SubstituteNameSize">Size of the substitute name in bytes</param>
/// <param name="PrintNameOffset">Offset at which print name starts. Relative to the start of the data</param>
/// <param name="PrintNameSize">Size of the substitute name in bytes</param>
/// <param name="SubstituteName">Target's pathname</param>
/// <param name="PrintName">User-friendly target's pathname</param>
public readonly record struct MountPoint(ushort SubstituteNameOffset, ushort SubstituteNameSize, ushort PrintNameOffset, 
    ushort PrintNameSize, UnicodeName SubstituteName, UnicodeName PrintName)
{
    public static MountPoint CreateFromRawData(in RawReparseData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var substituteNameOffset = reader.ReadUInt16(); // relative to the start of the data
        var substituteNameSize = reader.ReadUInt16();
        var printNameOffset = reader.ReadUInt16(); // relative to the start of the data
        var printNameSize = reader.ReadUInt16();

        var dataStart = reader.Position;
        reader.Position = dataStart + substituteNameOffset;
        var substituteName = reader.ReadBytes(substituteNameSize);
        reader.Position = dataStart + printNameOffset;
        var printName = reader.ReadBytes(printNameSize);
        
        return new MountPoint(substituteNameOffset, substituteNameSize, printNameOffset, printNameSize, 
            new UnicodeName(substituteName.ToArray()), new UnicodeName(printName.ToArray()));
    }
}