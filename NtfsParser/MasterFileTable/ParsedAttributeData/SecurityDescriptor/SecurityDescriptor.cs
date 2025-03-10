using NtfsParser.MasterFileTable.AttributeRecord;

namespace NtfsParser.MasterFileTable.ParsedAttributeData.SecurityDescriptor;

public record struct SecurityDescriptor(SecurityDescriptorHeader Header, AccessControlList Sacl, AccessControlList Dacl, 
    SecurityId UserSid, SecurityId GroupSid)
{
    public static SecurityDescriptor CreateFromRawData(RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var rawHeader = reader.ReadBytes(20);
        var header = SecurityDescriptorHeader.Parse(rawHeader);
        AccessControlList sacl;
        if (header.ControlFlags.HasFlag(SecurityDescriptorControlFlags.SaclPresent))
        {
            var boundary = (int)(header.ControlFlags.HasFlag(SecurityDescriptorControlFlags.DaclPresent)
                ? header.OffsetToDacl
                : header.OffsetToUserSid);
            reader.Position = (int)header.OffsetToSacl;
            var rawSacl = reader.ReadBytes(boundary - reader.Position);
            sacl = AccessControlList.Parse(rawSacl);
        }
        else
        {
            sacl = default;
        }

        AccessControlList dacl;
        if (header.ControlFlags.HasFlag(SecurityDescriptorControlFlags.DaclPresent))
        {
            var boundary = (int)header.OffsetToUserSid;
            reader.Position = (int)header.OffsetToDacl;
            var rawDacl = reader.ReadBytes(boundary - reader.Position);
            dacl = AccessControlList.Parse(rawDacl);
        }
        else
        {
            dacl = default;
        }

        reader.Position = (int)header.OffsetToUserSid;
        var userSid = reader.ReadBytes((int)header.OffsetToGroupSid - reader.Position);
        reader.Position = (int)header.OffsetToGroupSid;
        var groupSid = reader.ReadBytes(data.Length - reader.Position);
        return new SecurityDescriptor(header, sacl, dacl, new SecurityId(userSid.ToArray()), 
            new SecurityId(groupSid.ToArray()));
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