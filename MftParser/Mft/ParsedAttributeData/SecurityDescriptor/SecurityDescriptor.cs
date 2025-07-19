using MftParser.Mft.Attribute;

namespace MftParser.Mft.ParsedAttributeData.SecurityDescriptor;

/// <summary>
/// An attribute used to control access to files and folders. Most descriptors are stored in the $Secure meta file,
/// however some of the records will still have this attribute 
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

/// <summary>
/// Descriptor's control flags
/// </summary>
[Flags]
public enum SecurityDescriptorControlFlags : ushort
{
    /// <summary>
    /// Owner's SID was provided by a default mechanism
    /// </summary>
    OwnerDefaulted = 0x0001,
    /// <summary>
    /// Group's SID was provided by a default mechanism
    /// </summary>
    GroupDefaulted = 0x0002,
    /// <summary>
    /// Descriptor have a discretionary access list. If the flag is present and the list is null,
    /// then the descriptor allows access to everyone
    /// </summary>
    DaclPresent = 0x0004,
    /// <summary>
    /// Descriptor's DACL is default (e.g. access token's default DACL)
    /// </summary>
    DaclDefaulted = 0x0008,
    /// <summary>
    /// Descriptor have a system access list
    /// </summary>
    SaclPresent = 0x0010,
    /// <summary>
    /// SACL is provided by a default mechanism
    /// </summary>
    SaclDefaulted = 0x0020,
    /// <summary>
    /// Required descriptor in which the DACL supports automatic propagation of inheritable ACEs to existing child objects
    /// </summary>
    DaclAutoInheritReq = 0x0100,
    /// <summary>
    /// Required descriptor in which the SACL supports automatic propagation of inheritable ACEs to existing child objects
    /// </summary>
    SaclAutoInheritReq = 0x0200,
    /// <summary>
    /// DACL supports automatic propagation of inheritable ACEs to existing child objects
    /// </summary>
    DaclAutoInherited = 0x0400,
    /// <summary>
    /// SACL supports automatic propagation of inheritable ACEs to existing child objects
    /// </summary>
    SaclAutoInherited = 0x0800,
    /// <summary>
    /// DACL is protected from being modified by inheritable ACEs
    /// </summary>
    DaclProtected = 0x1000,
    /// <summary>
    /// SACL is protected from being modified by inheritable ACEs
    /// </summary>
    SaclProtected = 0x2000,
    /// <summary>
    /// Resource manager control is valid
    /// </summary>
    RmControlValid = 0x4000,
    /// <summary>
    /// Descriptor is self-relative
    /// </summary>
    SelfRelative = 0x8000,
}

/// <summary>
/// Descriptor's header that contains information about the layout
/// </summary>
/// <param name="Revision">Revision. Current descriptors revision in 0x1</param>
/// <param name="ControlFlags">Flags</param>
/// <param name="OffsetToUserSid">Offset to the start of the user SID from the start of the struct</param>
/// <param name="OffsetToGroupSid">Offset to the start of the group SID from the start of the descriptor</param>
/// <param name="OffsetToSacl">Offset to the start of the system access list from the start of the descriptor</param>
/// <param name="OffsetToDacl">Offset to the discretionary access list from the start of the descriptor</param>
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