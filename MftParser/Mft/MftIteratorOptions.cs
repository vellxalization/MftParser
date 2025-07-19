namespace MftParser.Mft;

/// <summary>
/// Filter options for the MFT iterator
/// </summary>
/// <param name="IgnoreEmpty">Ignore uninitialized MFT records (entries that consists of zeroes)</param>
/// <param name="IgnoreUnused">Ignore MFT records that don't have "InUse" flag in the header.
/// Unused records might contain outdated information so it might be a good idea to skip them</param>
/// <param name="StartFrom">Will set reader's position to this value before iteration</param>
public record MftIteratorOptions(bool IgnoreEmpty = false, bool IgnoreUnused = false, int StartFrom = 0);