// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		C5D3Ifd1.cs
// AUTHOR:		Greg Eakin

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg.JpegTags;
using PhotoLib.Tiff;

namespace PhotoTests.Canon5D3
{
    // The first IFD contains a small RGB version of the picture (one fourth the size) compressed in Jpeg, the EXIF part, and the Makernotes part. 
    // The second IFD contains a small RGB version (160x120 pixels) of the picture, compressed in Jpeg.
    // The third IFD contains a small RGB version of the picture, NOT compressed (even with compression==6), and one which no white balance, correction has been applied.
    // The fourth IFD contains the RAW data compressed in lossless Jpeg. 

    [TestClass]
    public class C5D3Ifd2
    {
        private const string FileName = @"d:\Users\Greg\Pictures\2018-08-29\0L2A3743.CR2";

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Assert.IsTrue(File.Exists(FileName), "Image file {0} doesn't exists!", FileName);
        }

        // == Tiff Directory[0x000118AC]:
        // 0)   0x0100 UShort 16-bit: 592                                        -- Image width
        // 1)   0x0101 UShort 16-bit: 395                                        -- Image heigth
        // 2)   0x0102 UShort 16-bit: [0x0001194E] (3): 16, 16, 16,              -- bits per sample
        // 3)   0x0103 UShort 16-bit: 1                                          -- compression, 1== uncompressed, 6 == old jpeg
        // 4)   0x0106 UShort 16-bit: 2                                          -- photometric interpretation, 2 == RGB
        // 5)   0x0111 ULong 32-bit: 2794548                                     -- strip offset
        // 6)   0x0115 UShort 16-bit: 3                                          -- samples per pixel
        // 7)   0x0116 UShort 16-bit: 395                                        -- rows per strip
        // 8)   0x0117 ULong 32-bit: 1403040                                     -- strip byte counts
        // 9)   0x011C UShort 16-bit: 1                                          -- planar configuration, 1 == chunky
        // 10)  0xC5D9 ULong 32-bit: 2                                           -- 
        // 11)  0xC6C5 ULong 32-bit: 3                                           -- 
        // 12)  0xC6DC ULong 32-bit: [0x00011954] (4): 0241 0182 000E 0009       -- 

        [TestMethod]
        public void DumpImageFileDirectory()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Skip(2).First();
                imageFileDirectory.DumpDirectory(binaryReader);
            }
        }

        [TestMethod]
        public void ImageSizeTest()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Skip(2).First();
                var width = (ushort)imageFileDirectory.Entries.Single(e => e.TagId == 0x0100 && e.TagType == 3).ValuePointer;
                var height = (ushort)imageFileDirectory.Entries.Single(e => e.TagId == 0x0101 && e.TagType == 3).ValuePointer;
                var samplesPerPixel = (ushort)imageFileDirectory.Entries.Single(e => e.TagId == 0x0115 && e.TagType == 3).ValuePointer;
                var bitsPerSample = imageFileDirectory.Entries.Single(e => e.TagId == 0x0102 && e.TagType == 3).ValuePointer;

                var total = width * height * samplesPerPixel * 2u;
                Assert.AreEqual(1403040u, total);

                var length = imageFileDirectory.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(1403040u, length);
            }
        }

        // 5)  0x0111 ULong 32-bit: 2794548u   -- Offset
        // 8)  0x0117 ULong 32-bit: 1403040u   -- Length
        [TestMethod]
        public void DumpImage()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Skip(2).First();

                CollectionAssert.AreEqual(
                    new ushort[] { 0x0100, 0x0101, 0x0102, 0x0103, 0x0106, 0x0111, 0x0115, 0x0116, 0x0117, 0x011C, 0xC5D9, 0xC6C5, 0xC6DC },
                    imageFileDirectory.Entries.Select(e => e.TagId).ToArray());

                var width = (ushort)imageFileDirectory.Entries.Single(e => e.TagId == 0x0100 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(592u, width);

                var height = (ushort)imageFileDirectory.Entries.Single(e => e.TagId == 0x0101 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(395u, height);

                var offset = imageFileDirectory.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(2794548u, offset);

                var length = imageFileDirectory.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(1403040u, length);

                binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
                var dir = Path.GetDirectoryName(FileName) ?? ".";
                var name = Path.GetFileNameWithoutExtension(FileName) + "-2.jpg";
                var path = Path.Combine(dir, name);
                DumpImage(path, binaryReader, width, height);
            }
        }

        private static void DumpImage(string filename, BinaryReader binaryReader, int width, int height)
        {
            var bmp = new Bitmap(width, height);
            for (var y = 0; y < height; ++y)
                for (var x = 0; x < width; ++x)
                {
                    var red = binaryReader.ReadUInt16() >> 6;
                    var green = binaryReader.ReadUInt16() >> 6;
                    var blue = binaryReader.ReadUInt16() >> 6;
                    var color = Color.FromArgb(0xFF, red, green, blue);
                    bmp.SetPixel(x, y, color);
                }
            bmp.Save(filename);
        }
    }
}