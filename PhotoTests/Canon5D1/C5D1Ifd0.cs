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

namespace PhotoTests.Canon5D1;

public class C5D1Ifd0
{
    private const string FileName = @"\\Data\Photo\2022\2022-08-03\IMG_0256.CR2";
    private readonly ITestOutputHelper _testOutputHelper;

    public C5D1Ifd0(ITestOutputHelper testOutputHelper)
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
    // ImageWidth       IFD0:0x0100  0)  UShort 16-bit: 2496
    // ImageLength      IFD0:0x0101  1)  UShort 16-bit: 1664
    // BitsPerSample    IFD0:0x0102  2)  UShort 16-bit: [0x000000BE] (3): 8, 8, 8, 
    // Compression      IFD0:0x0103  3)  UShort 16-bit: 6
    // Make             IFD0:0x010F  4)  Ascii 8-bit, null terminated: [0x000000C4] (6): "Canon"
    // Model            IFD0:0x0110  5)  Ascii 8-bit, null terminated: [0x000000CA] (13): "Canon EOS 5D"
    // StripOffsets     IFD0:0x0111  6)  ULong 32-bit: 87604
    // Orientation      IFD0:0x0112  7)  UShort 16-bit: 1
    // StripByteCounts  IFD0:0x0117  8)  ULong 32-bit: 1651998
    // XResolution      IFD0:0x011A  9)  URational 2x32-bit: [0x000000EA] (1): 72/1 = 72
    // YResolution      IFD0:0x011B 10)  URational 2x32-bit: [0x000000F2] (1): 72/1 = 72
    // ResolutionUnit   IFD0:0x0128 11)  UShort 16-bit: 2
    // DateTime         IFD0:0x0132 12)  Ascii 8-bit, null terminated: [0x000000FA] (20): "2022:08:03 07:53:40"
    // Exif IFD Pointer IFD0:0x8769 13)  Image File Directory: [0x0000010E] (1): 

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
                0x0132, 0x8769 
            },
            imageFileDirectory.Entries.Select(e => e.TagId).ToArray());
    }

    [Fact]
    public void ImageWidth()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0100 UShort 16-bit: 2496u
        var imageFileDirectory = rawImage.Directories.First();
        var imageFileEntry = imageFileDirectory[0x0100];
        Assert.Equal(3, imageFileEntry.TagType);
        Assert.Equal(2496u, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);
    }

    [Fact]
    public void ImageLength()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0101 UShort 16-bit: 1664
        var imageFileDirectory = rawImage.Directories.First();
        var imageFileEntry = imageFileDirectory[0x0101];
        Assert.Equal(3, imageFileEntry.TagType);
        Assert.Equal(1664u, imageFileEntry.ValuePointer);
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
        Assert.Equal(190u, imageFileEntry.ValuePointer);
        Assert.Equal(3u, imageFileEntry.NumberOfValue);

        Assert.Equal(new[] {(ushort) 8, (ushort) 8, (ushort) 8},
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
        Assert.Equal(0x000000C4u, imageFileEntry.ValuePointer);
        Assert.Equal(6u, imageFileEntry.NumberOfValue);

        Assert.Equal("Canon", RawImage.ReadChars(binaryReader, imageFileEntry));
    }

    [Fact]
    public void Model()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0110 Ascii 8-bit: [0x000000CA] (22): Canon EOS 5D
        var imageFileDirectory = rawImage.Directories.First();
        var imageFileEntry = imageFileDirectory[0x0110];
        Assert.Equal(2, imageFileEntry.TagType);
        Assert.Equal(0x000000CAu, imageFileEntry.ValuePointer);
        Assert.Equal(13u, imageFileEntry.NumberOfValue);

        Assert.Equal("Canon EOS 5D", RawImage.ReadChars(binaryReader, imageFileEntry));
    }

    // stripOffset 6)  0x0111 ULong 32-bit: 96332
    // orientation 7)  0x0112 UShort 16-bit: 1
    // stripByteCounts 8)  0x0117 ULong 32-bit: 2390306
    // xResolution 9)  0x011A Rational 2x32-bit: [0x0000011A] (2): 72/1 = 72
    // yResolution 10)  0x011B Rational 2x32-bit: [0x00000122] (2): 72/1 = 72
    // resolutionUnit 11)  0x0128 UShort 16-bit: 2, pixels per inch
    // dateTime 12)  0x0132 Ascii 8-bit: [0x0000012A] (20): 2013:07:13 01:10:00
    // Exif IFD Pointer 13) 0x8769 Image File Directory: [0x0000010E] (1): 

    [Fact]
    public void ExifTags()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var imageFileDirectory = rawImage.Directories.First();

        var imageFileEntry = imageFileDirectory[0x8769];
        Assert.Equal(4, imageFileEntry.TagType);
        Assert.Equal(0x0000010Eu, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);

        binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
        var tags = new ImageFileDirectory(binaryReader);
        var directory = tags.DumpDirectory(binaryReader, ":0x8769");
        _testOutputHelper.WriteLine(directory);
    }
        
    // :0x8769:0x829A  0)  URational 2x32-bit: [0x00000264] (1): 1/250 = 0.004
    // :0x8769:0x829D  1)  URational 2x32-bit: [0x0000026C] (1): 8/1 = 8
    // :0x8769:0x8822  2)  UShort 16-bit: 2
    // :0x8769:0x8827  3)  UShort 16-bit: 100
    // :0x8769:0x9000  4)  UByte[]: 48, 50, 50, 49
    // :0x8769:0x9003  5)  Ascii 8-bit, null terminated: [0x00000274] (20): "2022:08:03 07:53:40"
    // :0x8769:0x9004  6)  Ascii 8-bit, null terminated: [0x00000288] (20): "2022:08:03 07:53:40"
    // :0x8769:0x9101  7)  UByte[]: 1, 2, 3, 0
    // :0x8769:0x9201  8)  SRational 2x32-bit: [0x0000029C] (1): 524288/65536 = 8
    // :0x8769:0x9202  9)  URational 2x32-bit: [0x000002A4] (1): 393216/65536 = 6
    // :0x8769:0x9204 10)  SRational 2x32-bit: [0x000002AC] (1): 0/1 = 0
    // :0x8769:0x9207 11)  UShort 16-bit: 5
    // :0x8769:0x9209 12)  UShort 16-bit: 16
    // :0x8769:0x920A 13)  URational 2x32-bit: [0x000002B4] (1): 40/1 = 40
    // :0x8769:0x927C 14)  Maker note: [0x000002BC] (75080): 
    // :0x8769:0x927C:0x0001  0)  UShort 16-bit: [0x0000041E] (46): 92, 2, 0, 4, 0, 0, 0, 0, 0, 6, 0, 1, 0, 0, 0, 32767, 32767, 3, 2, 0, 1, 65535, 4144, 40, 40, 1, 96, 288, 0, 0, 0, 0, 65535, 65535, 65535, 0, 0, 0, 0, 65535, 65535, 0, 0, 32767, 65535, 65535, 
    // :0x8769:0x927C:0x0002  1)  UShort 16-bit: [0x0000047A] (4): 1, 40, 1481, 978, 
    // :0x8769:0x927C:0x0003  2)  UShort 16-bit: [0x00000482] (4): 0, 0, 0, 0, 
    // :0x8769:0x927C:0x0004  3)  UShort 16-bit: [0x0000048A] (34): 68, 0, 160, 284, 192, 256, 0, 0, 3, 0, 8, 8, 149, 0, 0, 0, 0, 0, 1, 0, 0, 188, 256, 159, 0, 0, 248, 65535, 65535, 65535, 65535, 0, 0, 0, 
    // :0x8769:0x927C:0x0006  4)  Ascii 8-bit, null terminated: [0x000004CE] (13): "Canon EOS 5D"
    // :0x8769:0x927C:0x0007  5)  Ascii 8-bit, null terminated: [0x000004EE] (24): "Firmware Version 1.1.1"
    // :0x8769:0x927C:0x0009  6)  Ascii 8-bit, null terminated: [0x00000506] (32): "Gregory Eakin"
    // :0x8769:0x927C:0x000C  7)  ULong 32-bit: 3821300853
    // :0x8769:0x927C:0x000D  8)  UByte[]: [0x00000526] (1024): 
    // :0x8769:0x927C:0x000F  9)  UShort 16-bit: [0x00000976] (23): 46, 0, 256, 512, 768, 1024, 1280, 1536, 1792, 2048, 2304, 2560, 2816, 3072, 3328, 3584, 3840, 4096, 4352, 4608, 4864, 5120, 5376, 
    // :0x8769:0x927C:0x0010 10)  ULong 32-bit: 2147484179
    // :0x8769:0x927C:0x0012 11)  UShort 16-bit: [0x00000926] (40): 15, 15, 4368, 2912, 4992, 3328, 119, 119, 64493, 65037, 0, 499, 1043, 499, 0, 65037, 0, 65337, 0, 199, 199, 0, 65337, 0, 249, 445, 249, 0, 65287, 65091, 65287, 0, 249, 249, 249, 65287, 65287, 65287, 256, 65535, 
    // :0x8769:0x927C:0x0013 12)  UShort 16-bit: [0x000009A4] (4): 0, 159, 7, 112, 
    // :0x8769:0x927C:0x0015 13)  ULong 32-bit: 2684354560
    // :0x8769:0x927C:0x0019 14)  UShort 16-bit: 1
    // :0x8769:0x927C:0x0083 15)  ULong 32-bit: 0
    // :0x8769:0x927C:0x0093 16)  UShort 16-bit: [0x000009AC] (16): 32, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 65535, 65535, 
    // :0x8769:0x927C:0x0095 17)  Ascii 8-bit, null terminated: [0x000009CC] (64): ""
    // :0x8769:0x927C:0x0096 18)  Ascii 8-bit, null terminated: [0x00000A0C] (16): "\x11???\x11???"
    // :0x8769:0x927C:0x00A0 19)  UShort 16-bit: [0x00000A1C] (14): 28, 0, 4, 0, 0, 0, 0, 0, 65535, 5200, 131, 0, 0, 0, 
    // :0x8769:0x927C:0x00AA 20)  UShort 16-bit: [0x00000A38] (5): 10, 349, 1024, 1024, 1076, 
    // :0x8769:0x927C:0x00B4 21)  UShort 16-bit: 1
    // :0x8769:0x927C:0x00E0 22)  UShort 16-bit: [0x00000A42] (17): 34, 4476, 2954, 1, 1, 100, 39, 4467, 2950, 0, 0, 0, 0, 0, 0, 0, 0, 
    // :0x8769:0x927C:0x00D0 23)  ULong 32-bit: 0
    // :0x8769:0x927C:0x4001 24)  UShort 16-bit: [0x00000A64] (796): 
    // :0x8769:0x927C:0x4002 25)  UShort 16-bit: [0x0000109C] (11110): 
    // :0x8769:0x927C:0x4005 26)  UByte[]: [0x00006768] (49288): 
    // :0x8769:0x927C:0x4008 27)  UShort 16-bit: [0x000127F0] (3): 129, 129, 129, 
    // :0x8769:0x927C:0x4009 28)  UShort 16-bit: [0x000127F6] (3): 129, 129, 129, 
    // :0x8769:0x9286 15)  UByte[]: [0x00012804] (264): 
    // :0x8769:0xA000 16)  UByte[]: 48, 49, 48, 48
    // :0x8769:0xA001 17)  UShort 16-bit: 1
    // :0x8769:0xA002 18)  UShort 16-bit: 4368
    // :0x8769:0xA003 19)  UShort 16-bit: 2912
    // :0x8769:0xA005 20)  Interoperability IFD: [0x0001290C] (1): 
    // :0x8769:0xA005:0x0001  0)  Ascii 8-bit, null terminated: [0x00383952] (4): "R98"
    // :0x8769:0xA005:0x0002  1)  UByte[]: 48, 49, 48, 48
    // :0x8769:0xA20E 21)  URational 2x32-bit: [0x0001292A] (1): 4368000/1415 = 3086.9257950530036
    // :0x8769:0xA20F 22)  URational 2x32-bit: [0x00012932] (1): 2912000/942 = 3091.295116772824
    // :0x8769:0xA210 23)  UShort 16-bit: 2
    // :0x8769:0xA401 24)  UShort 16-bit: 0
    // :0x8769:0xA402 25)  UShort 16-bit: 0
    // :0x8769:0xA403 26)  UShort 16-bit: 0
    // :0x8769:0xA406 27)  UShort 16-bit: 0

    [Fact]
    public void ShutterSpeedValue()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var imageFileDirectory = rawImage.Directories.First();

        var imageFileEntry = imageFileDirectory[0x8769];
        Assert.Equal(4, imageFileEntry.TagType);
        Assert.Equal(0x0000010Eu, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);

        binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
        var exif = new ImageFileDirectory(binaryReader);
            
        var entry = exif.Entries.Single(e => e.TagId == 0x9201 && e.TagType == 0x0A);
        binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
        var s1 = binaryReader.ReadInt32();
        var s2 = binaryReader.ReadInt32();

        var ratio = s1/(double)s2;
        var value = Math.Abs(ratio) < 100.0 ? Math.Pow(2.0, -ratio) : 0.0;
        Assert.Equal(1.0/256.0, value);
            
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
        Assert.Equal(0x0000010Eu, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);

        binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
        var exif = new ImageFileDirectory(binaryReader);
            
        var entry = exif.Entries.Single(e => e.TagId == 0x9202 && e.TagType == 0x05);
        binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
        var us1 = binaryReader.ReadUInt32();
        var us2 = binaryReader.ReadUInt32();

        var ratio = us1/(double)us2;
        var value = Math.Pow(2.0, ratio / 2.0);
        Assert.Equal(8.0, value);
            
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
        Assert.Equal(0x0000010Eu, imageFileEntry.ValuePointer);
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
        var length = imageFileDirectory.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
        binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

        var dir = Path.GetDirectoryName(FileName) ?? ".";
        var name = Path.GetFileNameWithoutExtension(FileName) + "-0.jpg";
        var path = Path.Combine(dir, name);
        DumpImage(path, binaryReader, length);
    }

    private static void DumpImage(string filename, BinaryReader binaryReader, uint length)
    {
        using var stream = File.Create(filename);
        var bytes = (int) length;
        var buffer = new byte[32768];
        int read;
        while (bytes > 0 && (read = binaryReader.BaseStream.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
        {
            stream.Write(buffer, 0, read);
            bytes -= read;
        }
    }
}
