using System.Text;

namespace MftParser.Mft;

public readonly record struct AsciiName(byte[] Name)
{
    public static AsciiName Empty => new([]);
    public override string ToString() => Encoding.ASCII.GetString(Name);
}