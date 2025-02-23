namespace FileSystemTraverser.MasterFileTable.ParsedAttributeData.SecurityDescriptor;

public record struct AccessControlEntry(AceType Type, AceFlags Flags, ushort Size, uint AccessMask, SecurityId SecurityId)
{
    public static AccessControlEntry CreateFromStream(ref SpanBinaryReader reader)
    {
        var start = reader.Position;
        var type = (AceType)reader.ReadByte();
        var flags = (AceFlags)reader.ReadByte();
        var size = reader.ReadUInt16();
        // TODO: create a struct to properly represent access mask
        var accessMask = reader.ReadUInt32();
        var sId = reader.ReadBytes(size - (reader.Position - start));
        
        return new AccessControlEntry(type, flags, size, accessMask, new SecurityId(sId.ToArray()));
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