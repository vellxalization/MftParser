namespace NtfsParser.Mft.ParsedAttributeData.SecurityDescriptor;

public readonly record struct AccessControlList(byte Revision, ushort AclSize, ushort AceCount, AccessControlEntry[] Entries)
{
    public static AccessControlList Parse(Span<byte> rawAcl)
    {
        var reader = new SpanBinaryReader(rawAcl);
        var revision = reader.ReadByte();
        reader.Skip(1); // padding
        var aclSize = reader.ReadUInt16();
        var aceCount = reader.ReadUInt16();
        reader.Skip(2); // padding
        var entries = new AccessControlEntry[aceCount];
        for (int i = 0; i < aceCount; ++i)
            entries[i] = AccessControlEntry.CreateFromStream(ref reader);
        
        return new AccessControlList(revision, aclSize, aceCount, entries); 
    }
}

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

        return new AccessControlEntry(type, flags, size, new AccessMask((int)accessMask), new SecurityId(sId.ToArray()));
    }
}

public enum AceType : byte
{
    AccessAllowed = 0x00,
    AccessDenied = 0x01,
    SystemAudit = 0x02
}

[Flags]
public enum AceFlags : byte
{
    ObjectInheritsAce = 0x01,
    ContainerInheritsAce = 0x02,
    DontPropagateInheritAce = 0x04,
    InheritOnlyAce = 0x08,
    AuditOnSuccess = 0x40,
    AuditOnFailure = 0x80
}