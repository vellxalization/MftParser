using System.Numerics;

namespace FileBrowser;

public class LogicalStringComparer : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x is null && y is null)
            return 0;

        if (x is null && y is not null)
            return -1;
        
        if (x is not null && y is null)
            return 1;
        
        var xSpan = x.AsSpan();
        var ySpan = y.AsSpan();
        
        var lim = Math.Min(xSpan.Length, ySpan.Length);
        for (var i = 0; i < lim; i++)
        {
            var xChar = char.ToLower(xSpan[i]);
            var yChar = char.ToLower(ySpan[i]);
            if (xChar == yChar)
                continue;
            
            var xIsDigit = char.IsDigit(xChar);
            var yIsDigit = char.IsDigit(yChar);
            if (xIsDigit && !yIsDigit)
                return -1;

            if (!xIsDigit && yIsDigit)
                return 1;
            
            if (!xIsDigit && !yIsDigit)
                return xChar.CompareTo(yChar);
            
            var xNum = SelectNumber(xSpan[i..]);
            var yNum = SelectNumber(ySpan[i..]);
            return BigInteger.Parse(xNum) < BigInteger.Parse(yNum) ? -1 : 1; // because file name might just be a bunch of number, use big int to avoid overflowing
        }
        
        if (xSpan.Length == ySpan.Length)
            return 0;
        
        return xSpan.Length < ySpan.Length ? -1 : 1;
    }

    private static ReadOnlySpan<char> SelectNumber(ReadOnlySpan<char> str)
    {
        var ind = 1;
        while (ind < str.Length && char.IsDigit(str[ind]))
            ++ind;

        return str[..ind];
    }
}