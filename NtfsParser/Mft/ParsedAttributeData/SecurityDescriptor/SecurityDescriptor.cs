using NtfsParser.Mft.Attribute;

namespace NtfsParser.Mft.ParsedAttributeData.SecurityDescriptor;

/// <summary>
/// An attribute used to control access to files and folders. Most descriptors are stored in the $Secure meta file,
/// however some of the files will still have this attribute 
/// </summary>
/// <param name="Header">Descriptor's header</param>
/// <param name="SystemList">Access list used to log access</param>
/// <param name="DiscretionaryList">Access list used to control access</param>
/// <param name="UserSid">User SID</param>
/// <param name="GroupSid">Group SID</param>
public readonly record struct SecurityDescriptor(SecurityDescriptorHeader Header, AccessControlList SystemList, AccessControlList DiscretionaryList, 
    SecurityId UserSid, SecurityId GroupSid)
{
    public static SecurityDescriptor CreateFromRawData(in RawAttributeData rawData)
    {
        var data = rawData.Data.AsSpan();
        var reader = new SpanBinaryReader(data);
        var rawHeader = reader.ReadBytes(20);
        var header = SecurityDescriptorHeader.Parse(rawHeader);
        AccessControlList sacl = default;
        if (header.ControlFlags.HasFlag(SecurityDescriptorControlFlags.SaclPresent))
        {
            var boundary = (int)(header.ControlFlags.HasFlag(SecurityDescriptorControlFlags.DaclPresent)
                ? header.OffsetToDacl
                : header.OffsetToUserSid);
            reader.Position = (int)header.OffsetToSacl;
            var rawSacl = reader.ReadBytes(boundary - reader.Position);
            sacl = AccessControlList.Parse(rawSacl);
        }

        AccessControlList dacl = default;
        if (header.ControlFlags.HasFlag(SecurityDescriptorControlFlags.DaclPresent))
        {
            var boundary = (int)header.OffsetToUserSid;
            reader.Position = (int)header.OffsetToDacl;
            var rawDacl = reader.ReadBytes(boundary - reader.Position);
            dacl = AccessControlList.Parse(rawDacl);
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
    SaclAutoInherited = 0x0800,
    DaclProtected = 0x1000,
    SaclProtected = 0x2000,
    RmControlValid = 0x4000,
    SelfRelative = 0x8000,
}

/// <summary>
/// Descriptor's header that contains information about the layout
/// </summary>
/// <param name="Revision">Revision. Currently, is set to 2</param>
/// <param name="ControlFlags">Flags</param>
/// <param name="OffsetToUserSid">Offset to the user's security ID within the raw descriptor</param>
/// <param name="OffsetToGroupSid">Offset to the group's security ID within the raw descriptor</param>
/// <param name="OffsetToSacl">Offset to the system access list within the raw descriptor</param>
/// <param name="OffsetToDacl">Offset to the discretionary access list within the raw descriptor</param>
public record struct SecurityDescriptorHeader(byte Revision, SecurityDescriptorControlFlags ControlFlags, uint OffsetToUserSid,
    uint OffsetToGroupSid, uint OffsetToSacl, uint OffsetToDacl)
{
    public static SecurityDescriptorHeader Parse(Span<byte> rawData)
    {
        var reader = new SpanBinaryReader(rawData);
        var revision = reader.ReadByte();
        reader.Skip(1); // padding
        var controlFlags = (SecurityDescriptorControlFlags)reader.ReadUInt16();
        var offsetToUserSid = reader.ReadUInt32();
        var offsetToGroupSid = reader.ReadUInt32();
        var offsetToSacl = reader.ReadUInt32();
        var offsetToDacl = reader.ReadUInt32();

        return new SecurityDescriptorHeader(revision, controlFlags, offsetToUserSid, offsetToGroupSid, offsetToSacl,
            offsetToDacl);
    }
}