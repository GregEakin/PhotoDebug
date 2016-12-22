// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		Image3MRawIV.cs
// AUTHOR:		Greg Eakin

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg.JpegTags;
using PhotoLib.Tiff;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class Image3MRawIV
    {
        private static int cc;

        private struct DataBuf
        {
            public ushort Y;
            public short Cb;
            public short Cr;
        }

        private struct DiffBuf
        {
            public short Y1;
            public short Y2;
            public short Y3;
            public short Y4;
            public short Cb;
            public short Cr;
        }

        [TestMethod]
        public void DumpImage3MRawTest()
        {
            const string fileName = @"D:\Users\Greg\Pictures\2016-02-28 MRaw\MRaw 002.CR2";
            DumpImage3SRaw(fileName);
        }

        private static void DumpImage3SRaw(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // Image #3 is a raw image compressed in ITU-T81 lossless JPEG

                var image = rawImage.Directories.Skip(3).First();
                Assert.AreEqual(7, image.Entries.Length);

                var compression = image.Entries.Single(e => e.TagId == 0x0103 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(6u, compression);

                var offset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(0x2D42DCu, offset);

                var count = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(0x1501476u, count);

                var item3 = image.Entries.Single(e => e.TagId == 0xC5D8 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(0x1u, item3);

                var item4 = image.Entries.Single(e => e.TagId == 0xC5E0 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(0x3u, item4);

                // 0xC640 UShort 16-bit: [0x000119BE] (3): 8, 1296, 1296, 
                var imageFileEntry = image.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3);
                // Assert.AreEqual(0x000119BEu, imageFileEntry.ValuePointer);
                // Assert.AreEqual(3u, imageFileEntry.NumberOfValue);
                var slices = RawImage.ReadUInts16(binaryReader, imageFileEntry);
                CollectionAssert.AreEqual(new ushort[] { 8, 1296, 1296 }, slices);

                var item6 = image.Entries.Single(e => e.TagId == 0xC6C5 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(0x4u, item6);

                binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, offset, count);

                var startOfFrame = startOfImage.StartOfFrame;
                Assert.AreEqual(2592u, startOfFrame.ScanLines);
                Assert.AreEqual(3888u, startOfFrame.SamplesPerLine);
                Assert.AreEqual(11664, startOfFrame.Width);

                // var rowBuf0 = new short[startOfFrame.SamplesPerLine * startOfFrame.Components.Length];
                // var rowBuf1 = new short[startOfFrame.SamplesPerLine * startOfFrame.Components.Length];
                // var predictor = new[] { (short)(1 << (startOfFrame.Precision - 1)), (short)(1 << (startOfFrame.Precision - 1)) };
                Assert.AreEqual(2, startOfImage.HuffmanTable.Tables.Count);
                // var table0 = startOfImage.HuffmanTable.Tables[0x00];
                // var table1 = startOfImage.HuffmanTable.Tables[0x01];

                Assert.AreEqual(15, startOfFrame.Precision); // mraw/sraw1

                // chrominance subsampling factors
                Assert.AreEqual(3, startOfFrame.Components.Length); // mraw/sraw1

                Assert.AreEqual(1, startOfFrame.Components[0].ComponentId);
                Assert.AreEqual(2, startOfFrame.Components[0].HFactor);
                Assert.AreEqual(2, startOfFrame.Components[0].VFactor); // MRAW
                Assert.AreEqual(0, startOfFrame.Components[0].TableId);

                Assert.AreEqual(2, startOfFrame.Components[1].ComponentId);
                Assert.AreEqual(1, startOfFrame.Components[1].HFactor);
                Assert.AreEqual(1, startOfFrame.Components[1].VFactor);
                Assert.AreEqual(0, startOfFrame.Components[1].TableId);

                Assert.AreEqual(3, startOfFrame.Components[2].ComponentId);
                Assert.AreEqual(1, startOfFrame.Components[2].HFactor);
                Assert.AreEqual(1, startOfFrame.Components[2].VFactor);
                Assert.AreEqual(0, startOfFrame.Components[2].TableId);

                Assert.AreEqual(6, startOfFrame.Components.Sum(component => component.HFactor * component.VFactor));

                // mraw/sraw1
                // Y1 Y2 Y3 Y4 Cb Cr
                // Y1 Cb Cr Y2  x  x
                // Y3  x  x Y4  x  x

                var startOfScan = startOfImage.StartOfScan;
                // DumpStartOfScan(startOfScan);

                Assert.AreEqual(1, startOfScan.Bb1); // Start of spectral or predictor selection
                Assert.AreEqual(0, startOfScan.Bb2); // end of spectral selection
                Assert.AreEqual(0, startOfScan.Bb3); // successive approximation bit positions
                Assert.AreEqual(3, startOfScan.Components.Length); // sraw/sraw2

                Assert.AreEqual(1, startOfScan.Components[0].Id);
                Assert.AreEqual(0, startOfScan.Components[0].Dc);
                Assert.AreEqual(0, startOfScan.Components[0].Ac);

                Assert.AreEqual(2, startOfScan.Components[1].Id);
                Assert.AreEqual(1, startOfScan.Components[1].Dc);
                Assert.AreEqual(0, startOfScan.Components[1].Ac);

                Assert.AreEqual(3, startOfScan.Components[2].Id);
                Assert.AreEqual(1, startOfScan.Components[2].Dc);
                Assert.AreEqual(0, startOfScan.Components[2].Ac);

                // DumpCompressedData(startOfImage);

                // horizontal sampling == 1
                startOfImage.ImageData.Reset();

                var outFile = Path.ChangeExtension(fileName, ".png");
                CreateBitmap(binaryReader, startOfImage, outFile, offset, slices);

                Assert.AreEqual(15116544, cc);
                Assert.AreEqual(1, startOfImage.ImageData.DistFromEnd);
            }
        }

        private static void CreateBitmap(BinaryReader binaryReader, StartOfImage startOfImage, string outFile, uint offset, ushort[] slices)
        {
            binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

            var startOfFrame = startOfImage.StartOfFrame;
            var height = startOfFrame.ScanLines;
            Assert.AreEqual(2592, height);              // image height

            var samplesPerLine = startOfFrame.SamplesPerLine;
            Assert.AreEqual(3888, samplesPerLine);      // image width

            var width = startOfFrame.Width;
            Assert.AreEqual(11664, startOfFrame.Width);
            Assert.AreEqual(samplesPerLine * 3, slices[0] * slices[1] + slices[2]);
            Assert.AreEqual(width, samplesPerLine * 3);
            Assert.AreEqual(2, 6 * samplesPerLine / (slices[0] * slices[1] + slices[2]));

            using (var bitmap = new Bitmap(samplesPerLine, height, PixelFormat.Format48bppRgb))
            {
                var size = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var data = bitmap.LockBits(size, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                try
                {
                    Assert.AreEqual(6 * samplesPerLine, data.Stride); // 6 bytes * 8 bits == 48 bits per pixel

                    for (var slice = 0; slice < slices[0]; slice++) // 0..8
                        ProcessSlice(startOfImage, slice, slices[1], data);
                    ProcessSlice(startOfImage, slices[0], slices[2], data);
                }
                finally
                {
                    bitmap.UnlockBits(data);
                }

                bitmap.Save(outFile);
            }
        }

        private static readonly DataBuf[] lastCol = new DataBuf[2592];

        private static void ProcessSlice(StartOfImage startOfImage, int slice, int samples, BitmapData data)
        {
            var startOfFrame = startOfImage.StartOfFrame;
            var table0 = startOfImage.HuffmanTable.Tables[0x00];
            var table1 = startOfImage.HuffmanTable.Tables[0x01];

            var lastRow = new DataBuf { Y = 0x0000 };       // 0x4000

            for (var line = 0; line < startOfFrame.ScanLines; line += 2) // 0..2592
            {
                // 6 bytes * 8 bits == 48 bits per pixel
                // 2 = 6 bytes * samplesPerLine / (slices[0] * slices[1] + slices[2]);
                var scan0 = data.Scan0 + data.Stride * line + slice * samples * 2;
                var scan1 = data.Scan0 + data.Stride * (line + 1) + slice * samples * 2;

                // read six shorts, for four pixels
                for (var col = 0; col < samples / 6; col++)       // 0..1296
                {
                    cc += 6;
                    var diff = new DiffBuf
                    {
                        Y1 = startOfImage.ProcessColor(0x00),
                        Y2 = startOfImage.ProcessColor(0x00),
                        Y3 = startOfImage.ProcessColor(0x00),
                        Y4 = startOfImage.ProcessColor(0x00),
                        Cb = startOfImage.ProcessColor(0x01),
                        Cr = startOfImage.ProcessColor(0x01),
                    };

                    var pixel0 = new DataBuf
                    {
                        Y = (ushort)(lastRow.Y + diff.Y1),
                        Cb = (short)(lastRow.Cb + diff.Cb),
                        Cr = (short)(lastRow.Cr + diff.Cr)
                    };

                    var pixel1 = new DataBuf
                    {
                        Y = (ushort)(lastRow.Y + diff.Y1 + diff.Y2),
                        Cb = (short)(lastRow.Cb + diff.Cb),
                        Cr = (short)(lastRow.Cr + diff.Cr)
                    };

                    var pixel2 = new DataBuf
                    {
                        Y = (ushort)(lastRow.Y + diff.Y1 + diff.Y2 + diff.Y3),
                        Cb = (short)(lastRow.Cb + diff.Cb),
                        Cr = (short)(lastRow.Cr + diff.Cr)
                    };

                    var pixel3 = new DataBuf
                    {
                        Y = (ushort)(lastRow.Y + diff.Y1 + diff.Y2 + diff.Y3 + diff.Y4),
                        Cb = (short)(lastRow.Cb + diff.Cb),
                        Cr = (short)(lastRow.Cr + diff.Cr)
                    };

                    PokePixels(scan0, scan1, col, pixel0, pixel1, pixel2, pixel3);
                }
            }
        }

        private static void PokePixels(IntPtr scan0, IntPtr scan1, int col, DataBuf pixel0, DataBuf pixel1, DataBuf pixel2, DataBuf pixel3)
        {
            {
                var red = pixel0.Y + 1.40200 * pixel0.Cr;
                var green = pixel0.Y - 0.34414 * pixel0.Cb - 0.71414 * pixel0.Cr;
                var blue = pixel0.Y + 1.77200 * pixel0.Cb;

                Marshal.WriteInt16(scan0, 12 * col + 4, (short)red);
                Marshal.WriteInt16(scan0, 12 * col + 2, (short)green);
                Marshal.WriteInt16(scan0, 12 * col + 0, (short)blue);
            }

            {
                var red = pixel1.Y + 1.40200 * pixel1.Cr;
                var green = pixel1.Y - 0.34414 * pixel1.Cb - 0.71414 * pixel1.Cr;
                var blue = pixel1.Y + 1.77200 * pixel1.Cb;

                Marshal.WriteInt16(scan0, 12 * col + 10, (short)red);
                Marshal.WriteInt16(scan0, 12 * col + 8, (short)green);
                Marshal.WriteInt16(scan0, 12 * col + 6, (short)blue);
            }

            {
                var red = pixel2.Y + 1.40200 * pixel2.Cr;
                var green = pixel2.Y - 0.34414 * pixel2.Cb - 0.71414 * pixel2.Cr;
                var blue = pixel2.Y + 1.77200 * pixel2.Cb;

                Marshal.WriteInt16(scan1, 12 * col + 4, (short)red);
                Marshal.WriteInt16(scan1, 12 * col + 2, (short)green);
                Marshal.WriteInt16(scan1, 12 * col + 0, (short)blue);
            }

            {
                var red = pixel3.Y + 1.40200 * pixel3.Cr;
                var green = pixel3.Y - 0.34414 * pixel3.Cb - 0.71414 * pixel3.Cr;
                var blue = pixel3.Y + 1.77200 * pixel3.Cb;

                Marshal.WriteInt16(scan1, 12 * col + 10, (short)red);
                Marshal.WriteInt16(scan1, 12 * col + 8, (short)green);
                Marshal.WriteInt16(scan1, 12 * col + 6, (short)blue);
            }
        }
    }
}
