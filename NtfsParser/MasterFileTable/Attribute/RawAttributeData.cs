using NtfsParser.MasterFileTable.ParsedAttributeData;
using NtfsParser.MasterFileTable.ParsedAttributeData.AttributeList;
using NtfsParser.MasterFileTable.ParsedAttributeData.ExtendedAttribute;
using NtfsParser.MasterFileTable.ParsedAttributeData.IndexAllocation;
using NtfsParser.MasterFileTable.ParsedAttributeData.IndexRoot;
using NtfsParser.MasterFileTable.ParsedAttributeData.ReparsePoint;
using NtfsParser.MasterFileTable.ParsedAttributeData.SecurityDescriptor;

namespace NtfsParser.MasterFileTable.Attribute;

public record struct RawAttributeData(byte[] Data)
{
    public StandardInformation ToStandardInformation() => StandardInformation.CreateFromRawData(this);
    public AttributeList ToAttributeList() => AttributeList.CreateFromRawData(this);
    public FileName ToFileName() => FileName.CreateFromRawData(this);
    public ObjectId ToObjectId() => ObjectId.CreateFromRawData(this);
    public SecurityDescriptor ToSecurityId() => SecurityDescriptor.CreateFromRawData(this);
    public VolumeName ToVolumeName() => VolumeName.CreateFromRawData(this);
    public VolumeInformation ToVolumeInformation() => VolumeInformation.CreateFromRawData(this);
    public Data ToData() => ParsedAttributeData.Data.CreateFromRawData(this);
    public IndexRoot ToIndexRoot() => IndexRoot.CreateFromRawData(this);
    public IndexAllocation ToIndexAllocation(uint indexRecordSize, int sectorByteSize) 
        => IndexAllocation.CreateFromRawData(this, indexRecordSize, sectorByteSize);
    
    public Bitmap ToBitmap() => Bitmap.CreateFromRawData(this);
    public ReparsePoint ToReparsePoint() => ReparsePoint.CreateFromRawData(this);
    public EaInformation ToEaInformation() => EaInformation.CreateFromRawData(this);
    public ExtendedAttribute ToExtendedAttribute() => ExtendedAttribute.CreateFromRawData(this);
    public LoggedUtilityStream ToLoggedUtilityStream() => LoggedUtilityStream.CreateFromRawData(this);
}