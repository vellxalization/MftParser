namespace NtfsParser.MasterFileTable.Attribute;

public record struct DataRun(byte Header, UInt128 Length, Int128 Offset);