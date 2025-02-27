namespace NtfsParser.BootSector;

public class BootSectorException(string message) : Exception(message);

public class InvalidStartingPositionException(byte expected, long actual)
    : BootSectorException($"Expected to start reading at byte offset 0x{expected:X}, tried to start at: 0x{actual:X}");

public class InvalidJmpException(byte[] value, long positionAfterReading)
    : BootSectorException($"Expected a JMP bytes (0xEB, 0x52, 0x90), got: 0x{value[0]:X}, 0x{value[1]:X}, 0x{value[2]:X}. Position after reading: 0x{positionAfterReading:X}.");

public class InvalidOemIdException(ulong value, long positionAfterReading)
    : BootSectorException($"Expected an OEM ID bytes (0x20, 0x20, 0x20, 0x20, 0x53, 0x46, 0x54, 0x4E), got: 0x{value:X}. Position after reading: 0x{positionAfterReading:X}.");

public class InvalidEndMarkerException(ushort value)
    : BootSectorException($"Expected an end of sector marker (0xAA55), got: 0x{value:X}.");

public class BpbException(string message) : BootSectorException(message);

public class ShouldBeZeroException(byte value, long positionAfterReading) : BpbException($"A field that should be 0 has a value of {value}. Position after reading: 0x{positionAfterReading - 1:X}.");