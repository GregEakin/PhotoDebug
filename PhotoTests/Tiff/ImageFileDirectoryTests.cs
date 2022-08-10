// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		ImageFileDirectoryTests.cs
// AUTHOR:		Greg Eakin

using System.IO;
using System.Linq;
using PhotoLib.Tiff;
using Xunit;

namespace PhotoTests.Tiff;

public class ImageFileDirectoryTests
{
    private static readonly byte[] Data = { 0x01, 0x00, 0x12, 0x00, 0x00, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x40, 0x14, 0x00, 0x00, 0x00, 0x00 };

    [Fact]
    public void EntriesCount()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var imageFileDirectory = new ImageFileDirectory(reader);
        Assert.Single(imageFileDirectory.Entries);
    }

    [Fact]
    public void Entry()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var imageFileDirectory = new ImageFileDirectory(reader);
        var entry = imageFileDirectory.Entries.First();
        Assert.Equal(0x0012, entry.TagId);
    }

    [Fact]
    public void NextEntry()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var imageFileDirectory = new ImageFileDirectory(reader);
        Assert.Equal(0x00000000u, imageFileDirectory.NextEntry);
    }
}
