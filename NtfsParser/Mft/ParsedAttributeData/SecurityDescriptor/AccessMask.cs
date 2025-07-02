using System.Text;

namespace NtfsParser.Mft.ParsedAttributeData.SecurityDescriptor;

/// <summary>
/// Access mask used by access lists
/// </summary>
/// <param name="Value">Raw 32-bit value</param>
public readonly record struct AccessMask(uint Value)
{
    private const uint GenericRightsMask = 0xF0000000;
    /// <summary>
    /// Returns most significant bits 0-3 as generic rights enum
    /// </summary>
    public GenericAccessRights GenericRights => (GenericAccessRights)((Value & GenericRightsMask) >> 28);

    private const uint MaximumAllowedMask = 1 << 26;
    
    /// <summary>
    /// Returns boolean value signaling whether then 6th most significant bit is set
    /// </summary>
    public bool MaximumAllowed => (Value & MaximumAllowedMask) != 0;
    
    private const uint AccessSystemSecurityMask = 1 << 25;
    /// <summary>
    /// Returns boolean value signaling whether then 7th most significant bit is set
    /// </summary>
    public bool AccessSystemSecurity => (Value & AccessSystemSecurityMask) != 0;
    
    private const uint StandardAccessMask = 0x1F0000;
    /// <summary>
    /// Returns most significant bits 11-15 as standard rights enum
    /// </summary>
    public StandardAccessRights StandardRights => (StandardAccessRights)((Value & StandardAccessMask) >> 16);
    
    private const ushort SpecificRightsMask = 0xFFFF;
    /// <summary>
    /// Returns most significant bits 16-31 as ushort value. Since the meaning of these flags are object-specific, we don't format them
    /// </summary>
    public ushort SpecificRights => (ushort)(Value & SpecificRightsMask);

    // public override string ToString()
    // {
    //     var sb = new StringBuilder();
    //     sb.Append($"{((Value & GenericRightsMask) >> 28):b4}");
    //     sb.Append('_');
    //     sb.Append("RR");
    //     sb.Append('_');
    //     sb.Append(MaximumAllowed ? '1' : '0');
    //     sb.Append('_');
    //     sb.Append(AccessSystemSecurity ? '1' : '0');
    //     sb.Append('_');
    //     sb.Append("RRR");
    //     sb.Append('_');
    //     sb.Append($"{((Value & StandardAccessMask) >> 16):B5}");
    //     sb.Append('_');
    //     sb.Append($"{SpecificRights:B16}");
    //     sb.Append(Environment.NewLine);
    //     sb.Append($"Generic: {GenericRights}");
    //     sb.Append(Environment.NewLine);
    //     sb.Append($"Standard: {StandardRights}");
    //     return sb.ToString();
    // }
}

[Flags]
public enum GenericAccessRights : sbyte
{
    GenericRead = 0b_1000,
    GenericWrite = 0b_0100,
    GenericExecute = 0b_0010,
    GenericAll = 0b_0001,
}

[Flags]
public enum StandardAccessRights : ushort
{
    Synchronize = 0b_10000,
    WriteOwner = 0b_01000,
    WriteDacl = 0b_00100,
    ReadControl = 0b_00010,
    Delete = 0b_00001,
}