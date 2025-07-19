namespace MftParser.Mft.ParsedAttributeData.SecurityDescriptor;

/// <summary>
/// List of access entries that identify a trustee and specifies access rights for the trustee
/// </summary>
/// <param name="Revision">Revision level of the list</param>
/// <param name="ListSize">Size of the list in bytes</param>
/// <param name="EntriesCount">Number of entries stored in the list</param>
/// <param name="Entries">Entries</param>
public readonly record struct AccessControlList(byte Revision, ushort ListSize, ushort EntriesCount, AccessControlEntry[] Entries)
{
    public static AccessControlList Parse(Span<byte> rawAcl)
    {
        var reader = new SpanBinaryReader(rawAcl);
        var revision = reader.ReadByte();
        reader.Skip(1); // padding
        var listSize = reader.ReadUInt16();
        var entriesCount = reader.ReadUInt16();
        reader.Skip(2); // padding
        var entries = new AccessControlEntry[entriesCount];
        for (int i = 0; i < entriesCount; ++i)
        {
            var entry = AccessControlEntry.CreateFromStream(ref reader);
            entries[i] = entry;
        }
        
        return new AccessControlList(revision, listSize, entriesCount, entries); 
    }
}

/// <summary>
/// A structure that describes access rights associated with an ID
/// </summary>
/// <param name="Type">Entry's type</param>
/// <param name="Flags">Entry's flags</param>
/// <param name="Size">Size of the entry in bytes</param>
/// <param name="AccessMask">Access rights</param>
/// <param name="SecurityId">SID</param>
public readonly record struct AccessControlEntry(AceType Type, AceFlags Flags, ushort Size, AccessMask AccessMask, SecurityId SecurityId)
{
    public static AccessControlEntry CreateFromStream(ref SpanBinaryReader reader)
    {
        var start = reader.Position;
        var type = (AceType)reader.ReadByte();
        var flags = (AceFlags)reader.ReadByte();
        var size = reader.ReadUInt16();
        var accessMask = reader.ReadUInt32();
        var sId = reader.ReadBytes(size - (reader.Position - start));
        
        return new AccessControlEntry(type, flags, size, new AccessMask(accessMask), new SecurityId(sId.ToArray()));
    }
}

public enum AceType : byte
{
    /// <summary>
    /// Rights are granted
    /// </summary>
    AccessAllowed = 0x00,
    /// <summary>
    /// Rights are denied
    /// </summary>
    AccessDenied = 0x01,
    /// <summary>
    /// Entry specifies auditing behavior
    /// </summary>
    SystemAudit = 0x02
}

[Flags]
public enum AceFlags : byte
{
    /// <summary>
    /// ACE was inherited
    /// </summary>
    ObjectInheritAce = 0x01,
    /// <summary>
    /// Container child objects (e.g. folders) inherits ACE 
    /// </summary>
    ContainerInheritAce = 0x02,
    /// <summary>
    /// ACE won't be inherited by subsequent generations
    /// </summary>
    NoPropagateInheritAce = 0x04,
    /// <summary>
    /// ACE doesn't control access and is only used for inheritance
    /// </summary>
    InheritOnlyAce = 0x08,
    /// <summary>
    /// Generates messages on successful access attempts. Exclusive for system audit type
    /// </summary>
    SuccessfulAccess = 0x40,
    /// <summary>
    /// Generates messages on failed access attempts. Exclusive for system audit type
    /// </summary>
    FailedAccess = 0x80
}