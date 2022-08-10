// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		ImageFileEntryTests.cs
// AUTHOR:		Greg Eakin

using System.IO;
using PhotoLib.Tiff;
using Xunit;

namespace PhotoTests.Tiff;

public class ImageFileEntryTests
{
    private static readonly byte[] Data = { 0x12, 0x00, 0x0A, 0x00, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x40, 0x14 };

    [Fact]
    public void NumberOfValue()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var imageFileEntry = new ImageFileEntry(reader);
        Assert.Equal(0x00010003u, imageFileEntry.NumberOfValue);
    }

    [Fact]
    public void TagId()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var imageFileEntry = new ImageFileEntry(reader);
        Assert.Equal(0x0012, imageFileEntry.TagId);
    }

    [Fact]
    public void TagType()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var imageFileEntry = new ImageFileEntry(reader);
        Assert.Equal(ImageFileEntry.TagTypes.SRational, imageFileEntry.TagType);
    }

    [Fact]
    public void ValuePointer()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var imageFileEntry = new ImageFileEntry(reader);
        Assert.Equal(0x14400000u, imageFileEntry.ValuePointer);
    }
}
