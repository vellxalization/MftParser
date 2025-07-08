using NtfsParser.Mft.ParsedAttributeData;
using NtfsParser.Mft.ParsedAttributeData.Index;
using NtfsParser.Mft.ParsedAttributeData.ReparsePoint;
using NtfsParser.Mft.ParsedAttributeData.SecurityDescriptor;

namespace NtfsParser.Mft.Attribute;

/// <summary>
/// Attribute's data. Use object's methods to cast it to the specific attribute type.
/// None of the methods check the data's type and assume that the caller calls the appropriate method
/// </summary>
/// <param name="Data">Raw data</param>
public readonly record struct RawAttributeData(byte[] Data)
{
    public StandardInformation ToStandardInformation() => StandardInformation.CreateFromRawData(this);
    public AttributeList ToAttributeList() => AttributeList.CreateFromRawData(this);
    public FileName ToFileName() => FileName.CreateFromRawData(this);
    public ObjectId ToObjectId() => ObjectId.CreateFromRawData(this);
    public SecurityDescriptor ToSecurityDescriptor() => SecurityDescriptor.CreateFromRawData(this);
    public VolumeName ToVolumeName() => VolumeName.CreateFromRawData(this);
    public VolumeInformation ToVolumeInformation() => VolumeInformation.CreateFromRawData(this);
    public Data ToData() => ParsedAttributeData.Data.CreateFromRawData(this);
    public IndexRoot ToIndexRoot() => IndexRoot.CreateFromRawData(this);
    public IndexAllocation ToIndexAllocation(int indexRecordSize, int sectorByteSize) 
        => IndexAllocation.CreateFromRawData(this, indexRecordSize, sectorByteSize);
    
    public Bitmap ToBitmap() => Bitmap.CreateFromRawData(this);
    public ReparsePoint ToReparsePoint() => ReparsePoint.CreateFromRawData(this);
    public EaInformation ToEaInformation() => EaInformation.CreateFromRawData(this);
    public ExtendedAttribute ToExtendedAttribute() => ExtendedAttribute.CreateFromRawData(this);
    public LoggedUtilityStream ToLoggedUtilityStream() => LoggedUtilityStream.CreateFromRawData(this);
}