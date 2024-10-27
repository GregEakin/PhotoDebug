// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		OtherUnitTests.cs
// AUTHOR:		Greg Eakin

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using PhotoLib.Jpeg;
using PhotoLib.Jpeg.JpegTags;
using PhotoLib.RecipeData;
using PhotoLib.Tiff;
using Xunit;

namespace PhotoTests;

public class OtherUnitTests
{
    // [Fact]
    public void DumpHuffmanTable()
    {
        const string directory = @"..\..\..\Samples\";
        const string fileName2 = directory + "huff_simple0.jpg";

        using var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        binaryReader.BaseStream.Seek(0x000000D0u, SeekOrigin.Begin);
        var huffmanTable = new DefineHuffmanTable(binaryReader);
        Assert.Equal(0xFF, huffmanTable.Mark);
        Assert.Equal(0xC4, huffmanTable.Tag);
        Console.WriteLine(huffmanTable);
    }

    // [Fact]
    public void TestMethod8()
    {
        const string directory = @"..\..\..\Samples\";
        const string fileName2 = directory + "IMAG0086.jpg";

        using var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var startOfImage = new StartOfImage(binaryReader, 0x00u, (uint)fileStream.Length);
        Assert.Equal(0xFF, startOfImage.Mark);
        Assert.Equal(0xD8, startOfImage.Tag); // JPG_MARK_SOI

        var huffmanTable = startOfImage.HuffmanTable;
        Assert.Equal(0xFF, huffmanTable.Mark);
        Assert.Equal(0xC4, huffmanTable.Tag);
        Console.WriteLine(huffmanTable);
    }

    public void TestMethodA()
    {
        // it is the width of a
        // row.	The initial values at the beginning of each row is the RG/GB value of
        // its nearest previous row beginning.  For the first row, the initial row
        // values are 1/2 the bit range defined by the precision.  Thus for 12-bit
        // precision:
        //     Pix[Row, Col] = Val
        //     Pix[0,0] = (1 << (Precision - 1)) + Diff
        //     Pix[0,1] = (1 << (Precision - 1)) + Diff
        // and for n >= 1
        //     Pix[n,0] = Pix[n-2,0] + Diff
        //     Pix[n,1] = Pix[n-2,1] + Diff
        // while for any other Row/Column
        //     Pix[R,C] = Pix[R,C-2] + Diff
    }

    // [Fact]
    public void TestMethodB()
    {
        // const string Directory = @"\\Data\Photo\Source\";
        // const string FileName2 = Directory + "IMG_0503.CR2";
        const string directory = @"\\Data\Photo\2013\2013-10-06 001\";
        const string fileName2 = directory + "0L2A8892.CR2";

        using var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var imageFileDirectory = rawImage.Directories.Last();

        var strips = imageFileDirectory.Entries.Single(e => e.TagId == 0xC640 && e.TagType == ImageFileEntry.TagTypes.UShort).ValuePointer; // TIF_CR2_SLICE
        binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
        var x = binaryReader.ReadUInt16();
        var y = binaryReader.ReadUInt16();
        var z = binaryReader.ReadUInt16();

        var address = imageFileDirectory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
        var length = imageFileDirectory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
        binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
        var startOfImage = new StartOfImage(binaryReader, address, length);
        var lossless = startOfImage.StartOfFrame;
        // Assert.Equal(4711440, lossless.SamplesPerLine * lossless.ScanLines); // IbSize (IB = new ushort[IbSize])

        var rawSize = address + length - binaryReader.BaseStream.Position - 2;
        // Assert.Equal(23852856, rawSize); // RawSize (Raw = new byte[RawSize]
        startOfImage.ImageData = new ImageData(binaryReader, (uint)rawSize);
        // var table0 = startOfImage.HuffmanTable.Tables[0x00];

        var buffer = new byte[rawSize];

        var rp = 0;
        for (var jrow = 0; jrow < lossless.ScanLines; jrow++)
        {
            for (var jcol = 0; jcol < lossless.SamplesPerLine; jcol++)
            {
                var val = startOfImage.ImageData.RawData[rp++];
                if (val == 0xFF)
                {
                    var code = startOfImage.ImageData.RawData[rp];
                    Assert.True(0 == code, $"Invalid code found {rp}, {startOfImage.ImageData.RawData[rp]}");

                    rp++;
                }

                var jidx = jrow * lossless.SamplesPerLine + jcol;
                var i = jidx / (y * lossless.ScanLines);
                var j = i >= x;
                if (j)
                    i = x;
                jidx -= i * (y * lossless.ScanLines);
                var row = jidx / (j ? y : z);
                var col = jidx % (j ? y : z) + i * y;

                buffer[row * lossless.SamplesPerLine + col] = val;
            }
        }
    }

    // [Fact]
    public void TestMethodB6()
    {
        // const string Folder = @"\\Data\Photo\Source\";
        // const string FileName2 = Folder + "IMG_0503.CR2";

        const string folder = @"\\Data\Photo\2013\2013-10-06 001\";
        const string fileName2 = folder + "0L2A8892.CR2";
        const string bitmap = folder + "0L2A8892 B6.BMP";

        //const string Folder = @"\\Data\Photo\2013\2013_06_02\";
        //const string FileName2 = Folder + "IMG_3559.CR2";
        //const string Bitmap = Folder + "IMG_3559.BMP";

        ProcessFile(fileName2, bitmap);
    }

    private static void ProcessFile(string fileName, string bitmap)
    {
        using var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var imageFileDirectory = rawImage.Directories.Last();

        var strips = imageFileDirectory.Entries.Single(e => e.TagId == 0xC640 && e.TagType == ImageFileEntry.TagTypes.UShort).ValuePointer; // TIF_CR2_SLICE
        binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
        var x = binaryReader.ReadUInt16();
        var y = binaryReader.ReadUInt16();
        var z = binaryReader.ReadUInt16();

        var address = imageFileDirectory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
        var length = imageFileDirectory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
        binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
        var startOfImage = new StartOfImage(binaryReader, address, length);
        var lossless = startOfImage.StartOfFrame;

        var rawSize = address + length - binaryReader.BaseStream.Position - 2;
        // Assert.Equal(23852858, rawSize); // RawSize (Raw = new byte[RawSize]
        startOfImage.ImageData = new ImageData(binaryReader, (uint)rawSize);

        var colors = lossless.Components.Sum(comp => comp.HFactor * comp.VFactor);
        var table0 = startOfImage.HuffmanTable.Tables[0x00];

        // // var buffer = new byte[rawSize];
        // using (var image1 = new Bitmap(500, 500))
        // {
        //     for (var jrow = 0; jrow < lossless.ScanLines; jrow++)
        //     {
        //         var rowBuf = new ushort[lossless.SamplesPerLine * colors];
        //         for (var jcol = 0; jcol < lossless.SamplesPerLine; jcol++)
        //         {
        //             for (var jcolor = 0; jcolor < colors; jcolor++)
        //             {
        //                 //var pred = (ushort)0;
        //                 //var len = gethuff();
        //                 //var diff = getbits(len);
        //                 //var row = pred + diff;
        //
        //                 var val = GetValue(startOfImage.ImageData, table0);
        //                 var bits = startOfImage.ImageData.GetSetOfBits(val);
        //                 rowBuf[jcol * colors + jcolor] = bits;
        //             }
        //
        //             DumpPixel(jcol, jrow, rowBuf, colors, image1);
        //         }
        //         // var pp = startOfImage.ImageData.Index;
        //     }
        //
        //     image1.Save(bitmap);
        // }

        // Assert.Equal(23852855, startOfImage.ImageData.Index);
        //Console.WriteLine("{0}: ", startOfImage.ImageData.BitsLeft);
        //for (var i = startOfImage.ImageData.Index; i < rawSize - 2; i++)
        //{
        //    Console.WriteLine("{0} ", startOfImage.ImageData.RawData[i].ToString("X2"));
        //}
    }

    private static void DumpPixelDebug(int row, IList<short> rowBuf0, IList<short> rowBuf1)
    {
        const int x = 122; // 2116;
        const int y = 40; // 1416 / 2;

        var q = row - y;
        if (q < 0 || q >= 5)
        {
            return;
        }

        for (var p = 0; p < 5; p++)
        {
            var red = rowBuf0[2 * p + x + 0] - 2047;
            var green = rowBuf0[2 * p + x + 1] - 2047;
            var blue = rowBuf1[2 * p + x + 1] - 2047;
            var green2 = rowBuf1[2 * p + x + 0] - 2047;

            Console.WriteLine("{4}, {5}: {0}, {1}, {2}, {3}", red, green, blue, green2, p + 1, q + 1);
        }
    }

    // private static void DumpPixel(int jcol, int jrow, ushort[] rowBuf, int colors, Bitmap image1)
    // {
    //     var p = jcol - 50;
    //     var q = jrow - 30;
    //     if (p < 0 || p >= 500 || q < 0 || q >= 500)
    //         return;
    //
    //     var bits1 = rowBuf[jcol * colors + 0] >> 2;
    //     if (bits1 > 0xFF)
    //         bits1 = 0xFF;
    //
    //     var bits2 = rowBuf[jcol * colors + 1] >> 2;
    //     if (bits2 > 0xFF)
    //         bits2 = 0xFF;
    //
    //     if (jrow % 2 == 0)
    //     {
    //         var red = bits1;
    //         var green = bits2 >> 1;
    //         var color = Color.FromArgb(red, green, 0);
    //         image1.SetPixel(p, q, color);
    //     }
    //     else
    //     {
    //         var green = bits1 >> 1;
    //         var blue = bits2;
    //         var color = Color.FromArgb(0, green, blue);
    //         image1.SetPixel(p, q, color);
    //     }
    // }

    [Fact]
    public void SerialNums()
    {
        const uint cam1 = 3071201378;
        const uint cam1H = cam1 & 0xFFFF0000 >> 8;
        const uint cam1L = cam1 & 0x0000FFFF;
        Assert.Equal("ED00053346", $"{cam1H:X4}{cam1L:D5}");
    }

    [Fact]
    public void SearchWhiteBalance()
    {
        const string fileName = @"\\Data\Photo\Source\5DIIIhigh.CR2";

        using var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        var ifid0 = rawImage.Directories.First();
        var make = ifid0[0x010f];   // Make
        Assert.Equal("Canon", RawImage.ReadChars(binaryReader, make));

        var model = ifid0[0x0110];  // Model
        // Assert.Equal("Canon EOS 7D", RawImage.ReadChars(binaryReader, model));
        Assert.Equal("Canon EOS 5D Mark III", RawImage.ReadChars(binaryReader, model));

        var exif = ifid0[0x8769];   // Exif Offset
        binaryReader.BaseStream.Seek(exif.ValuePointer, SeekOrigin.Begin);
        var tags = new ImageFileDirectory(binaryReader);
        // tags.DumpDirectory(binaryReader);

        var makerNotes = tags[0x927C];  // Maker Notes
        binaryReader.BaseStream.Seek(makerNotes.ValuePointer, SeekOrigin.Begin);
        var notes = new ImageFileDirectory(binaryReader);
        // notes.DumpDirectory(binaryReader);

        // Camera settings
        var settings = notes.Entries.Single(e => e.TagId == 0x0001 && e.TagType == ImageFileEntry.TagTypes.UShort);
        //Console.WriteLine("Camera Settings id: {0}, type: {1}, count {2}, value {3}", settings.TagId, settings.TagType, settings.NumberOfValue, settings.ValuePointer);
        binaryReader.BaseStream.Seek(settings.ValuePointer, SeekOrigin.Begin);
        var settingsData = new ushort[settings.NumberOfValue];
        for (var i = 0; i < settings.NumberOfValue; i++)
            settingsData[i] = binaryReader.ReadUInt16();
        Assert.Equal(2, settingsData[1]);    // MacroMode == Normal
        Assert.Equal(4, settingsData[3]);    // Qualtiy == RAW
        Assert.Equal(0, settingsData[4]);    // Flash == Off
        Assert.Equal(6, settingsData[9]);    // RecordMode == CR2

        // focus info
        //var focalLength = notes.Entries.Single(e => e.TagId == 0x0002 && e.TagType == ImageFileEntry.TagTypes.UShort);
        //Console.WriteLine("Focal Length: {0}, type: {1}, count {2}, value {3}", focalLength.TagId, focalLength.TagType, focalLength.NumberOfValue, focalLength.ValuePointer);
        //binaryReader.BaseStream.Seek(focalLength.ValuePointer, SeekOrigin.Begin);
        //for (var i = 0; i < focalLength.NumberOfValue; i++)
        //    var x = binaryReader.ReadUInt16();

        // shot info
        var shot = notes.Entries.Single(e => e.TagId == 0x0004 && e.TagType == ImageFileEntry.TagTypes.UShort);
        binaryReader.BaseStream.Seek(shot.ValuePointer, SeekOrigin.Begin);
        var shotData = new ushort[shot.NumberOfValue];
        for (var i = 0; i < shot.NumberOfValue; i++)
            shotData[i] = binaryReader.ReadUInt16();
        Assert.Equal(0x0000, shotData[7]);    //! Auto - White balance

        // ProcessingInfo
        var process = notes.Entries.Single(e => e.TagId == 0x00A0 && e.TagType == ImageFileEntry.TagTypes.UShort);
        binaryReader.BaseStream.Seek(process.ValuePointer, SeekOrigin.Begin);
        var processData = new ushort[process.NumberOfValue];
        for (var i = 0; i < process.NumberOfValue; i++)
            processData[i] = binaryReader.ReadUInt16();
        Assert.Equal(0x0000, processData[6]);    // White balance - Red
        Assert.Equal(0x0000, processData[7]);    // White balance - Blue
        Assert.Equal(0xFFFF, processData[8]);    // White balance
        Assert.Equal(5200, processData[9]);      // Color Temp

        // Mesaured Color Tags
        //var colorTags = notes.Entries.Single(e => e.TagId == 0x00aa && e.TagType == ImageFileEntry.TagTypes.UShort);
        //binaryReader.BaseStream.Seek(colorTags.ValuePointer, SeekOrigin.Begin);
        //var colorTagsData = new ushort[colorTags.NumberOfValue];
        //for (var i = 0; i < colorTags.NumberOfValue; i++)
        //    colorTagsData[i] = binaryReader.ReadUInt16();

        // Color Data
        var colorBalance = notes.Entries.Single(e => e.TagId == 0x4001 && e.TagType == ImageFileEntry.TagTypes.UShort);
        binaryReader.BaseStream.Seek(colorBalance.ValuePointer, SeekOrigin.Begin);
        var colorBalanceTags = new ushort[colorBalance.NumberOfValue];
        for (var i = 0; i < colorBalance.NumberOfValue; i++)
            colorBalanceTags[i] = binaryReader.ReadUInt16();

        // color data version
        Assert.Equal(10, colorBalanceTags[0]);

        // WB_RGGBLevelsAsShot
        Assert.Equal(0x07F0, colorBalanceTags[63]);
        Assert.Equal(0x0400, colorBalanceTags[64]);
        Assert.Equal(0x0400, colorBalanceTags[65]);
        Assert.Equal(0x06A6, colorBalanceTags[66]);

        // ColorTempAsShot
        Assert.Equal(4997, colorBalanceTags[67]);

        // WB_RGGBLevelsAuto
        Assert.Equal(0x07F0, colorBalanceTags[68]);
        Assert.Equal(0x0400, colorBalanceTags[69]);
        Assert.Equal(0x0400, colorBalanceTags[70]);
        Assert.Equal(0x06A6, colorBalanceTags[71]);

        // ColorTempAuto
        Assert.Equal(4997, colorBalanceTags[72]);

        // WB_RGGBLevelsMeasured
        Assert.Equal(0x07F0, colorBalanceTags[73]);
        Assert.Equal(0x0400, colorBalanceTags[74]);
        Assert.Equal(0x0400, colorBalanceTags[75]);
        Assert.Equal(0x06A6, colorBalanceTags[76]);

        // ColorTempMeasured
        Assert.Equal(4997, colorBalanceTags[77]);

        // 213-275 color clib tags

        // Average Black level
        Assert.Equal(0x0800, colorBalanceTags[276]);
        Assert.Equal(0x0800, colorBalanceTags[277]);
        Assert.Equal(0x0800, colorBalanceTags[278]);
        Assert.Equal(0x0800, colorBalanceTags[279]);

        // Raw masured RGGB
        Assert.Equal(0x000191A3, colorBalanceTags[429] << 16 | colorBalanceTags[430]);
        Assert.Equal(0x00018143, colorBalanceTags[431] << 16 | colorBalanceTags[432]);
        Assert.Equal(0x00017B99, colorBalanceTags[433] << 16 | colorBalanceTags[434]);
        Assert.Equal(0x000090B6, colorBalanceTags[435] << 16 | colorBalanceTags[436]);

        // var wb = new WhiteBalance(binaryReader, colorBalance);
    }

    [Fact]
    public void CanonDustDeleteData()
    {
        const string fileName = @"\\Data\Photo\Source\5DIIIhigh.CR2";
        using var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        var ifid0 = rawImage.Directories.First();
        var exif = ifid0[0x8769];
        binaryReader.BaseStream.Seek(exif.ValuePointer, SeekOrigin.Begin);
        var tags = new ImageFileDirectory(binaryReader);

        var makerNotes = tags[0x927C];
        binaryReader.BaseStream.Seek(makerNotes.ValuePointer, SeekOrigin.Begin);
        var notes = new ImageFileDirectory(binaryReader);
        // notes.DumpDirectory(binaryReader);

        var data = notes[0x0097];
        binaryReader.BaseStream.Seek(data.ValuePointer, SeekOrigin.Begin);
        for (var i = 0; i < data.NumberOfValue; i++)
        {
            var x = binaryReader.ReadUInt16();
            Console.WriteLine("{0} : {1}", i, x);
        }
    }

    [Fact]
    public void VrdEndTags5D3()
    {
        const string fileName = @"\\Data\Photo\2016\2016-05-10\0L2A2451.CR2";

        using var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var data = VrdData.ParseFile(binaryReader);
        Assert.IsType<VrdDataF7>(data[0]);
        foreach (var vrdData in data)
            vrdData.DumpData();
    }

    [Fact]
    public void VrdEndTags7D()
    {
        const string fileName = @"\\Data\Photo\Source\7Dhigh - Copy.CR2";

        using var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var data = VrdData.ParseFile(binaryReader);
        Assert.IsType<VrdDataF7>(data[0]);
        foreach (var vrdData in data)
            vrdData.DumpData();
    }

    [Fact]
    public void VrdEndTags7DD()
    {
        const string fileName = @"\\Data\Photo\Source\7Dhigh - Copy - Copy.CR2";

        using var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var data = VrdData.ParseFile(binaryReader);
        Assert.IsType<VrdDataF4>(data[0]);
        Assert.IsType<VrdDataF7>(data[1]);
        foreach (var vrdData in data)
            vrdData.DumpData();
    }

    [Fact]
    public void VrdEndTags40D()
    {
        const string fileName = @"\\Data\Photo\2015\2015-08-08\IMG_4331.CR2";

        using var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var data = VrdData.ParseFile(binaryReader);
        Assert.IsType<VrdDataF4>(data[0]);
        foreach (var vrdData in data)
            vrdData.DumpData();
    }

    [Fact]
    public void TestMethodTags()
    {
        const string fileName2 = @"\\Data\Photo\Source\5DIIIhigh.CR2";

        using var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        var ifd0 = rawImage.Directories.First();
        var make = ifd0[0x010f];
        Assert.Equal("Canon", RawImage.ReadChars(binaryReader, make));
        var model = ifd0[0x0110];
        // Assert.Equal("Canon EOS 7D", RawImage.ReadChars(binaryReader, model));
        Assert.Equal("Canon EOS 5D Mark III", RawImage.ReadChars(binaryReader, model));

        var exif = ifd0[0x8769];
        binaryReader.BaseStream.Seek(exif.ValuePointer, SeekOrigin.Begin);
        var tags = new ImageFileDirectory(binaryReader);

        var makerNotes = tags[0x927C];
        binaryReader.BaseStream.Seek(makerNotes.ValuePointer, SeekOrigin.Begin);
        var notes = new ImageFileDirectory(binaryReader);

        var white = notes[0x4001];
        Console.WriteLine("0x{0:X4}, {1}, {2}, {3}", white.TagId, white.TagType, white.NumberOfValue, white.ValuePointer);
        // var wb = new WhiteBalance(binaryReader, white);
        ReadSomeData(binaryReader, white.ValuePointer);

        var size2 = notes[0x4002];
        Console.WriteLine("0x{0:X4}, {1}, {2}, {3}", size2.TagId, size2.TagType, size2.NumberOfValue, size2.ValuePointer);
        ReadSomeData(binaryReader, size2.ValuePointer);

        var size5 = notes[0x4005];
        Console.WriteLine("0x{0:X4}, {1}, {2}, {3}", size5.TagId, size5.TagType, size5.NumberOfValue, size5.ValuePointer);
        ReadSomeData(binaryReader, size5.ValuePointer);
    }

    private static void ReadSomeData(BinaryReader binaryReader, uint valuePointer)
    {
        if (binaryReader.BaseStream.Position != valuePointer)
        {
            binaryReader.BaseStream.Seek(valuePointer, SeekOrigin.Begin);
        }

        var ar = 0;
        var length = binaryReader.ReadUInt16();
        Console.WriteLine("0x{0:X4} Len = {1} Length", ar, length);
        ar += 2;
    }

    private static short DecodeDifBits(ushort difBits, ushort difCode)
    {
        short dif0;
        if ((difCode & (0x01u << (difBits - 1))) != 0)
        {
            // msb is 1, thus decoded DifCode is positive
            dif0 = (short)difCode;
        }
        else
        {
            // msb is 0, thus DifCode is negative
            var mask = (1 << difBits) - 1;
            var m1 = difCode ^ mask;
            dif0 = (short)(0 - m1);
        }
        return dif0;
    }

    private static ushort GetValue(ImageData imageData, HuffmanTable table)
    {
        var hufIndex = (ushort)0;
        var hufBits = (ushort)0;
        HuffmanTable.HCode hCode;
        do
        {
            hufIndex = imageData.GetNextShort(hufIndex);
            hufBits++;
        }
        while (!table.Dictionary.TryGetValue(hufIndex, out hCode) || hCode.Length != hufBits);

        return hCode.Code;
    }
}
