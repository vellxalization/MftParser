namespace FileSystemTraverser.MasterFileTable.ParsedAttributeData.SecurityDescriptor;

public record struct AccessControlList(byte Revision, ushort AclSize, ushort AceCount, AccessControlEntry[] Entries)
{
    public static AccessControlList Parse(ReadOnlySpan<byte> rawAcl)
    {
        var reader = new SpanBinaryReader(rawAcl);
        var revision = reader.ReadByte();
        reader.Skip(1); // padding
        var aclSize = reader.ReadUInt16();
        var aceCount = reader.ReadUInt16();
        reader.Skip(2); // padding
        var entries = new AccessControlEntry[aceCount];
        for (int i = 0; i < aceCount; ++i)
        {
            entries[i] = AccessControlEntry.CreateFromStream(ref reader);
        }
        
        return new AccessControlList(revision, aclSize, aceCount, entries); 
    }
}