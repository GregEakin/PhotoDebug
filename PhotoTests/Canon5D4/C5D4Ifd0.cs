// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		C5D4Ifd1.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;
using System.Linq;
using PhotoLib.Tiff;
using Xunit;
using Xunit.Abstractions;

namespace PhotoTests.Canon5D4;

public class C5D4Ifd0
{
    const string FileName = @"P:\Canon 5D IV\Y_DReggie_03.CR2";
    private readonly ITestOutputHelper _testOutputHelper;

    public C5D4Ifd0(ITestOutputHelper testOutputHelper)
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
                0x0132, 0x013B, 0x02BC, 0x8298, 0x8769, 0x8825
            },
            imageFileDirectory.Entries.Select(e => e.TagId).ToArray());
    }

    // [Fact]
    // public void ReadImageGuid()
    // {
    //     using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
    //     using var binaryReader = new BinaryReader(fileStream);
    //     var rawImage = new RawImage(binaryReader);
    //
    //     var ifid0 = rawImage.Directories.First();
    //     var exif = ifid0[0x8769];       // Exif Offset
    //     binaryReader.BaseStream.Seek(exif.ValuePointer, SeekOrigin.Begin);
    //     var tags = new ImageFileDirectory(binaryReader);
    //
    //     var makerNotes = tags[0x927C];  // Maker Notes
    //     binaryReader.BaseStream.Seek(makerNotes.ValuePointer, SeekOrigin.Begin);
    //     var notes = new ImageFileDirectory(binaryReader);
    //
    //     var settings = notes.Entries.FirstOrDefault(e => e.TagId == 0x0028);
    //     if (settings == null) return;
    //
    //     binaryReader.BaseStream.Seek(settings.ValuePointer, SeekOrigin.Begin);
    //     var settingsData = new byte[settings.NumberOfValue];
    //     for (var i = 0; i < settings.NumberOfValue; i++)
    //         settingsData[i] = binaryReader.ReadByte();
    //
    //     var guid = new Guid(settingsData);
    //     _testOutputHelper.WriteLine($"Guid = {guid}");  // Guid = c261806b-f82c-9003-427a-06be63189acb
    // }

    [Fact]
    public void ImageWidth()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0100 UShort 16-bit: 5760
        var imageFileDirectory = rawImage.Directories.First();
        var imageFileEntry = imageFileDirectory[0x0100];
        Assert.Equal(ImageFileEntry.TagTypes.UShort, imageFileEntry.TagType);
        Assert.Equal(6720u, imageFileEntry.ValuePointer);
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
        Assert.Equal(ImageFileEntry.TagTypes.UShort, imageFileEntry.TagType);
        Assert.Equal(4480u, imageFileEntry.ValuePointer);
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
        Assert.Equal(ImageFileEntry.TagTypes.UShort, imageFileEntry.TagType);
        Assert.Equal(238u, imageFileEntry.ValuePointer);
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
        Assert.Equal(ImageFileEntry.TagTypes.UShort, imageFileEntry.TagType);
        Assert.Equal(6u, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);
    }

    [Fact]
    public void Maker()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x010F Ascii 8-bit: [0x000000F4] (6): Canon
        var imageFileDirectory = rawImage.Directories.First();
        var imageFileEntry = imageFileDirectory[0x010F];
        Assert.Equal(ImageFileEntry.TagTypes.Ascii, imageFileEntry.TagType);
        Assert.Equal(0x000000F4u, imageFileEntry.ValuePointer);
        Assert.Equal(6u, imageFileEntry.NumberOfValue);

        Assert.Equal("Canon", RawImage.ReadChars(binaryReader, imageFileEntry));
    }

    [Fact]
    public void Model()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0110 Ascii 8-bit: [0x000000FA] (22): Canon EOS 5D Mark III
        var imageFileDirectory = rawImage.Directories.First();
        var imageFileEntry = imageFileDirectory[0x0110];
        Assert.Equal(ImageFileEntry.TagTypes.Ascii, imageFileEntry.TagType);
        Assert.Equal(0x000000FAu, imageFileEntry.ValuePointer);
        Assert.Equal(21u, imageFileEntry.NumberOfValue);

        Assert.Equal("Canon EOS 5D Mark IV", RawImage.ReadChars(binaryReader, imageFileEntry));
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
        Assert.Equal(ImageFileEntry.TagTypes.ULong, imageFileEntry.TagType);
        Assert.Equal(0x000001BEu, imageFileEntry.ValuePointer);
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
        Assert.Equal(ImageFileEntry.TagTypes.ULong, imageFileEntry.TagType);
        Assert.Equal(0x000001BEu, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);

        binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
        var exif = new ImageFileDirectory(binaryReader);
            
        var entry = exif.Entries.Single(e => e.TagId == 0x9201 && e.TagType == ImageFileEntry.TagTypes.SRational);
        binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
        var s1 = binaryReader.ReadInt32();
        var s2 = binaryReader.ReadInt32();

        var ratio = s1/(double)s2;
        var value = Math.Abs(ratio) < 100.0 ? Math.Pow(2.0, -ratio) : 0.0;
        var x = 1.0 / value;
        Assert.Equal(1.0/197.40298565221642, value);
            
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
        Assert.Equal(ImageFileEntry.TagTypes.ULong, imageFileEntry.TagType);
        Assert.Equal(0x000001BEu, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);

        binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
        var exif = new ImageFileDirectory(binaryReader);
            
        var entry = exif.Entries.Single(e => e.TagId == 0x9202 && e.TagType == ImageFileEntry.TagTypes.URational);
        binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
        var us1 = binaryReader.ReadUInt32();
        var us2 = binaryReader.ReadUInt32();

        var ratio = us1/(double)us2;
        var value = Math.Pow(2.0, ratio / 2.0);
        Assert.Equal(4.0, value);
            
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
        Assert.Equal(ImageFileEntry.TagTypes.ULong, imageFileEntry.TagType);
        Assert.Equal(0x000001BEu, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);

        binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
        var exif = new ImageFileDirectory(binaryReader);
            
        var entry = exif.Entries.Single(e => e.TagId == 0x9204 && e.TagType == ImageFileEntry.TagTypes.SRational);
        binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
        var s1 = binaryReader.ReadInt32();
        var s2 = binaryReader.ReadInt32();

        var value = s1/(double)s2;
        Assert.Equal(1.0 / 3.0, value);
    }
        
    [Fact]
    public void XmpMetadata()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x02BC Byte 8-bit: [0x000119C4] (8192): // XML packet containing XMP metadata
        var imageFileDirectory = rawImage.Directories.First();
        var imageFileEntry = imageFileDirectory[0x02BC];
        Assert.Equal(ImageFileEntry.TagTypes.UByte, imageFileEntry.TagType);
        Assert.Equal(0x0000B608u, imageFileEntry.ValuePointer);
        Assert.Equal(8192u, imageFileEntry.NumberOfValue);

        var readChars = RawImage.ReadChars(binaryReader, imageFileEntry);

        const string expected1 =
            "<?xpacket begin='ï»¿' id='W5M0MpCehiHzreSzNTczkc9d'?><x:xmpmeta xmlns:x=\"adobe:ns:meta/\"><rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"><rdf:Description rdf:about=\"\" xmlns:xmp=\"http://ns.adobe.com/xap/1.0/\"><xmp:Rating>0</xmp:Rating></rdf:Description></rdf:RDF></x:xmpmeta>";
        Assert.Equal(expected1, readChars.Substring(0, 291));

        // lots of white space between these two substrings.
        Assert.True(string.IsNullOrWhiteSpace(readChars.Substring(291, 8173 - 291)));

        const string expected2 = "<?xpacket end='w'?>";
        Assert.Equal(expected2, readChars.Substring(8173));
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

        var offset = imageFileDirectory.Entries.Single(e => e.TagId == 0x0111 && e.TagType == ImageFileEntry.TagTypes.ULong).ValuePointer;
        // Assert.Equal(91648u, offset);

        var length = imageFileDirectory.Entries.Single(e => e.TagId == 0x0117 && e.TagType == ImageFileEntry.TagTypes.ULong).ValuePointer;
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
