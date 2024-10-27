// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		C5D3Ifd3.cs
// AUTHOR:		Greg Eakin

using System.IO;
using System.Linq;
using PhotoLib.Tiff;
using Xunit;

namespace PhotoTests.Canon5D3;

public class C5D3Ifd3
{
    private const string FileName = @"P:\2018\2018-08-29\0L2A3743.CR2";

    public C5D3Ifd3()
    {
        Assert.True(File.Exists(FileName), $"Image file {FileName} doesn't exists!");
    }

    //== Tiff Directory [0x00011964]:
    //0)  0x0103 UShort 16-bit: 6
    //1)  0x0111 ULong 32-bit: 4223344
    //2)  0x0117 ULong 32-bit: 25591542
    //3)  0xC5D8 ULong 32-bit: 1
    //4)  0xC5E0 ULong 32-bit: 1
    //5)  0xC640 UShort 16-bit: [0x000119BE] (3): 1, 2960, 2960, 
    //6)  0xC6C5 ULong 32-bit: 1

    [Fact]
    public void DumpImageFileDirectory()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var imageFileDirectory = rawImage.Directories.Skip(3).First();
        imageFileDirectory.DumpDirectory(binaryReader);
    }

    [Fact]
    public void Compression()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0103 UShort 16-bit: 6
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

        // 0x0111 ULong 32-bit: 4223344
        var imageFileDirectory = rawImage.Directories.Last();
        var imageFileEntry = imageFileDirectory[0x0111];
        Assert.Equal(ImageFileEntry.TagTypes.ULong, imageFileEntry.TagType);
        Assert.Equal(0x0111, imageFileEntry.TagId);
        // Assert.Equal(4223344u, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);
    }

    [Fact]
    public void StripByteCounts()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0117 ULong 32-bit: 25591542 
        var imageFileDirectory = rawImage.Directories.Last();
        var imageFileEntry = imageFileDirectory[0x0117];
        Assert.Equal(ImageFileEntry.TagTypes.ULong, imageFileEntry.TagType);
        Assert.Equal(0x0117, imageFileEntry.TagId);
        // Assert.Equal(25591542u, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);
    }

    [Fact]
    public void Cr2Slice()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0xC640 UShort 16-bit: [0x000119BE] (3): 1, 2960, 2960, 
        var imageFileDirectory = rawImage.Directories.Last();
        var imageFileEntry = imageFileDirectory[0xC640];
        Assert.Equal(ImageFileEntry.TagTypes.UShort, imageFileEntry.TagType);
        Assert.Equal(0x000119BEu, imageFileEntry.ValuePointer);
        Assert.Equal(3u, imageFileEntry.NumberOfValue);

        Assert.Equal(new[] { (ushort)1, (ushort)2960, (ushort)2960 },
            RawImage.ReadUInts16(binaryReader, imageFileEntry));
    }
}
