// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		StartOfFrameTests.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;
using PhotoLib.Jpeg;
using Xunit;

namespace PhotoTests.Jpeg.JpegTags;

public class StartOfFrameTests
{
    private static readonly byte[] Data =
    {
        0xFF, 0xC3, 0x00, 0x14, 0x0E, 0x0D, 0xBC, 0x05,
        0x3C, 0x04, 0x01, 0x11, 0x00, 0x02, 0x11, 0x00,
        0x03, 0x11, 0x00, 0x04, 0x11, 0x00
    };

    [Fact]
    public void BadMark()
    {
        var badData = new byte[] { 0x00, 0x00 };
        using var memory = new MemoryStream(badData);
        using var reader = new BinaryReader(memory);
        Assert.Throws<ArgumentException>(() => new StartOfFrame(reader));
    }

    [Fact]
    public void Mark()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var lossless = new StartOfFrame(reader);
        Assert.Equal(0xFF, lossless.Mark);
    }

    [Fact]
    public void BadTag()
    {
        var badData = new byte[] { 0xFF, 0x00 };
        using var memory = new MemoryStream(badData);
        using var reader = new BinaryReader(memory);
        Assert.Throws<ArgumentException>(() => new StartOfFrame(reader));
    }

    [Fact]
    public void Tag()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var lossless = new StartOfFrame(reader);
        Assert.Equal(0xC3, lossless.Tag);
    }

    [Fact]
    public void Length()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var lossless = new StartOfFrame(reader);
        Assert.Equal(0x0014, lossless.Length);
    }

    [Fact]
    public void Precision()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var lossless = new StartOfFrame(reader);
        Assert.Equal(0x0E, lossless.Precision);
    }

    [Fact]
    public void ScanLines()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var lossless = new StartOfFrame(reader);
        Assert.Equal(0x0DBC, lossless.ScanLines);
    }

    [Fact]
    public void SamplesPerLine()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var lossless = new StartOfFrame(reader);
        Assert.Equal(0x053C, lossless.SamplesPerLine);
    }

    [Fact]
    public void ComponentCount()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var lossless = new StartOfFrame(reader);
        Assert.Equal(0x04, lossless.Components.Length);
    }

    [Fact]
    public void ShortLength()
    {
        var badData = new byte[]
        {
            0xFF, 0xC3, 0x00, 0x07, 0x0E, 0x0D, 0xBC, 0x05, 0x3C
        };

        using var memory = new MemoryStream(badData);
        using var reader = new BinaryReader(memory);
        Assert.Throws<EndOfStreamException>(() => new StartOfFrame(reader));
    }

    [Fact]
    public void LongLength()
    {
        var badData = new byte[]
        {
            0xFF, 0xC3, 0x00, 0x09, 0x0E, 0x0D, 0xBC, 0x05, 0x3C, 0x00
        };

        using var memory = new MemoryStream(badData);
        using var reader = new BinaryReader(memory);
        Assert.Throws<ArgumentException>(() => new StartOfFrame(reader));
    }

    [Fact]
    public void ShortComponentCount()
    {
        var badData = new byte[]
        {
            0xFF, 0xC3, 0x00, 0x0C, 0x0E, 0x0D, 0xBC, 0x05, 0x3C, 0x01, 0x01, 0x11, 0x00, 0x02
        };

        using var memory = new MemoryStream(badData);
        using var reader = new BinaryReader(memory);
        Assert.Throws<ArgumentException>(() => new StartOfFrame(reader));
    }

    [Fact]
    public void LongComponentCount()
    {
        var badData = new byte[]
        {
            0xFF, 0xC3, 0x00, 0x0A, 0x0E, 0x0D, 0xBC, 0x05, 0x3C, 0x01, 0x01, 0x11
        };

        using var memory = new MemoryStream(badData);
        using var reader = new BinaryReader(memory);
        Assert.Throws<EndOfStreamException>(() => new StartOfFrame(reader));
    }

    [Fact]
    public void ComponentId()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var lossless = new StartOfFrame(reader);
        Assert.Equal(0x01, lossless.Components[0].ComponentId);
    }

    [Fact]
    public void TableId()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var lossless = new StartOfFrame(reader);
        Assert.Equal(0x00, lossless.Components[0].TableId);
    }

    [Fact]
    public void HFactor()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var lossless = new StartOfFrame(reader);
        Assert.Equal(0x01, lossless.Components[0].HFactor);
    }

    [Fact]
    public void VFactor()
    {
        using var memory = new MemoryStream(Data);
        using var reader = new BinaryReader(memory);
        var lossless = new StartOfFrame(reader);
        Assert.Equal(0x01, lossless.Components[0].VFactor);
    }
}
