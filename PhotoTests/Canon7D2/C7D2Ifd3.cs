// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		C7D2Ifd3.cs
// AUTHOR:		Greg Eakin

using System.IO;
using System.Linq;
using PhotoLib.Tiff;
using Xunit;

namespace PhotoTests.Canon7D2;

public class C7D2Ifd3
{
    //private const string FileName = @"P:\Source\7D2high.CR2";
    private const string FileName = @"P:\2018\2018-10-19\B05A1194.CR2";
        
    public C7D2Ifd3()
    {
        Assert.True(File.Exists(FileName), $"Image file {FileName} doesn't exists!");
    }

    [Fact]
    public void TestMethod1()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        Assert.Equal(new byte[] { 0x49, 0x49 }, rawImage.Header.ByteOrder);
        Assert.Equal(0x002A, rawImage.Header.TiffMagic);
        Assert.Equal(0x5243, rawImage.Header.CR2Magic);
        Assert.Equal(new byte[] { 0x02, 0x00 }, rawImage.Header.CR2Version);

        rawImage.DumpHeader(binaryReader);
    }

    // == Tiff Directory [0x00007222]:
    // IFD3:0x0100  0)  UShort 16-bit: 5568
    // IFD3:0x0101  1)  UShort 16-bit: 3708
    // IFD3:0x0103  2)  UShort 16-bit: 6
    // IFD3:0x0111  3)  ULong 32-bit: 3650732
    // IFD3:0x0117  4)  ULong 32-bit: 26098618
    // IFD3:0xC5D8  5)  ULong 32-bit: 1
    // IFD3:0xC5E0  6)  ULong 32-bit: 1
    // IFD3:0xC640  7)  UShort 16-bit: [0x00007294] (3): 1, 2784, 2784, 
    // IFD3:0xC6C5  8)  ULong 32-bit: 1

    [Fact]
    public void Compression()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0103 UShort 16-bit: 6
        // var imageFileDirectory = rawImage[0x0000BF46];
        var imageFileDirectory = rawImage.Directories.Last();
        var imageFileEntry = imageFileDirectory[0x0103];
        Assert.Equal(ImageFileEntry.TagTypes.UShort, imageFileEntry.TagType);
        Assert.Equal(6u, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);
    }

    [Fact]
    public void StripOffset()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0111 ULong 32-bit: 3213024
        // var imageFileDirectory = rawImage[0x0000BF46];
        var imageFileDirectory = rawImage.Directories.Last();
        var imageFileEntry = imageFileDirectory[0x0111];
        Assert.Equal(0x0111, imageFileEntry.TagId);
        Assert.Equal(ImageFileEntry.TagTypes.ULong, imageFileEntry.TagType);
        // Assert.Equal(3213024u, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);
    }

    [Fact]
    public void StripByteCounts()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0117 ULong 32-bit: 22286138 
        // var imageFileDirectory = rawImage[0x0000BF46];
        var imageFileDirectory = rawImage.Directories.Last();
        var imageFileEntry = imageFileDirectory[0x0117];
        Assert.Equal(0x0117, imageFileEntry.TagId);
        Assert.Equal(ImageFileEntry.TagTypes.ULong, imageFileEntry.TagType);
        // Assert.Equal(22286138u, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);
    }

    [Fact]
    public void Cr2Slice()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0xC640 UShort 16-bit: [0x0000BFA0] (3): 2, 1728, 1904, 
        // var imageFileDirectory = rawImage[0x0000BF46];
        var imageFileDirectory = rawImage.Directories.Last();
        var imageFileEntry = imageFileDirectory[0xC640];
        Assert.Equal(ImageFileEntry.TagTypes.UShort, imageFileEntry.TagType);
        Assert.Equal(0x00007294u, imageFileEntry.ValuePointer);
        Assert.Equal(3u, imageFileEntry.NumberOfValue);

        Assert.Equal(new[] { (ushort)1, (ushort)2784, (ushort)2784 },
            RawImage.ReadUInts16(binaryReader, imageFileEntry));
    }
}
