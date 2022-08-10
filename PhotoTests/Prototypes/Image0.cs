// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		Image0.cs
// AUTHOR:		Greg Eakin

using System.IO;
using System.Linq;
using PhotoLib.Tiff;
using Xunit;

namespace PhotoTests.Prototypes;

public class Image0
{
    [Fact]
    public void DumpImage0Test()
    {
        const string Folder = @"\\Data\Photo\2016\2016-02-21 Studio\";
        DumpImage0(Folder, "Studio 015.CR2");
    }

    private static void DumpImage0(string folder, string file)
    {
        var fileName2 = folder + file;
        using var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // Images #0 and #1 are compressed in lossy (classic) JPEG
        var image = rawImage.Directories.First();

        var stripOffset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == ImageFileEntry.TagTypes.ULong).ValuePointer;
        // Assert.Equal(99812u, stripOffset);

        var orientation = image.Entries.Single(e => e.TagId == 0x0112 && e.TagType == ImageFileEntry.TagTypes.UShort).ValuePointer;
        // Assert.Equal(1u, orientation);    // 1 = 0,0 is top left

        var stripByteCounts = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == ImageFileEntry.TagTypes.ULong).ValuePointer;
        // Assert.Equal(2823352u, stripByteCounts);

        DumpImage(binaryReader, folder + "0L2A8897-0.JPG", stripOffset, stripByteCounts);
    }

    private static void DumpImage(BinaryReader binaryReader, string filename, uint offset, uint length)
    {
        using var fout = File.Open(filename, FileMode.Create, FileAccess.Write);
        binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

        var buffer = new byte[32];
        for (var i = 0; i < length;)
        {
            var n = binaryReader.Read(buffer, 0, buffer.Length);
            Assert.True(n > 0);
            fout.Write(buffer, 0, n);
            i += n;
        }

        fout.Flush();
    }
}
