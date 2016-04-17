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
    public class Image3Raw
    {
        static int _cc;

        [TestMethod]
        public void DumpImage3Test()
        {
            const string fileName = @"D:\Users\Greg\Pictures\2016-02-21 Studio\Studio 015.CR2";
            DumpImage3(fileName);
        }

        private static void DumpImage3(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // Image #3 is a raw image compressed in ITU-T81 lossless JPEG

                var image = rawImage.Directories.Skip(3).First();
                Assert.AreEqual(7, image.Entries.Length);

                var compression = image.Entries.Single(e => e.TagId == 0x0103 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(6u, compression); // 6 == old jpeg

                var offset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(0x2D42DCu, offset);

                var count = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(0x1501476u, count);

                var item3 = image.Entries.Single(e => e.TagId == 0xC5D8 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(0x1u, item3);

                var item4 = image.Entries.Single(e => e.TagId == 0xC5E0 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(0x1u, item4);

                // 0xC640 UShort 16-bit: [0x000119BE] (3): 1, 2960, 2960, 
                var imageFileEntry = image.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3);
                // Assert.AreEqual(0x000119BEu, imageFileEntry.ValuePointer);
                Assert.AreEqual(3u, imageFileEntry.NumberOfValue);
                var slices = RawImage.ReadUInts16(binaryReader, imageFileEntry);
                CollectionAssert.AreEqual(new ushort[] { 1, 2960, 2960 }, slices);

                var item6 = image.Entries.Single(e => e.TagId == 0xC6C5 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(0x1u, item6);

                binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, offset, count);

                var startOfFrame = startOfImage.StartOfFrame;
                Assert.AreEqual(3950u, startOfFrame.ScanLines); // = 3840 + 110
                Assert.AreEqual(2960u, startOfFrame.SamplesPerLine); // = 5920 / 2
                Assert.AreEqual(2, startOfFrame.Components.Length);
                Assert.AreEqual(5920, startOfFrame.Width); // = 5760 + 160

                Assert.AreEqual(2, startOfImage.HuffmanTable.Tables.Count);
                //var table0 = startOfImage.HuffmanTable.Tables[0x00];
                //var table1 = startOfImage.HuffmanTable.Tables[0x01];

                Assert.AreEqual(14, startOfFrame.Precision); // RGGB
                Assert.AreEqual(2, startOfFrame.Components.Length); // RGGB

                Assert.AreEqual(1, startOfFrame.Components[0].ComponentId);
                Assert.AreEqual(1, startOfFrame.Components[0].HFactor);
                Assert.AreEqual(1, startOfFrame.Components[0].VFactor);
                Assert.AreEqual(0, startOfFrame.Components[0].TableId);

                Assert.AreEqual(2, startOfFrame.Components[1].ComponentId);
                Assert.AreEqual(1, startOfFrame.Components[1].HFactor);
                Assert.AreEqual(1, startOfFrame.Components[1].VFactor);
                Assert.AreEqual(0, startOfFrame.Components[1].TableId);

                Assert.AreEqual(2, startOfFrame.Components.Sum(component => component.HFactor * component.VFactor));

                // normal RAW (RGGB)
                // RGRGRGRG...GBGBGBGB...
                // R G R G R G 
                // G B G B G B

                var startOfScan = startOfImage.StartOfScan;
                //DumpStartOfScan(startOfScan);
                Assert.AreEqual(1, startOfScan.Bb1); // Start of spectral or predictor selection
                Assert.AreEqual(0, startOfScan.Bb2); // end of spectral selection
                Assert.AreEqual(0, startOfScan.Bb3); // successive approximation bit positions
                Assert.AreEqual(2, startOfScan.Components.Length); // RGGB

                // startOfScan.Bb1 == 1, Ss = Algroithm A
                //     C, B, D
                //     A, X

                Assert.AreEqual(1, startOfScan.Components[0].Id);
                Assert.AreEqual(0, startOfScan.Components[0].Dc);
                Assert.AreEqual(0, startOfScan.Components[0].Ac); // in lossless, this value is always zero

                Assert.AreEqual(2, startOfScan.Components[1].Id);
                Assert.AreEqual(1, startOfScan.Components[1].Dc);
                Assert.AreEqual(0, startOfScan.Components[1].Ac);

                // @@ White Balance

                // @@ Black Substraction

                // horz sampling == 1
                startOfImage.ImageData.Reset();

                var memory = new ushort[startOfFrame.ScanLines][]; // 3950 x 5920
                var pp = new[] { (ushort)0x2000, (ushort)0x2000 };
                for (var line = 0; line < startOfFrame.ScanLines; line++) // 0 .. 3950
                {
                    var diff = ReadDiffRow(startOfImage);
                    var memory1 = ProcessDiff(diff, pp);
                    memory[line] = memory1;
                }

                Assert.AreEqual(23384000, _cc);
                Assert.AreEqual(1, startOfImage.ImageData.DistFromEnd);

                var outFile = Path.ChangeExtension(fileName, ".2.png");
                MakeBitmap(memory, outFile, slices);

                DumpData(memory, fileName);
            }
        }

        private static void DumpData(ushort[][] memory, string fileName)
        {
            var y = memory.GetLength(0);
            var x = memory[0].GetLength(0);

            var hist = new int[128];
            var min = ushort.MaxValue;
            var max = ushort.MinValue;

            for (var mrow = 0; mrow < y; mrow++)
            {
                var rdata = memory[mrow];
                for (var mcol = 0; mcol < x; mcol++)
                {
                    var data = rdata[mcol];
                    if (min > data) min = data;
                    if (max < data) max = data;
                    var index = data >> 7;
                    hist[index]++;
                }
            }

            Console.WriteLine("Min = 0x{0:X4}, Max = 0x{1:X4}", min, max);
            for (var i = 0; i < hist.Length; i++)
                Console.WriteLine("0x{0:X4}: {1,8}", i << 7, hist[i]);

            var name = Path.ChangeExtension(fileName, ".txt");
            using (var file = new StreamWriter(name))
            {
                file.WriteLine("Min = 0x{0:X4}, Max = 0x{1:X4}", min, max);
                for (var i = 0; i < hist.Length; i++)
                    file.WriteLine("0x{0:X4}: {1,8}", i << 7, hist[i]);
            }
        }

        private static short[] ReadDiffRow(StartOfImage startOfImage)
        {
            var startOfFrame = startOfImage.StartOfFrame;
            var width = startOfFrame.Width;

            var diff = new short[width];
            for (var x = 0; x < width / 2; x++)
            {
                diff[2 * x + 0] = startOfImage.ProcessColor(0x00);
                diff[2 * x + 1] = startOfImage.ProcessColor(0x01);

                _cc += 2;
            }

            return diff;
        }

        private static ushort[] ProcessDiff(short[] diff, ushort[] pp)
        {
            var memory = new ushort[diff.Length];
            var step = pp.Length;
            for (var x = 0; x < diff.Length; x++)   //  0..2960
            {
                if (x / step == 0)
                {
                    var pred = pp[x % step];
                    pp[x % step] += (ushort)diff[x];
                    memory[x] = (ushort)(pred + diff[x]);
                }
                else
                {
                    var pred = memory[x - step];
                    memory[x] = (ushort)(pred + diff[x]);
                }
            }

            return memory;
        }

        private static void MakeBitmap(ushort[][] memory, string folder, ushort[] slices)
        {
            var y = memory.GetLength(0);
            var x = memory[0].GetLength(0);

            using (var bitmap = new Bitmap(x, y, PixelFormat.Format48bppRgb))
            {
                var size = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var data = bitmap.LockBits(size, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                try
                {
                    Assert.AreEqual(6 * 5920, data.Stride);  // 6 bytes * 8 bits == 48 bits per pixel

                    for (var mrow = 0; mrow < y; mrow++)
                    {
                        var rdata = memory[mrow];
                        for (var mcol = 0; mcol < x; mcol++)
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

                            var scan0 = data.Scan0 + data.Stride * brow;
                            if (brow % 2 == 0 && bcol % 2 == 0)
                                Marshal.WriteInt16(scan0, 6 * bcol + 4, (short)val);
                            else if ((brow % 2 == 1 && bcol % 2 == 0) || (brow % 2 == 0 && bcol % 2 == 1))
                                Marshal.WriteInt16(scan0, 6 * bcol + 2, (short)val);
                            else if (brow % 2 == 1 && bcol % 2 == 1)
                                Marshal.WriteInt16(scan0, 6 * bcol + 0, (short)val);
                        }
                    }

                }
                finally
                {
                    bitmap.UnlockBits(data);
                }

                bitmap.Save(folder + "0L2A8897-3.png");
            }
        }

        private static void MakeBitmap16Bit(ushort[][] memory, string folder, ushort[] slices)
        {
            var y = memory.GetLength(0);
            var x = memory[0].GetLength(0);

            using (var bitmap = new Bitmap(x, y))
            {
                for (var mrow = 0; mrow < y; mrow++)
                {
                    var rdata = memory[mrow];
                    for (var mcol = 0; mcol < x; mcol++)
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

                bitmap.Save(folder + "0L2A8897-3b.bmp");
            }
        }

        private static void PixelSet(Bitmap bitmap, int row, int col, ushort val)
        {
            if (row % 2 == 0 && col % 2 == 0)
            {
                var r = (byte)Math.Min((val >> 4), 255);
                var color = Color.FromArgb(r, 0, 0);
                bitmap.SetPixel(col, row, color);
            }
            else if ((row % 2 == 1 && col % 2 == 0) || (row % 2 == 0 && col % 2 == 1))
            {
                var g = (byte)Math.Min((val >> 5), 255);
                var color = Color.FromArgb(0, g, 0);
                bitmap.SetPixel(col, row, color);
            }
            else if (row % 2 == 1 && col % 2 == 1)
            {
                var b = (byte)Math.Min((val >> 4), 255);
                var color = Color.FromArgb(0, 0, b);
                bitmap.SetPixel(col, row, color);
            }
        }
    }
}
