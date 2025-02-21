namespace FileSystemTraverser.MasterFileTable;

public class MftException(string message) : Exception(message);

public class InvalidFlagValueException(string message) : MftException(message);

public class MftHeaderException(string message) : Exception(message);

public class FileSignatureException(byte[] signature) 
    : MftHeaderException($"Expected a FILE signature (0x46, 0x49, 0x4C, 0x45), got: 0x{signature[0]:X}, 0x{signature[1]:X}, 0x{signature[2]:X}, 0x{signature[3]:X}");
