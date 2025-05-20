using System.Text;

namespace NtfsParser.Mft;

public readonly record struct UnicodeName(byte[] Name)
{
    public static UnicodeName Empty => new([]);
    public override string ToString() => Encoding.Unicode.GetString(Name);
}