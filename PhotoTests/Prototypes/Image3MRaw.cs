// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		Image3MRaw.cs
// AUTHOR:		Greg Eakin

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg.JpegTags;
using PhotoLib.Tiff;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class Image3MRaw
    {
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

        private static int cc;

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

                // 
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

                // horz sampling == 1
                startOfImage.ImageData.Reset();

                var memory = new DataBuf[startOfFrame.ScanLines][]; // [2592][]
                for (var line = 0; line < startOfFrame.ScanLines; line++) // 0 .. 2592
                {
                    var diff = ReadDiffRow(startOfImage);
                    // VerifyDiff(diff, line);
                    var memory1 = ProcessDiff(diff, startOfFrame.SamplesPerLine); //
                    memory[line] = memory1;
                }

                Assert.AreEqual(10077696, cc);
                Assert.AreEqual(1, startOfImage.ImageData.DistFromEnd);

                var outFile = Path.ChangeExtension(fileName, ".bmp");
                MakeBitmap(memory, outFile, slices);
            }
        }

        private static DiffBuf[] ReadDiffRow(StartOfImage startOfImage)
        {
            var startOfFrame = startOfImage.StartOfFrame;
            int samplesPerLine = startOfFrame.SamplesPerLine;

            var diff = new DiffBuf[samplesPerLine / 4];         // 648
            for (var x = 0; x < samplesPerLine / 4; x++)        // 0..648
            {
                diff[x].Y1 = startOfImage.ProcessColor(0x00);
                diff[x].Y2 = startOfImage.ProcessColor(0x00);
                diff[x].Y3 = startOfImage.ProcessColor(0x00);
                diff[x].Y4 = startOfImage.ProcessColor(0x00);
                diff[x].Cb = startOfImage.ProcessColor(0x01);
                diff[x].Cr = startOfImage.ProcessColor(0x01);
                cc += 4;
            }

            return diff;
        }

        private static void VerifyDiff(DiffBuf[] diff, int line)
        {
            // Debug: Dump the diff data.
            {
                var y1 = (double)0x4000; var minY = double.MaxValue; var maxY = double.MinValue;
                var y2 = (double)0x4000;
                var y3 = (double)0x4000;
                var y4 = (double)0x4000;
                var cb = 0.0; var minCb = double.MaxValue; var maxCb = double.MinValue;
                var cr = 0.0; var minCr = double.MaxValue; var maxCr = double.MinValue;

                for (var x = 0; x < diff.Length; x++)
                {
                    y1 += diff[x].Y1;
                    y2 += diff[x].Y2;
                    y3 += diff[x].Y2;
                    y4 += diff[x].Y2;
                    if (minY > y1 + y2 + y3 + y4) minY = y1 + y2 + y3 + y4;
                    if (maxY < y1 + y2 + y3 + y4) maxY = y1 + y2 + y3 + y4;

                    cb += diff[x].Cb;
                    if (minCb > cb) minCb = cb;
                    if (maxCb < cb) maxCb = cb;

                    cr += diff[x].Cr;
                    if (minCr > cb) minCr = cr;
                    if (maxCr < cb) maxCr = cr;
                }

                // if (line == 1000 || line == 0 || line == 1 || line == 999)
                {
                    Console.Write("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}  ", 0, line, y1, y2, y3, y4, cb, cr);
                    Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5},  ", minY, maxY, minCb, maxCb, minCr, maxCb);
                }
            }
        }

        private static DataBuf[] ProcessDiff(DiffBuf[] diff, int samplesPerLine)
        {
            Assert.AreEqual(samplesPerLine / 4, diff.Length);

            var prev = new DataBuf { Y = 0x4000, Cb = 0, Cr = 0 };

            var memory = new DataBuf[samplesPerLine];       // 2592
            for (var x = 0; x < samplesPerLine / 4; x++)    // 2592
            {
                var y1 = (ushort)(prev.Y + diff[x].Y1);
                var y2 = (ushort)(prev.Y + diff[x].Y1 + diff[x].Y2);
                var y3 = (ushort)(prev.Y + diff[x].Y1 + diff[x].Y2 + diff[x].Y3);
                var y4 = (ushort)(prev.Y + diff[x].Y1 + diff[x].Y2 + diff[x].Y3 + diff[x].Y4);
                var cb = (short)(prev.Cb + diff[x].Cb);
                var cr = (short)(prev.Cr + diff[x].Cr);

                prev.Y = y2;
                prev.Cb = cb;
                prev.Cr = cr;

                memory[4 * x].Y = y1;
                memory[4 * x].Cb = cb;
                memory[4 * x].Cr = cr;

                memory[4 * x + 1].Y = y2;
                memory[4 * x + 1].Cb = cb;
                memory[4 * x + 1].Cr = cr;
            }

            return memory;
        }

        private static void MakeBitmap(DataBuf[][] memory, string folder, ushort[] slices)
        {
            var y = memory.GetLength(0);
            var x = memory[0].GetLength(0);

            //using (var bitmap = new Bitmap(x, y, PixelFormat.Format24bppRgb))
            //{
            //    var size = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            //    var data = bitmap.LockBits(size, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            //    try
            //    {
            //        for (var row = 0; row < y; row++)
            //        {
            //            var scan0 = data.Scan0 + data.Stride * row;
            //            for (var col = 0; col < x; col++)
            //            {
            //                var pt = memory[row, col];
            //                var r = pt.Y + 1.40200 * pt.Cr;
            //                var g = pt.Y - 0.34414 * pt.Cb - 0.71414 * pt.Cr;
            //                var b = pt.Y + 1.77200 * pt.Cb;
            //                Marshal.WriteInt16(scan0, 3 * col + 0, check(b));
            //                Marshal.WriteInt16(scan0, 3 * col + 1, check(g));
            //                Marshal.WriteInt16(scan0, 3 * col + 2, check(r));
            //            }
            //        }
            //    }
            //      
            //    {
            //        bitmap.UnlockBits(data);
            //    }

            //    bitmap.Save(folder + "0L2A8897-3.bmp");
            //}

            Assert.AreEqual(2592, y);   // scan lines
            Assert.AreEqual(3888, x);   // samples per line

            Assert.AreEqual(3 * x, slices[0] * slices[1] + slices[2]);
            CollectionAssert.AreEqual(new[] { (ushort)8, (ushort)1296, (ushort)1296 }, slices);
            slices[1] /= 3;
            slices[2] /= 3;

            using (var bitmap = new Bitmap(x, y))
            {
                for (var mrow = 0; mrow < y; mrow++)  // 0..1728
                {
                    var rdata = memory[mrow];
                    for (var mcol = 0; mcol < x; mcol++)    // 0..2592
                    {
                        var index = mrow * x + mcol;
                        var slice = index / (slices[1] * y);
                        if (slice > slices[0])
                            slice = slices[0];
                        var offset = index - slice * (slices[1] * y);
                        var page = slice < slices[0] ? 1 : 2;
                        var brow = offset / slices[page];
                        var bcol = offset % slices[page] + slice * slices[1];

                        var val = rdata[mcol];
                        PixelSet(bitmap, brow, bcol, val);
                    }
                }

                bitmap.Save(folder + "0L2A8897-3.bmp");
            }
        }

        private static void PixelSet(Bitmap bitmap, int row, int col, DataBuf val)
        {
            var r = val.Y + 1.40200 * val.Cr;
            var g = val.Y - 0.34414 * val.Cb - 0.71414 * val.Cr;
            var b = val.Y + 1.77200 * val.Cb;
            var color = Color.FromArgb((byte)((int)r >> 7), (byte)((int)g >> 7), (byte)((int)b >> 7));
            bitmap.SetPixel(col, row, color);
        }
    }
}
