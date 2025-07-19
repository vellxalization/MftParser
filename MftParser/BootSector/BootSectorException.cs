namespace MftParser.BootSector;

public class BootSectorException(string message) : Exception(message);

public class InvalidJmpException(Span<byte> value, long positionAfterReading)
    : BootSectorException($"Expected a JMP bytes (0xEB, 0x52, 0x90), got: 0x{value[0]:X}, 0x{value[1]:X}, 0x{value[2]:X} at index 0x{positionAfterReading - 3:X}.");

public class InvalidOemIdException(ulong value, long positionAfterReading)
    : BootSectorException($"Expected an OEM ID bytes (0x20, 0x20, 0x20, 0x20, 0x53, 0x46, 0x54, 0x4E), got: 0x{value:X} at index 0x{positionAfterReading - 8:X}.");

public class InvalidEndMarkerException(ushort value)
    : BootSectorException($"Expected an end of sector marker (0xAA55), got 0x{value:X} at the end of the sector.");

public class BpbException(string message) : BootSectorException(message);

public class ZeroedFieldException(byte value, long positionAfterReading) : BpbException($"A field that should be 0 has a value of {value} at index 0x{positionAfterReading - 1:X}.");