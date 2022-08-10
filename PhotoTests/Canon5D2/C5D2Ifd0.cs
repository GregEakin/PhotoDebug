// Copyright © 2013-2021. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		C5D3Ifd1.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;
using System.Linq;
using PhotoLib.Tiff;
using Xunit;
using Xunit.Abstractions;

namespace PhotoTests.Canon5D2;

public class C5D2Ifd0
{
    private const string FileName = @"\\Data\Photo\2022\2022-07-25\IMG_3975.CR2";
    private readonly ITestOutputHelper _testOutputHelper;

    public C5D2Ifd0(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        Assert.True(File.Exists(FileName), $"Image file {FileName} doesn't exists!");
    }

    [Fact]
    public void DumpImageFileDirectory()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var imageFileDirectory = rawImage.Directories.First();

        var directory = imageFileDirectory.DumpDirectory(binaryReader);
        _testOutputHelper.WriteLine(directory);
    }

    // == Tiff Directory[0x00000010]:
    // ImageWidth           :0x0100  0)  UShort 16-bit: 5616
    // ImageLength          :0x0101  1)  UShort 16-bit: 3744
    // BitsPerSample        :0x0102  2)  UShort 16-bit: [0x000000E2] (3): 8, 8, 8, 
    // Compression          :0x0103  3)  UShort 16-bit: 6
    // Make                 :0x010F  4)  Ascii 8-bit, null terminated: [0x000000E8] (6): "Canon"
    // Model                :0x0110  5)  Ascii 8-bit, null terminated: [0x000000EE] (21): "Canon EOS 5D Mark II"
    // StripOffsets         :0x0111  6)  ULong 32-bit: 55284
    // Orientation          :0x0112  7)  UShort 16-bit: 1
    // StripByteCounts      :0x0117  8)  ULong 32-bit: 1135633
    // XResolution          :0x011A  9)  URational 2x32-bit: [0x0000010E] (1): 72/1 = 72
    // YResolution          :0x011B 10)  URational 2x32-bit: [0x00000116] (1): 72/1 = 72
    // ResolutionUnit       :0x0128 11)  UShort 16-bit: 2
    // DateTime             :0x0132 12)  Ascii 8-bit, null terminated: [0x0000011E] (20): "2022:07:25 12:09:41"
    // Artist               :0x013B 13)  Ascii 8-bit, null terminated: [0x00000000] (1): ""
    // Copyright            :0x8298 14)  Ascii 8-bit, null terminated: [0x00000172] (16): "Copyright: 2022"
    // EXIF IFD Pointer     :0x8769 15)  Image File Directory: [0x000001B2] (1): 
    // GPS Info IFD Pointer :0x8825 16)  GPS Info: [0x0000A572] (1): 

    [Fact]
    public void TagsInDirectory()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var imageFileDirectory = rawImage.Directories.First();

        Assert.Equal(
            new ushort[]
            {
                0x0100, 0x0101, 0x0102, 0x0103, 0x010F, 0x0110, 0x0111, 0x0112, 0x0117, 0x011A, 0x011B, 0x0128,
                0x0132, 0x013B, 0x8298, 0x8769, 0x8825
            },
            imageFileDirectory.Entries.Select(e => e.TagId).ToArray());
    }

    [Fact]
    public void ImageWidth()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0100 UShort 16-bit: 5760
        var imageFileDirectory = rawImage.Directories.First();
        var imageFileEntry = imageFileDirectory[0x0100];
        Assert.Equal(3, imageFileEntry.TagType);
        Assert.Equal(5616u, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);
    }

    [Fact]
    public void ImageLength()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0101 UShort 16-bit: 3840
        var imageFileDirectory = rawImage.Directories.First();
        var imageFileEntry = imageFileDirectory[0x0101];
        Assert.Equal(3, imageFileEntry.TagType);
        Assert.Equal(3744u, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);
    }

    [Fact]
    public void BitsPerSample()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0102 UShort 16-bit: [0x000000EE] (3): 8, 8, 8, 
        var imageFileDirectory = rawImage.Directories.First();
        var imageFileEntry = imageFileDirectory[0x0102];
        Assert.Equal(3, imageFileEntry.TagType);
        Assert.Equal(226u, imageFileEntry.ValuePointer);
        Assert.Equal(3u, imageFileEntry.NumberOfValue);

        Assert.Equal(new[] { (ushort)8, (ushort)8, (ushort)8 },
            RawImage.ReadUInts16(binaryReader, imageFileEntry));
    }

    [Fact]
    public void Compression()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0103 UShort 16-bit: 6
        var imageFileDirectory = rawImage.Directories.First();
        var imageFileEntry = imageFileDirectory[0x0103];
        Assert.Equal(3, imageFileEntry.TagType);
        Assert.Equal(6u, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);
    }

    [Fact]
    public void Maker()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x010F Ascii 8-bit: [0x000000C4] (6): Canon
        var imageFileDirectory = rawImage.Directories.First();
        var imageFileEntry = imageFileDirectory[0x010F];
        Assert.Equal(2, imageFileEntry.TagType);
        Assert.Equal(0x000000E8u, imageFileEntry.ValuePointer);
        Assert.Equal(6u, imageFileEntry.NumberOfValue);

        Assert.Equal("Canon", RawImage.ReadChars(binaryReader, imageFileEntry));
    }

    [Fact]
    public void Model()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0110 Ascii 8-bit: [0x000000CA] (22): Canon EOS 5D Mark II
        var imageFileDirectory = rawImage.Directories.First();
        var imageFileEntry = imageFileDirectory[0x0110];
        Assert.Equal(2, imageFileEntry.TagType);
        Assert.Equal(0x000000EEu, imageFileEntry.ValuePointer);
        Assert.Equal(21u, imageFileEntry.NumberOfValue);

        Assert.Equal("Canon EOS 5D Mark II", RawImage.ReadChars(binaryReader, imageFileEntry));
    }

    // stripOffset 6)  0x0111 ULong 32-bit: 96332
    // orientation 7)  0x0112 UShort 16-bit: 1
    // stripByteCounts 8)  0x0117 ULong 32-bit: 2390306
    // xResolution 9)  0x011A Rational 2x32-bit: [0x0000011A] (2): 72/1 = 72
    // yResolution 10)  0x011B Rational 2x32-bit: [0x00000122] (2): 72/1 = 72
    // resolutionUnit 11)  0x0128 UShort 16-bit: 2, pixels per inch
    // dateTime 12)  0x0132 Ascii 8-bit: [0x0000012A] (20): 2013:07:13 01:10:00
    // 13)  0x013B Ascii 8-bit: [0x0000013E] (11): Greg Eakin

    [Fact]
    public void ExifTags()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var imageFileDirectory = rawImage.Directories.First();

        var imageFileEntry = imageFileDirectory[0x8769];
        Assert.Equal(4, imageFileEntry.TagType);
        Assert.Equal(0x000001B2u, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);

        binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
        var tags = new ImageFileDirectory(binaryReader);
        var directory = tags.DumpDirectory(binaryReader, ":0x8769");
        _testOutputHelper.WriteLine(directory);
    }
        
    [Fact]
    public void ShutterSpeedValue()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var imageFileDirectory = rawImage.Directories.First();

        var imageFileEntry = imageFileDirectory[0x8769];
        Assert.Equal(4, imageFileEntry.TagType);
        Assert.Equal(0x000001B2u, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);

        binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
        var exif = new ImageFileDirectory(binaryReader);
            
        var entry = exif.Entries.Single(e => e.TagId == 0x9201 && e.TagType == 0x0A);
        binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
        var s1 = binaryReader.ReadInt32();
        var s2 = binaryReader.ReadInt32();

        var ratio = s1/(double)s2;
        var value = Math.Abs(ratio) < 100.0 ? Math.Pow(2.0, -ratio) : 0.0;
        Assert.Equal(1.0/64.0, value);
            
        // write: Value > 0.0 ? -Math.Log(value)/Math.Log(2.0) : -100.0;
    }

    [Fact]
    public void Aperture()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var imageFileDirectory = rawImage.Directories.First();

        var imageFileEntry = imageFileDirectory[0x8769];
        Assert.Equal(4, imageFileEntry.TagType);
        Assert.Equal(0x000001B2u, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);

        binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
        var exif = new ImageFileDirectory(binaryReader);
            
        var entry = exif.Entries.Single(e => e.TagId == 0x9202 && e.TagType == 0x05);
        binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
        var us1 = binaryReader.ReadUInt32();
        var us2 = binaryReader.ReadUInt32();

        var ratio = us1/(double)us2;
        var value = Math.Pow(2.0, ratio / 2.0);
        Assert.Equal(2 * Math.Sqrt(2.0), value);
            
        // write: Value > 0.0 ? 2.0 * Math.Log(value) / Math.Log(2.0) : 0.0
    }

    [Fact]
    public void ExposureCompensation()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var imageFileDirectory = rawImage.Directories.First();

        var imageFileEntry = imageFileDirectory[0x8769];
        Assert.Equal(4, imageFileEntry.TagType);
        Assert.Equal(0x000001B2u, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);

        binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
        var exif = new ImageFileDirectory(binaryReader);
            
        var entry = exif.Entries.Single(e => e.TagId == 0x9204 && e.TagType == 0x0A);
        binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
        var s1 = binaryReader.ReadInt32();
        var s2 = binaryReader.ReadInt32();

        var value = s1/(double)s2;
        Assert.Equal(0.0, value);
    }
        
    // 15)  0x8298 Ascii 8-bit: [0x0000017E] (11): Greg Eakin

    // 6)  0x0111 ULong 32-bit: 91648u     -- Offset
    // 8)  0x0117 ULong 32-bit: 2702898u   -- Length
    [Fact]
    public void DumpImage0()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var imageFileDirectory = rawImage.Directories.First();

        var offset = imageFileDirectory.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
        // Assert.Equal(91648u, offset);

        var length = imageFileDirectory.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
        // Assert.Equal(2702898u, length);

        binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

        var dir = Path.GetDirectoryName(FileName) ?? ".";
        var name = Path.GetFileNameWithoutExtension(FileName) + "-0.jpg";
        var path = Path.Combine(dir, name);
        DumpImage(path, binaryReader, length);
    }

    private static void DumpImage(string filename, BinaryReader binaryReader, uint length)
    {
        using var stream = File.Create(filename);
        var bytes = (int)length;
        var buffer = new byte[32768];
        int read;
        while (bytes > 0 && (read = binaryReader.BaseStream.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
        {
            stream.Write(buffer, 0, read);
            bytes -= read;
        }
    }
}
