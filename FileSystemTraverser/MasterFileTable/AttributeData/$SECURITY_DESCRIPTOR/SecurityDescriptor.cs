namespace FileSystemTraverser.MasterFileTable.AttributeData._SECURITY_DESCRIPTOR;

public record struct SecurityDescriptor(SecurityDescriptorHeader Header, AccessControlList Sacl, AccessControlList Dacl, 
    SecurityId UserSid, SecurityId GroupSid)
{
    public static SecurityDescriptor CreateFromData(ref byte[] data, int validDataSize)
    {
        using var reader = new BinaryReader(new MemoryStream(data));
        var header = SecurityDescriptorHeader.CreateFromStream(reader);
        AccessControlList sacl;
        if (header.ControlFlags.HasFlag(SecurityDescriptorControlFlags.SaclPresent))
        {
            reader.BaseStream.Position = header.OffsetToSacl;
            sacl = AccessControlList.CreateFromStream(reader);
        }
        else
        {
            sacl = default;
        }

        AccessControlList dacl;
        if (header.ControlFlags.HasFlag(SecurityDescriptorControlFlags.DaclPresent))
        {
            reader.BaseStream.Position = header.OffsetToDacl;
            dacl = AccessControlList.CreateFromStream(reader);
        }
        else
        {
            dacl = default;
        }

        reader.BaseStream.Position = header.OffsetToUserSid;
        var userSid = SecurityId.CreateFromStream(reader, (int)(header.OffsetToGroupSid - reader.BaseStream.Position));
        reader.BaseStream.Position = header.OffsetToGroupSid;
        var groupSid = SecurityId.CreateFromStream(reader, validDataSize - (int)reader.BaseStream.Position);
        return new SecurityDescriptor(header, sacl, dacl, userSid, groupSid);
    }
}

[Flags]
public enum SecurityDescriptorControlFlags : ushort
{
    OwnerDefaulted = 0x0001,
    GroupDefaulted = 0x0002,
    DaclPresent = 0x0004,
    DaclDefaulted = 0x0008,
    SaclPresent = 0x0010,
    SaclDefaulted = 0x0020,
    DaclAutoInheritReq = 0x0100,
    SaclAutoInheritReq = 0x0200,
    DaclAutoInherited = 0x0400,
    DaclProtected = 0x1000,
    SaclProtected = 0x2000,
    RmControlValid = 0x4000,
    SelfRelative = 0x8000
}