namespace NtfsParser.Mft;

public class MftException(string message) : Exception(message);

public class InvalidMftRecordException(Span<byte> signature) 
    : MftException($"Invalid MFT record signature: 0x{signature[0]:X}, 0x{signature[1]:X}, 0x{signature[2]:X}, 0x{signature[3]:X}");

public class InvalidIndexException(Span<byte> signature)
    : MftException($"Invalid INDX signature: 0x{signature[0]:X}, 0x{signature[1]:X}, 0x{signature[2]:X}, 0x{signature[3]:X}");

public class FixUpMismatchException(byte expectedValue, byte actualValue) : MftException($"Fixup values mismatch. Expected a 0x{actualValue:X}, got: 0x{expectedValue:X}. Possibly a corrupted sector!");