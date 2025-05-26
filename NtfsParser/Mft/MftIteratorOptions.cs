namespace NtfsParser.Mft;

public record MftIteratorOptions(bool IgnoreEmpty = false, bool IgnoreUnused = false, int StartFrom = 0);