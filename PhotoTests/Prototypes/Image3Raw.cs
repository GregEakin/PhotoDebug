using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg;
using PhotoLib.Tiff;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class Image3Raw
    {
        static int _cc;

        [TestMethod]
        public void DumpImage3Test()
        {
            const string folder = @"D:\Users\Greg\Pictures\2016-02-21 Studio\";
            DumpImage3(folder, "Studio 015.CR2");
            //const string Folder = @"D:\Users\Greg\Pictures\2013_10_14\";
            // DumpImage3(Folder, "IMG_4194.CR2");
        }

        private static void DumpImage3(string folder, string file)
        {
            var fileName2 = folder + file;
            using (var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read))
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
                    Assert.AreEqual(3950u, startOfFrame.ScanLines);      // = 3840 + 110
                    Assert.AreEqual(2960u, startOfFrame.SamplesPerLine); // = 5920 / 2
                    Assert.AreEqual(2, startOfFrame.Components.Length);
                    Assert.AreEqual(5920, startOfFrame.Width);           // = 5760 + 160

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

                    // normal RAW (RGGB)
                    // RGRGRGRG...GBGBGBGB...
                    // R G R G R G 
                    // G B G B G B

                    var startOfScan = startOfImage.StartOfScan;
                    //DumpStartOfScan(startOfScan);
                    Assert.AreEqual(1, startOfScan.Bb1);    // Start of spectral or predictor selection
                    Assert.AreEqual(0, startOfScan.Bb2);    // end of spectral selection
                    Assert.AreEqual(0, startOfScan.Bb3);    // successive approximation bit positions
                    Assert.AreEqual(2, startOfScan.Components.Length);   // RGGB

                    Assert.AreEqual(1, startOfScan.Components[0].Id);
                    Assert.AreEqual(0, startOfScan.Components[0].Dc);
                    Assert.AreEqual(0, startOfScan.Components[0].Ac);

                    Assert.AreEqual(2, startOfScan.Components[1].Id);
                    Assert.AreEqual(1, startOfScan.Components[1].Dc);
                    Assert.AreEqual(0, startOfScan.Components[1].Ac);

                    // @@ White Balance

                    // @@ Black Substraction

                    // horz sampling == 1
                    startOfImage.ImageData.Reset();

                    var memory = new ushort[startOfFrame.ScanLines][];          // 3950 x 5920
                    var pp = new[] { (ushort)0x2000, (ushort)0x2000 };
                    for (var line = 0; line < startOfFrame.ScanLines; line++) // 0 .. 3950
                    {
                        var diff = ReadDiffRow(startOfImage);
                        var memory1 = ProcessDiff(diff, pp);
                        memory[line] = memory1;
                    }

                    Assert.AreEqual(23384000, _cc);
                    Assert.AreEqual(1, startOfImage.ImageData.DistFromEnd);
                    MakeBitmap(memory, folder, slices);
                }
            }
        }

        private static short[] ReadDiffRow(StartOfImage startOfImage)
        {
            var startOfFrame = startOfImage.StartOfFrame;
            int samplesPerLine = startOfFrame.SamplesPerLine;
            var table0 = startOfImage.HuffmanTable.Tables[0x00];
            var table1 = startOfImage.HuffmanTable.Tables[0x01];

            var diff = new short[2 * samplesPerLine];
            for (var x = 0; x < samplesPerLine; x++)
            {
                diff[2 * x + 0] = ProcessColor(startOfImage, table0);
                diff[2 * x + 1] = ProcessColor(startOfImage, table1);

                _cc += 2;
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

            //using (var bitmap = new Bitmap(x, y, PixelFormat.Format24bppRgb))
            //{
            //    var size = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            //    var data = bitmap.LockBits(size, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            //    for (var mrow = 0; mrow < y; mrow++)
            //    {
            //        var rdata = memory[mrow];
            //        for (var mcol = 0; mcol < x; mcol++)
            //        {
            //            var index = mrow * x + mcol;
            //            var slice = index / (sizes[1] * y);
            //            if (slice >= sizes[0])
            //                slice = sizes[0];
            //            index -= slice * (sizes[1] * y);
            //            var brow = index / sizes[slice < sizes[0] ? 1 : 2];
            //            var bcol = index % sizes[slice < sizes[0] ? 1 : 2] + slice * sizes[1];

            //            var val = rdata[mcol];
            //            PixelSet(scan0, brow, bcol, (short)val);
            //        }
            //    }

            //    bitmap.UnlockBits(data);
            //    bitmap.Save(folder + "0L2A8897-3.bmp");
            //}

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

                bitmap.Save(folder + "0L2A8897-3.bmp");
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
