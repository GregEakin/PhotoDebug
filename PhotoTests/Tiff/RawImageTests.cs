// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		RawImageTests.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;
using System.Linq;
using PhotoLib.Tiff;
using Xunit;

namespace PhotoTests.Tiff;

public class RawImageTests
{
    [Fact]
    public void Header()
    {
        var data = new byte[]
        {
            0x49, 0x49, 0x2A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x43, 0x52, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00
        };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var rawImage = new RawImage(reader);
        var cr2Header = rawImage.Header;
        Assert.Equal(0x5243, cr2Header.CR2Magic);
    }

    [Fact]
    public void Directory()
    {
        var data = new byte[]
        {
            0x49, 0x49, 0x2A, 0x00, 0x10, 0x00, 0x00, 0x00, 0x43, 0x52, 0x02, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00
        };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        var rawImage = new RawImage(reader);
        var directory = rawImage.Directories.First();
        Assert.Empty(directory.Entries); 
        Assert.Equal(0x00000000u, directory.NextEntry);
    }

    [Fact]
    public void DirectoryDuplicate()
    {
        var data = new byte[]
        {
            0x49, 0x49, 0x2A, 0x00, 0x10, 0x00, 0x00, 0x00, 0x43, 0x52, 0x02, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x10, 0x00, 0x00, 0x00
        };
        using var memory = new MemoryStream(data);
        using var reader = new BinaryReader(memory);
        Assert.Throws<ArgumentException>(() => new RawImage(reader));
    }
}
