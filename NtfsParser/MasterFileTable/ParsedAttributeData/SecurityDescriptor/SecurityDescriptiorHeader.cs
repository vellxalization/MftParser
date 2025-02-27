namespace NtfsParser.MasterFileTable.ParsedAttributeData.SecurityDescriptor;

public record struct SecurityDescriptorHeader(byte Revision, SecurityDescriptorControlFlags ControlFlags, uint OffsetToUserSid, 
    uint OffsetToGroupSid, uint OffsetToSacl, uint OffsetToDacl)
{
    public static SecurityDescriptorHeader Parse(ReadOnlySpan<byte> rawData)
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