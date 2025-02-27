namespace NtfsParser.MasterFileTable.ParsedAttributeData.SecurityDescriptor;

public record struct AccessMask
{
    public AccessMask(int mask)
    {
        Rights = (AccessRights)mask;
    }
    
    public AccessRights Rights { get; }

    public NonfolderAccessRights GetSpecificRightsAsNonFolder()
    {
        var value = (int)Rights & 0b_11111111_11111111;
        return (NonfolderAccessRights)value;
    }

    public FolderAccessRights GetSpecificRightsAsFolder()
    {
        var value = (int)Rights & 0b_11111111_11111111;
        return (FolderAccessRights)value;
    }

    public MandatoryLabelAccessRights GetSpecificRightsAsMandatoryLabel()
    {
        var value = (int)Rights & 0b_11111111_11111111;
        return (MandatoryLabelAccessRights)value;
    }
}

[Flags]
public enum AccessRights
{
    Delete = 1 << 16,
    ReadControl = 1 << 17,
    WriteDac = 1 << 18,
    WriteOwner = 1 << 19,
    Synchronize = 1 << 20,
    AccessSystemSecurity = 1 << 24,
    MaximumAllowed = 1 << 25,
    GenericAll = 1 << 28,
    GenericExecute = 1 << 29,
    GenericWrite = 1 << 30,
    GenericRead = 1 << 31
}

[Flags]
public enum NonfolderAccessRights
{
    ReadData = 1,
    WriteData = 1 << 1,
    AppendMsg = 1 << 2,
    ReadEa = 1 << 3,
    WriteEa = 1 << 4,
    Execute = 1 << 5,
    ReadAttributes = 1 << 7,
    WriteAttributes = 1 << 8,
    WriteOwnProperty = 1 << 9,
    DeleteOwnItem = 1 << 10,
    ViewItem = 1 << 11
}

[Flags]
public enum FolderAccessRights
{
    ListDirectory = 1,
    AddFile = 1 << 1,
    AddSubdirectory = 1 << 2,
    ReadEa = 1 << 3,
    WriteEa = 1 << 4,
    ReadAttributes = 1 << 7,
    WriteAttributes = 1 << 8,
    WriteOwnProperty = 1 << 9,
    DeleteOwnItem = 1 << 10,
    ViewItem = 1 << 11,
    Owner = 1 << 14,
    Contact = 1 << 15
}

[Flags]
public enum MandatoryLabelAccessRights
{
    NoWriteUp = 1,
    NoReadUp = 1 << 1,
    NoExecuteUp = 1 << 2
}