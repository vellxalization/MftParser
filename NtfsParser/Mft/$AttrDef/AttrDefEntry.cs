using System.Text;

namespace NtfsParser.Mft._AttrDef;

public record struct AttrDefEntry(byte[] UnicodeLabel, uint Type, uint DisplayRule, uint CollationRule, uint Flags, ulong MinSize, ulong MaxSize)
{
    public static AttrDefEntry CreateFromStream(BinaryReader reader)
    {
        var label = reader.ReadBytes(128);
        if (label[0] == 0x00)
        {
            return new AttrDefEntry();
        }
        
        var type = reader.ReadUInt32();
        var displayRule = reader.ReadUInt32();
        var collationRule = reader.ReadUInt32();
        var flags = reader.ReadUInt32();
        var minSize = reader.ReadUInt64();
        var maxSize = reader.ReadUInt64();
        
        return new AttrDefEntry(label, type, displayRule, collationRule, flags, minSize, maxSize);
    }

    public string GetStringLabel() => Encoding.Unicode.GetString(UnicodeLabel);
}