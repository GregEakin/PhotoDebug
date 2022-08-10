// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		C7D2Ifd1.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;
using System.Linq;
using PhotoLib.Tiff;
using Xunit;
using Xunit.Abstractions;

namespace PhotoTests.Canon7D2;

public class C7D2Ifd0
{
    // private const string FileName = @"\\Data\Photo\Source\7D2high.CR2";
    private const string FileName = @"\\Data\Photo\2018\2018-10-19\B05A1194.CR2";
    private readonly ITestOutputHelper _testOutputHelper;

    public C7D2Ifd0(ITestOutputHelper testOutputHelper)
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

    [Fact]
    public void ReadImageGuid()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        var ifid0 = rawImage.Directories.First();
        var exif = ifid0[0x8769];       // Exif Offset
        binaryReader.BaseStream.Seek(exif.ValuePointer, SeekOrigin.Begin);
        var tags = new ImageFileDirectory(binaryReader);

        var makerNotes = tags[0x927C];  // Maker Notes
        binaryReader.BaseStream.Seek(makerNotes.ValuePointer, SeekOrigin.Begin);
        var notes = new ImageFileDirectory(binaryReader);

        var settings = notes.Entries.FirstOrDefault(e => e.TagId == 0x0028);
        if (settings == null) return;

        binaryReader.BaseStream.Seek(settings.ValuePointer, SeekOrigin.Begin);
        var settingsData = new byte[settings.NumberOfValue];
        for (var i = 0; i < settings.NumberOfValue; i++)
            settingsData[i] = binaryReader.ReadByte();

        var guid = new Guid(settingsData);
        Console.WriteLine("Guid = {0}", guid);  // Guid = c261806b-f82c-9003-427a-06be63189acb
    }

    [Fact]
    public void ImageWidth()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0100 UShort 16-bit: 5184
        var imageFileDirectory = rawImage[0x00000010];
        var imageFileEntry = imageFileDirectory[0x0100];
        Assert.Equal(3, imageFileEntry.TagType);
        Assert.Equal(5472u, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);
    }

    [Fact]
    public void ImageLength()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0101 UShort 16-bit: 3456
        var imageFileDirectory = rawImage[0x00000010];
        var imageFileEntry = imageFileDirectory[0x0101];
        Assert.Equal(3, imageFileEntry.TagType);
        Assert.Equal(3648u, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);
    }

    [Fact]
    public void Maker()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x010F Ascii 8-bit: [0x000000F4] (6): Canon
        var imageFileDirectory = rawImage[0x00000010];
        var imageFileEntry = imageFileDirectory[0x010F];
        Assert.Equal(2, imageFileEntry.TagType);
        Assert.Equal(0x000000F4u, imageFileEntry.ValuePointer);
        Assert.Equal(6u, imageFileEntry.NumberOfValue);

        var readChars = RawImage.ReadChars(binaryReader, imageFileEntry);
        Assert.Equal("Canon", readChars);
    }

    [Fact]
    public void Model()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // 0x0110 Ascii 8-bit: [0x000000FA] (13): Canon EOS 7D
        var imageFileDirectory = rawImage[0x00000010];
        var imageFileEntry = imageFileDirectory[0x0110];
        Assert.Equal(2, imageFileEntry.TagType);
        Assert.Equal(0x000000FAu, imageFileEntry.ValuePointer);
        Assert.Equal(21u, imageFileEntry.NumberOfValue);

        Assert.Equal("Canon EOS 7D Mark II", RawImage.ReadChars(binaryReader, imageFileEntry));
    }

    [Fact]
    public void ExifTags()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var imageFileDirectory = rawImage.Directories.First();

        var imageFileEntry = imageFileDirectory[0x8769];
        Assert.Equal(4, imageFileEntry.TagType);
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
        Assert.Equal(4, imageFileEntry.TagType);
        Assert.Equal(0x000001BEu, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);

        binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
        var exif = new ImageFileDirectory(binaryReader);
            
        var entry = exif.Entries.Single(e => e.TagId == 0x9201 && e.TagType == 0x0A);
        binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
        var s1 = binaryReader.ReadInt32();
        var s2 = binaryReader.ReadInt32();

        var ratio = s1/(double)s2;
        var value = Math.Abs(ratio) < 100.0 ? Math.Pow(2.0, -ratio) : 0.0;
        Assert.Equal(1.0/12.337686603263526759513918861431, value);
            
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
        Assert.Equal(0x000001BEu, imageFileEntry.ValuePointer);
        Assert.Equal(1u, imageFileEntry.NumberOfValue);

        binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
        var exif = new ImageFileDirectory(binaryReader);
            
        var entry = exif.Entries.Single(e => e.TagId == 0x9202 && e.TagType == 0x05);
        binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
        var us1 = binaryReader.ReadUInt32();
        var us2 = binaryReader.ReadUInt32();

        var ratio = us1/(double)us2;
        var value = Math.Pow(2.0, ratio / 2.0);
        Assert.Equal(2.0 * Math.Sqrt(2.0), value);
            
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
        Assert.Equal(0x000001BEu, imageFileEntry.ValuePointer);
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
