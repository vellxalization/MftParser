namespace FileSystemTraverser.MasterFileTable.AttributeData._SECURITY_DESCRIPTOR;

public record struct SecurityDescriptorHeader(byte Revision, SecurityDescriptorControlFlags ControlFlags, uint OffsetToUserSid, 
    uint OffsetToGroupSid, uint OffsetToSacl, uint OffsetToDacl)
{
    public static SecurityDescriptorHeader CreateFromStream(BinaryReader reader)
    {
        var revision = reader.ReadByte();
        reader.BaseStream.Position += 1; // padding
        var controlFlags = (SecurityDescriptorControlFlags)reader.ReadUInt16();
        var offsetToUserSid = reader.ReadUInt32();
        var offsetToGroupSid = reader.ReadUInt32();
        var offsetToSacl = reader.ReadUInt32();
        var offsetToDacl = reader.ReadUInt32();

        return new SecurityDescriptorHeader(revision, controlFlags, offsetToUserSid, offsetToGroupSid, offsetToSacl,
            offsetToDacl);
    }
}