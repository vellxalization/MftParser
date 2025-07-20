# C# library to read and parse Master File Table of an NTFS volume
An NTFS volume consists of two zones: the MFT and the rest of the space to store actual file data. The Master File Table, as the name suggests, is a table that contains at least one entry for each file on the disk. The purpose of this library is to ease the process of reading and parsing MFT entries.
## How to use this library
The core functionality is concentrated in three classes: RawVolume, MftReader and VolumeDataReader (both latter are a part of each RawVolume).
* Create a RawVolume instance by providing a volume letter. 
* Use the RawVolume's MftReader to read MFT entries. 
* Use the RawVolume's VolumeDataReader to read data of the nonresident attributes.

Each MFT entry contains an array of MftAttributes that are used to store various data about the file.
<b>Keep in mind that some entries might contain too many attributes to keep them in a single MFT entry. In this case, a special AttributeList is created to help locate the rest of the attributes.</b> Use the MftAttribute's GetAttributeData() method (the method will automatically handle both sparse and compressed attributes) to get an instance of RawAttributeData that can be transformed to an appropriate data type using instance methods.
## Examples
The solution also contains three demo projects that demonstrate the usage of the library.
* A simple program that loops through the whole MFT and prints each entry's attributes.
* A file searcher that uses the $I30 index and depth-first search to find files.
* A file browser that uses the $I30 index to scan and display volume's folders and files.
## Information sources
* File System Forensic Analysis by Brian Carries. Great book about file systems in general with a lot of useful information and details. Helped a lot to get my head around indices in NTFS.
* NTFS documentation from the libfsntfs' GitHub repository (https://github.com/libyal/libfsntfs/blob/main/documentation/New%20Technologies%20File%20System%20(NTFS).asciidoc). Another great source of information about NTFS. Helped with some of the more obscure details about the file system.
* Richard Russon's (flatcap) NTFS documentation (https://github.com/flatcap/ntfs-docs). Again, a great source of information about NTFS.