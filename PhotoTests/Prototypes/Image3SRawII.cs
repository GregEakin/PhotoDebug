using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg;
using PhotoLib.Tiff;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class Image3SRawII
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
            public short Cb;
            public short Cr;
        }

        static int cc;

        [TestMethod]
        public void DumpImage3SRawTest()
        {
            const string fileName = @"D:\Users\Greg\Pictures\2016-02-26\003.CR2";
            DumpImage3SRaw(fileName);
        }

        private static void DumpImage3SRaw(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                // Image #3 is a raw image compressed in ITU-T81 lossless JPEG
                {
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

                    // 0xC640 UShort 16-bit: [0x000119BE] (3): 5, 864, 864, 
                    var imageFileEntry = image.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3);
                    // Assert.AreEqual(0x000119BEu, imageFileEntry.ValuePointer);
                    Assert.AreEqual(3u, imageFileEntry.NumberOfValue);
                    var slices = RawImage.ReadUInts16(binaryReader, imageFileEntry);
                    CollectionAssert.AreEqual(new ushort[] { 5, 864, 864 }, slices);

                    var item6 = image.Entries.Single(e => e.TagId == 0xC6C5 && e.TagType == 4).ValuePointer;
                    Assert.AreEqual(0x4u, item6);

                    binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
                    var startOfImage = new StartOfImage(binaryReader, offset, count); // { ImageData = new ImageData(binaryReader, count) };

                    var startOfFrame = startOfImage.StartOfFrame;
                    Assert.AreEqual(1728u, startOfFrame.ScanLines);
                    Assert.AreEqual(2592u, startOfFrame.SamplesPerLine);
                    Assert.AreEqual(7776, startOfFrame.Width);

                    Assert.AreEqual(2, startOfImage.HuffmanTable.Tables.Count);
                    //var table0 = startOfImage.HuffmanTable.Tables[0x00];
                    //var table1 = startOfImage.HuffmanTable.Tables[0x01];

                    Assert.AreEqual(15, startOfFrame.Precision); // sraw/sraw2

                    // chrominance subsampling factors
                    Assert.AreEqual(3, startOfFrame.Components.Length); // sraw/sraw2

                    // J:a:b = 4:2:2, h/v = 2/1
                    Assert.AreEqual(1, startOfFrame.Components[0].ComponentId);
                    Assert.AreEqual(2, startOfFrame.Components[0].HFactor);
                    Assert.AreEqual(1, startOfFrame.Components[0].VFactor);
                    Assert.AreEqual(0, startOfFrame.Components[0].TableId);

                    Assert.AreEqual(2, startOfFrame.Components[1].ComponentId);
                    Assert.AreEqual(1, startOfFrame.Components[1].HFactor);
                    Assert.AreEqual(1, startOfFrame.Components[1].VFactor);
                    Assert.AreEqual(0, startOfFrame.Components[1].TableId);

                    Assert.AreEqual(3, startOfFrame.Components[2].ComponentId);
                    Assert.AreEqual(1, startOfFrame.Components[2].HFactor);
                    Assert.AreEqual(1, startOfFrame.Components[2].VFactor);
                    Assert.AreEqual(0, startOfFrame.Components[2].TableId);

                    // sraw/sraw2
                    // Y1 Y2 Cb Cr ...
                    // Y1 Cb Cr Y2 x x
                    // Y1 Cb Cr Y2 x x

                    var startOfScan = startOfImage.StartOfScan;
                    // DumpStartOfScan(startOfScan);

                    Assert.AreEqual(1, startOfScan.Bb1);    // Start of spectral or predictor selection
                    Assert.AreEqual(0, startOfScan.Bb2);    // end of spectral selection
                    Assert.AreEqual(0, startOfScan.Bb3);    // successive approximation bit positions
                    Assert.AreEqual(3, startOfScan.Components.Length);   // sraw/sraw2

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

                    var memory = new DataBuf[startOfFrame.ScanLines][];          // [1728][]
                    var prev = new DataBuf { Y = 0x4000, Cb = 0, Cr = 0 };
                    for (var line = 0; line < startOfFrame.ScanLines; line++)   // 0 .. 1728
                    {
                        var diff = ReadDiffRow(startOfImage); 
                        // VerifyDiff(diff, line);
                        var memory1 = ProcessDiff(diff, prev);  // 2592
                        memory[line] = memory1;
                    }

                    Assert.AreEqual(8957952, cc);
                    Assert.AreEqual(3, startOfImage.ImageData.DistFromEnd);

                    var outFile = Path.ChangeExtension(fileName, ".bmp");
                    MakeBitmap(memory, outFile, slices);
                }
            }
        }

        private static DiffBuf[] ReadDiffRow(StartOfImage startOfImage)
        {
            var startOfFrame = startOfImage.StartOfFrame;
            int samplesPerLine = startOfFrame.SamplesPerLine;
            var table0 = startOfImage.HuffmanTable.Tables[0x00];
            var table1 = startOfImage.HuffmanTable.Tables[0x01];

            var diff = new DiffBuf[samplesPerLine / 2];         // 1296
            for (var x = 0; x < samplesPerLine / 2; x++)        // 1296
            {
                diff[x].Y1 = ProcessColor(startOfImage, table0);
                diff[x].Y2 = ProcessColor(startOfImage, table0);
                diff[x].Cb = ProcessColor(startOfImage, table1);
                diff[x].Cr = ProcessColor(startOfImage, table1);

                cc += 4;
            }

            return diff;
        }

        private static short ProcessColor(StartOfImage startOfImage, HuffmanTable table)
        {
            var hufBits = startOfImage.ImageData.GetValue(table);
            var difCode = startOfImage.ImageData.GetValue(hufBits);
            var difValue = HuffmanTable.DecodeDifBits(hufBits, difCode);
            return difValue;
        }

        private static void VerifyDiff(DiffBuf[] diff, int line)
        {
            // Debug: Dump the diff data.
            {
                var y1 = (double)0x4000; var minY = double.MaxValue; var maxY = double.MinValue;
                var y2 = (double)0x4000;
                var cb = 0.0; var minCb = double.MaxValue; var maxCb = double.MinValue;
                var cr = 0.0; var minCr = double.MaxValue; var maxCr = double.MinValue;

                for (var x = 0; x < diff.Length; x++)
                {
                    y1 += diff[x].Y1;
                    y2 += diff[x].Y2;
                    if (minY > y1 + y2) minY = y1 + y2;
                    if (maxY < y1 + y2) maxY = y1 + y2;

                    cb += diff[x].Cb;
                    if (minCb > cb) minCb = cb;
                    if (maxCb < cb) maxCb = cb;

                    cr += diff[x].Cr;
                    if (minCr > cb) minCr = cr;
                    if (maxCr < cb) maxCr = cr;
                }

                // if (line == 1000 || line == 0 || line == 1 || line == 999)
                {
                    Console.Write("{0}, {1}, {2}, {3}, {4}, {5},  ", 0, line, y1, y2, cb, cr);
                    Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5},  ", minY, maxY, minCb, maxCb, minCr, maxCb);
                }
            }
        }

        private static DataBuf[] ProcessDiff(DiffBuf[] diff, DataBuf prev)
        {
            var memory = new DataBuf[2 * diff.Length];       // 2592
            for (var x = 0; x < diff.Length; x++)    // 2592
            {
                var y1 = (ushort)(prev.Y + diff[x].Y1);
                var y2 = (ushort)(prev.Y + diff[x].Y1 + diff[x].Y2);
                var cb = (short)(prev.Cb + diff[x].Cb);
                var cr = (short)(prev.Cr + diff[x].Cr);

                prev.Y = y2;
                prev.Cb = cb;
                prev.Cr = cr;

                memory[2 * x].Y = y1;
                memory[2 * x].Cb = cb;
                memory[2 * x].Cr = cr;

                memory[2 * x + 1].Y = y2;
                memory[2 * x + 1].Cb = cb;
                memory[2 * x + 1].Cr = cr;
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
            //    finally
            //    {
            //        bitmap.UnlockBits(data);
            //    }

            //    bitmap.Save(folder + "0L2A8897-3.bmp");
            //}

            Assert.AreEqual(1728, y);   // scan lines
            Assert.AreEqual(2592, x);   // samples per line

            Assert.AreEqual(2 * x, slices[0] * slices[1] + slices[2]);
            CollectionAssert.AreEqual(new[] { (ushort)5, (ushort)864, (ushort)864 }, slices);
            slices[1] /= 2;
            slices[2] /= 2;

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
                        var offset = index - slice * slices[1] * y;
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
