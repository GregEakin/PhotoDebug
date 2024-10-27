//    Copyright 2022 Gregory Eakin
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.IO;
using System.Linq;
using PhotoLib.Jpeg.JpegTags;
using PhotoLib.Tiff;
using Xunit;

namespace PhotoTests.Canon5D2;

public class UnitTest5D2
{
    private const string FileName = @"P:\2022\2022-07-25\IMG_3975.CR2";

    public UnitTest5D2()
    {
        Assert.True(File.Exists(FileName), $"Image file {FileName} doesn't exists!");
    }

    [Fact]
    public void RawImageDumpData()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);
        Assert.Equal(new byte[] { 0x49, 0x49 }, rawImage.Header.ByteOrder);
        Assert.Equal(0x002A, rawImage.Header.TiffMagic);
        Assert.Equal(0x5243, rawImage.Header.CR2Magic);
        Assert.Equal(new byte[] { 0x02, 0x00 }, rawImage.Header.CR2Version);

        rawImage.DumpHeader(binaryReader);
    }

    [Fact]
    public void RawImageSize()
    {
        // 1 Sensor Width                    : 5920 = 1 * 2960 + 2960
        // 2 Sensor Height                   : 3950

        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        var directory = rawImage.Directories.Last();
        var address = directory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
        var length = directory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
        var strips = directory.Entries.Single(e => e.TagId == 0xC640 && e.TagType == ImageFileEntry.TagTypes.UShort).ValuePointer; // TIF_CR2_SLICE

        binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
        var x = binaryReader.ReadUInt16();
        var y = binaryReader.ReadUInt16();
        var z = binaryReader.ReadUInt16();
        Assert.Equal(2, x);
        Assert.Equal(1936, y);
        Assert.Equal(1920, z);

        binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
        var startOfImage = new StartOfImage(binaryReader, address, length);
        var lossless = startOfImage.StartOfFrame;
        Assert.Equal(14, lossless.Precision);
        Assert.Equal(4, lossless.Components.Length);
        Assert.Equal(1448, lossless.SamplesPerLine);
        Assert.Equal(3804, lossless.ScanLines);
    }

    [Fact]
    public void Bits()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        var directory = rawImage.Directories.Last();
        var address = directory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
        var length = directory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS

        binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
        var startOfImage = new StartOfImage(binaryReader, address, length);
        var lossless = startOfImage.StartOfFrame;
        Assert.Equal(14, lossless.Precision);

        var startOfScan = startOfImage.StartOfScan;
        Assert.Equal(0, startOfScan.Bb3 & 0x0F);

        Assert.Equal(14, lossless.Precision - (startOfScan.Bb3 & 0x0f));
    }

    [Fact]
    public void Colors()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        var directory = rawImage.Directories.Last();
        var address = directory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
        var length = directory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS

        binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
        var startOfImage = new StartOfImage(binaryReader, address, length);
        var lossless = startOfImage.StartOfFrame;

        Assert.Equal(4, lossless.Components.Length); // clrs
        foreach (var component in lossless.Components)
        {
            Assert.Equal(1, component.HFactor); // sraw
            Assert.Equal(1, component.VFactor); // sraw
        }

        Assert.Equal(4, lossless.Components.Sum(comp => comp.HFactor * comp.VFactor));
    }

    [Fact]
    public void PredictorSelectionValue()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        var directory = rawImage.Directories.Last();
        var address = directory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
        var length = directory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS

        binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
        var startOfImage = new StartOfImage(binaryReader, address, length);
        Assert.Equal(1, startOfImage.StartOfScan.Bb1); // Do nothing
    }

    [Fact]
    public void DumpRawImageHex()
    {
        // 1 Sensor Width                    : 5760
        // 2 Sensor Height                   : 3840

        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        var directory = rawImage.Directories.Last();
        var address = directory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
        var length = directory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
        DumpBlock(binaryReader, address, length, 256);

        address = address + length - 64;
        DumpBlock(binaryReader, address, length, 64);
    }

    private static void DumpBlock(BinaryReader binaryReader, uint address, uint length, uint size)
    {
        const int Width = 16;
        binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
        for (var i = 0; i < size; i += Width)
        {
            Console.Write("0x{0:X8}: ", (address + i));
            var nextStep = (int)Math.Min(Width, length - i);
            var data = binaryReader.ReadBytes(nextStep);
            foreach (var b in data)
            {
                Console.Write("{0:X2} ", b);
            }
            Console.WriteLine();
        }
        Console.WriteLine("...");
    }

    [Fact]
    public void TestMethod6()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
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
        Assert.Equal(0xFF, startOfImage.Mark);
        Assert.Equal(0xD8, startOfImage.Tag); // JPG_MARK_SOI

        var huffmanTable = startOfImage.HuffmanTable;
        Assert.Equal(0xFF, huffmanTable.Mark);
        Assert.Equal(0xC4, huffmanTable.Tag);

        // This file has two huffman tables: 0x00 and 0x01
        Assert.Equal(2, huffmanTable.Tables.Count);
        Assert.True(huffmanTable.Tables.ContainsKey(0x00));
        Assert.True(huffmanTable.Tables.ContainsKey(0x01));

        var lossless = startOfImage.StartOfFrame;
        Assert.Equal(0xFF, lossless.Mark);
        Assert.Equal(0xC3, lossless.Tag);

        Assert.Equal(14, lossless.Precision);
        Assert.Equal(4, lossless.Components.Length);
        Assert.Equal(1448, lossless.SamplesPerLine);
        Assert.Equal(3804, lossless.ScanLines);

        Assert.Equal(5792, lossless.Width); // Sensor width (bits)
        Assert.Equal(5792, lossless.SamplesPerLine * lossless.Components.Length);
        Assert.Equal(5792, x * y + z);

        foreach (var component in lossless.Components)
        {
            // Console.WriteLine("== {0}: {1} {2} {3}", component.ComponentId, component.HFactor, component.VFactor, component.TableId);
            Assert.Equal(0x01, component.HFactor);
            Assert.Equal(0x01, component.VFactor);
            Assert.Equal(0x00, component.TableId);
        }

        var startOfScan = startOfImage.StartOfScan;
        Assert.Equal(0xFF, startOfScan.Mark);
        Assert.Equal(0xDA, startOfScan.Tag);

        //foreach (var scanComponent in startOfScan.Components)
        //{
        //    Console.WriteLine("{0}: {1} {2}", scanComponent.Id, scanComponent.Dc, scanComponent.Ac);
        //}

        var imageData = startOfImage.ImageData;
    }

    [Fact]
    public void Test1()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // The first IFD contains a small RGB version of the picture (one fourth the size) compressed in Jpeg, the EXIF part, and the Makernotes part. 
        // The second IFD contains a small RGB version (160x120 pixels) of the picture, compressed in Jpeg.
        // The third IFD contains a small RGB version of the picture, NOT compressed (even with compression==6), and one which no white balance, correction has been applied.
        // The fourth IFD contains the RAW data compressed in lossless Jpeg. 

        var directory = rawImage.Directories.Skip(3).First();
        var address = directory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
        var length = directory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS

        binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
        var startOfImage = new StartOfImage(binaryReader, address, length);

        // Assert.Equal(0, startOfImage.);
    }

    [Fact]
    public void Test2()
    {
        using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var binaryReader = new BinaryReader(fileStream);
        var rawImage = new RawImage(binaryReader);

        // The first IFD contains a small RGB version of the picture (one fourth the size) compressed in Jpeg, the EXIF part, and the Makernotes part. 
        // The second IFD contains a small RGB version (160x120 pixels) of the picture, compressed in Jpeg.
        // The third IFD contains a small RGB version of the picture, NOT compressed (even with compression==6), and one which no white balance correction has been applied.
        // The fourth IFD contains the RAW data compressed in lossless Jpeg. 

        var directory = rawImage.Directories.First();
        // stripOffset 6)  0x0111 ULong 32-bit: 96332
        // orientation 7)  0x0112 UShort 16-bit: 1
        // stripByteCounts 8)  0x0117 ULong 32-bit: 2390306
        var address = directory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
        var orientation = directory.Entries.Single(e => e.TagId == 0x0112).ValuePointer;
        var length = directory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS

        Assert.Equal(0x0000D7F4u, address);
        Assert.Equal(0x00115411u, length);
        Assert.Equal(1u, orientation);

        binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
        var startOfImage = new StartOfImage(binaryReader, address, length);

        // Assert.Equal(0, startOfImage.);
    }
}
