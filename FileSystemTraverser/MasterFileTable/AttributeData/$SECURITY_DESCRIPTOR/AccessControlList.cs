namespace FileSystemTraverser.MasterFileTable.AttributeData._SECURITY_DESCRIPTOR;

public record struct AccessControlList(byte Revision, ushort AclSize, ushort AceCount, AccessControlEntry[] Entries)
{
    public static AccessControlList CreateFromStream(BinaryReader reader)
    {
        var revision = reader.ReadByte();
        reader.BaseStream.Position += 1; // padding
        var aclSize = reader.ReadUInt16();
        var aceCount = reader.ReadUInt16();
        reader.BaseStream.Position += 2; // padding
        
        var entries = new AccessControlEntry[aceCount];
        for (int i = 0; i < aceCount; ++i)
        {
            entries[i] = AccessControlEntry.CreateFromStream(reader);
        }
        
        return new AccessControlList(revision, aclSize, aceCount, entries); 
    }
}