using System.Security.Principal;

namespace NtfsParser.Mft.ParsedAttributeData.SecurityDescriptor;

public readonly record struct SecurityId(byte[] SId)
{
    public SecurityIdentifier ToDotnetVariant() => new(SId, 0);
}