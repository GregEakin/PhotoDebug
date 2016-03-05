using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg;
using PhotoLib.Jpeg.JpegTags;
using PhotoLib.Tiff;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class Image3SRawI
    {
        static double minY = double.MaxValue; static double maxY = double.MinValue;
        static double minCb = double.MaxValue; static double maxCb = double.MinValue;
        static double minCr = double.MaxValue; static double maxCr = double.MinValue;
        static int cc;
        private static double minR = double.MaxValue;
        private static double maxR = double.MinValue;
        private static double minG = double.MaxValue;
        private static double maxG = double.MinValue;
        private static double minB = double.MaxValue;
        private static double maxB = double.MinValue;

        struct DataBuf
        {
            public ushort Y;
            public short Cb;
            public short Cr;
        }

        struct DiffBuf
        {
            public short Y1;
            public short Y2;
            public short Cb;
            public short Cr;
        }

        [TestMethod]
        public void DumpImage3SRawTest()
        {
            // 2592 x 1728, Canon EOS 7D, 1/160 sec. f/1.8 85mm, SRAW   
            // const string Folder = @"D:\Users\Greg\Pictures\2013_10_14\";
            // DumpImage3SRaw(Folder, "IMG_4194.CR2");
            const string folder = @"D:\Users\Greg\Pictures\2016-02-26\";
            DumpImage3SRaw(folder, "003.CR2");
        }

        private static void DumpImage3SRaw(string folder, string file)
        {
            var fileName2 = folder + file;
            using (var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                // Image #3 is a raw image compressed in ITU-T81 lossless JPEG
                {
                    var image = rawImage.Directories.Skip(3).First();

                    var offset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(0x2D42DCu, offset);

                    var count = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(0x1501476u, count);

                    var imageFileEntry = image.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3);
                    var slices = RawImage.ReadUInts16(binaryReader, imageFileEntry);
                    CollectionAssert.AreEqual(new[] { (ushort)5, (ushort)864, (ushort)864 }, slices);

                    binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
                    var startOfImage = new StartOfImage(binaryReader, offset, count);

                    var startOfFrame = startOfImage.StartOfFrame;
                    Assert.AreEqual(1728u, startOfFrame.ScanLines);
                    Assert.AreEqual(2592u, startOfFrame.SamplesPerLine);
                    Assert.AreEqual(7776, startOfFrame.Width);

                    Assert.AreEqual(15, startOfFrame.Precision); // sraw/sraw2

                    // chrominance subsampling factors
                    Assert.AreEqual(3, startOfFrame.Components.Length); // sraw/sraw2

                    // J:a:b = 4:2:2, h/v = 2/1

                    // Component[1]: ID=0x01, Samp Fac=0x21 (Subsamp 1 x 1), Quant Tbl Sel=0x00 (Lum: Y)
                    Assert.AreEqual(1, startOfFrame.Components[0].ComponentId);
                    Assert.AreEqual(2, startOfFrame.Components[0].HFactor);         // SRAW
                    Assert.AreEqual(1, startOfFrame.Components[0].VFactor);
                    Assert.AreEqual(0, startOfFrame.Components[0].TableId);

                    // Component[2]: ID=0x02, Samp Fac=0x11 (Subsamp 2 x 1), Quant Tbl Sel=0x00 (Chrom: Cb)
                    Assert.AreEqual(2, startOfFrame.Components[1].ComponentId);
                    Assert.AreEqual(1, startOfFrame.Components[1].HFactor);
                    Assert.AreEqual(1, startOfFrame.Components[1].VFactor);
                    Assert.AreEqual(0, startOfFrame.Components[1].TableId);

                    // Component[3]: ID=0x03, Samp Fac=0x11 (Subsamp 2 x 1), Quant Tbl Sel=0x00 (Chrom: Cr)
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

                    // Spectral selection = 1 .. 0
                    Assert.AreEqual(1, startOfScan.Bb1);    // Start of spectral or predictor selection
                    Assert.AreEqual(0, startOfScan.Bb2);    // end of spectral selection

                    // Successive approximation = 0x00
                    Assert.AreEqual(0, startOfScan.Bb3);    // successive approximation bit positions
                    Assert.AreEqual(3, startOfScan.Components.Length);   // sraw/sraw2

                    // Component[1]: selector=0x01, table=0(DC),0(AC)
                    Assert.AreEqual(1, startOfScan.Components[0].Id);
                    Assert.AreEqual(0, startOfScan.Components[0].Dc);
                    Assert.AreEqual(0, startOfScan.Components[0].Ac);

                    // Component[2]: selector = 0x02, table = 1(DC),0(AC)
                    Assert.AreEqual(2, startOfScan.Components[1].Id);
                    Assert.AreEqual(1, startOfScan.Components[1].Dc);
                    Assert.AreEqual(0, startOfScan.Components[1].Ac);

                    // Component[3]: selector = 0x03, table = 1(DC),0(AC)
                    Assert.AreEqual(3, startOfScan.Components[2].Id);
                    Assert.AreEqual(1, startOfScan.Components[2].Dc);
                    Assert.AreEqual(0, startOfScan.Components[2].Ac);

                    // DumpCompressedData(startOfImage);

                    // horz sampling == 1
                    startOfImage.ImageData.Reset();

                    // startOfScan.Bb1 == 1, Ss = Algroithm P(A)
                    //     C, B, D
                    //     A, X
                    var bits = startOfFrame.Precision;                                                        // 15
                    var sraw = startOfFrame.Components[0].HFactor * startOfFrame.Components[0].VFactor - 1;   // 1
                    var colors = startOfFrame.Components.Length + sraw;                                       // 4

                    //var memory = new DataBuf[startOfFrame.ScanLines, startOfFrame.Width / 3];   // 1728 x 7776 / 3 (= 2592)
                    //LoadRaw(startOfImage, memory);

                    // var prev = new DataBuf[colors / 3];
                    //for (var line = 0; line < prev.Length; line++)
                    //    prev[line] = new DataBuf { Y = 0x3FFF, Cb = 0x000, Cr = 0x0000 };
                    //for (var line = 0; line < prev.Length; line++)
                    //    prev[line] = line == 0
                    //        ? new DataBuf { Y = 0x8000 - 3720, Cb = 0, Cr = 0 }
                    //        : new DataBuf { Y = 0x4000 - 3720, Cb = 0, Cr = 0 };
                    //for (var line = 0; line < prev.Length; line++)
                    //    prev[line] = new DataBuf { Y = (ushort)(1 << (bits - 1)), Cb = 0, Cr = 0 };    // 0x4000

                    var prev = new DataBuf { Y = 0x2000 };
                    var memory = new DataBuf[startOfFrame.ScanLines, startOfFrame.Width / 3];   // 1728 x 7776 / 3 (= 2592)
                    for (var slice = 0; slice < slices[0]; slice++)                              // 0..5
                    {
                        for (var line = 0; line < startOfFrame.ScanLines; line++)                 // 0..1728
                            ProcessLine15321(slice, line, slices[1], startOfImage, memory, prev);  // 864
                    }
                    {
                        for (var line = 0; line < startOfFrame.ScanLines; line++)                 // 0..1728
                            ProcessLine15321(slices[0], line, slices[2], startOfImage, memory, prev);  // 864   
                    }

                    Assert.AreEqual(8957952, cc);
                    Assert.AreEqual(3, startOfImage.ImageData.DistFromEnd);

                    //for (var slice = 0; slice < 2; slice++)                              // 0..5
                    //    for (var line = 0; line < startOfFrame.ScanLines; line++)
                    //        ProcessLine15321(slice, line, slices[1], startOfImage, memory, prev);  // 864

                    MakeBitmap(memory, folder);

                    Console.WriteLine();

                    Console.WriteLine("MinY = {0}, MaxY {1}", minY, maxY);
                    Console.WriteLine("MinCb = {0}, MaxCb {1}", minCb, maxCb);
                    Console.WriteLine("MinCr = {0}, MaxCr {1}", minCr, maxCr);

                    Console.WriteLine();

                    Console.WriteLine("MinR = {0}, MaxR {1}", minR, maxR);
                    Console.WriteLine("MinG = {0}, MaxG {1}", minG, maxG);
                    Console.WriteLine("MinB = {0}, MaxB {1}", minB, maxB);
                }
            }
        }

        private static void LoadRaw(StartOfImage startOfImage, DataBuf[,] memory)    // memroy[1728, 2592]
        {
            var startOfFrame = startOfImage.StartOfFrame;
            var sraw = startOfFrame.Components[0].HFactor * startOfFrame.Components[0].VFactor - 1;   // 1
            var colors = startOfFrame.Components.Length + sraw;                                       // 4

            var slices = new[] { (ushort)5, (ushort)864, (ushort)864 };
            Assert.AreEqual(memory.GetLength(1), (slices[0] * slices[1] + slices[2]) / 2);
            for (var slice = 0; slice <= slices[0]; slice++)
            {
                for (var line = 0; line < startOfFrame.ScanLines; line += (colors >> 1) - 1)     // 1728
                {
                    var rp = Ljpeg_row(startOfImage);  // startOfFrame.SamplesPerLine / 4  == 216
                    Assert.AreEqual(slice < slices[0] ? slices[1] : slices[2], rp.Length * 4);

                    var scol = slice * slices[1];
                    var ecol = scol + (slice < slices[0] ? slices[1] : slices[2]);
                    for (var col = scol / 2; col < ecol / 2; col++)
                    {
                        var jcol = (col - scol / 2) / 2;
                        memory[line, col].Y = (ushort)(jcol % 2 == 0 ? rp[jcol].Y1 : rp[jcol].Y2);
                        memory[line, col].Cb = rp[jcol].Cb;
                        memory[line, col].Cr = rp[jcol].Cr;
                    }
                }
            }
        }

        private static readonly int[] Vpred = { 0x2000, 0x2000, 0x0000, 0x0000 };
        private static readonly DataBuf Prev = new DataBuf { Y = 0x2000 };

        private static DiffBuf[] Ljpeg_row(StartOfImage startOfImage)
        {
            var table0 = startOfImage.HuffmanTable.Tables[0x00];
            var table1 = startOfImage.HuffmanTable.Tables[0x01];
            var tables = new[] { table0, table0, table1, table1 };

            var diffRow = new DiffBuf[216]; // 864 / 4 == 216

            var spred = 0;
            for (var col = 0; col < 216; col++)
            {
                //for (var c = 0; c < colors; c++)
                //{
                //    var diff = ProcessColor(startOfImage, tables[c]);
                //    var pred = 0;
                //    if (c <= 1 && (col > 0 || c > 0))
                //        pred = spred;
                //    else if (col > 0)
                //        pred = row[0][-jh->clrs];
                //    else
                //        pred = (Vpred[c] += diff) - diff;

                //    **row = pred + diff
                //    if (**row >> bits) error();
                //    if (c <= sraw) spred = **row;
                //    row[0]++
                //}


                // C == 0 --> Y1
                {
                    var diff = ProcessColor(startOfImage, tables[0]);
                    if (col == 0)
                    {
                        var pt = Vpred[0] += diff;
                        spred = pt;
                        diffRow[col].Y1 = (short)pt;
                    }
                    else
                    {
                        var pt = spred + diff;
                        spred = pt;
                        diffRow[col].Y1 = (short)pt;
                    }
                }

                // C == 1 --> Y2
                {
                    var pt = spred + ProcessColor(startOfImage, tables[1]);
                    spred = pt;
                    diffRow[col].Y2 = (short)pt;
                }

                // c == 2 --> Cb
                {
                    var diff = ProcessColor(startOfImage, tables[2]);
                    diffRow[col].Cb = col == 0
                        ? (short)(Vpred[2] += diff)
                        : (short)(diffRow[col - 1].Cb + diff);
                }

                // c == 3 --> Cr
                {
                    var diff = ProcessColor(startOfImage, tables[3]);
                    diffRow[col].Cr = col == 0
                        ? (short)(Vpred[3] += diff)
                        : (short)(diffRow[col - 1].Cr + diff);
                }

                cc += 4;
            }

            return diffRow;
        }

        // 4:2:2 chrominance subsampling pattern
        // 2x1 chroma subsampling
        private static void ProcessLine15321(int slice, int line, int samplesPerLine, StartOfImage startOfImage, DataBuf[,] memory, DataBuf prev)
        {
            var diff = ReadDiffBufs(samplesPerLine, startOfImage);

            var lineMinY1 = double.MaxValue;
            var lineMaxY1 = double.MinValue;
            var lineMinY2 = double.MaxValue;
            var lineMaxY2 = double.MinValue;

            for (var x = 0; x < diff.Length; x++)        // 216
            {
                var pp = prev;

                var y1 = (ushort)(pp.Y + diff[x].Y1);
                var y2 = (ushort)(pp.Y + diff[x].Y1 + diff[x].Y2);
                var cb = (short)(pp.Cb + diff[x].Cb);
                var cr = (short)(pp.Cr + diff[x].Cr);

                pp.Y = y2;
                pp.Cb = cb;
                pp.Cr = cr;

                var col = 2 * slice * diff.Length + 2 * x;
                memory[line, col].Y = y1;
                memory[line, col].Cb = cb;
                memory[line, col].Cr = cr;

                memory[line, col + 1].Y = y2;
                memory[line, col + 1].Cb = 0;
                memory[line, col + 1].Cr = 0;

                // debug: check the bounds of the running sum
                if (lineMinY1 > y1) lineMinY1 = y1;
                if (lineMaxY1 < y1) lineMaxY1 = y1;
                if (lineMinY2 > y2) lineMinY2 = y2;
                if (lineMaxY2 < y2) lineMaxY2 = y2;

                if (minY > y1) minY = y1;
                if (maxY < y1) maxY = y1;
                if (minY > y2) minY = y2;
                if (maxY < y2) maxY = y2;

                if (minCb > cb) minCb = cb;
                if (maxCb < cb) maxCb = cb;

                if (minCr > cr) minCr = cr;
                if (maxCr < cr) maxCr = cr;
            }

            if (slice >= 2 || (200 > line || line > 205))
                //if (slice > 0)
                return;

            Console.WriteLine("S: {0}, L: {1}, min y1 {2}, max y1 {3}, min y2 {4}, max y2 {5}"
                , slice, line, lineMinY1, lineMaxY1, lineMinY2, lineMaxY2);
        }

        private static DiffBuf[] ReadDiffBufs(int samplesPerLine, StartOfImage startOfImage)
        {
            var table0 = startOfImage.HuffmanTable.Tables[0x00];
            var table1 = startOfImage.HuffmanTable.Tables[0x01];

            var diff = new DiffBuf[samplesPerLine / 4]; // 864 / 4 == 216
            for (var x = 0; x < diff.Length; x++)
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

        private static void MakeBitmap(DataBuf[,] memory, string folder)
        {
            var y = memory.GetLength(0);
            var x = memory.GetLength(1);
            using (var bitmap = new Bitmap(x, y))
            {
                for (var row = 0; row < y; row++)
                    for (var col = 0; col < x; col++)
                    {
                        var red = memory[row, col].Y + 1.40200 * memory[row, col].Cr;
                        red = red / 128.0 + 0.5;
                        if (maxR < red) maxR = red;
                        if (minR > red) minR = red;
                        if (0.0 > red || red > 255.0) red = CheckValue(red, row, col);

                        var green = memory[row, col].Y - 0.34414 * memory[row, col].Cb - 0.71414 * memory[row, col].Cr;
                        green = green / 128.0 + 0.5;
                        if (maxG < green) maxG = green;
                        if (minG > green) minG = green;
                        if (0.0 > green || green > 255.0) green = CheckValue(green, row, col);

                        var blue = memory[row, col].Y + 1.77200 * memory[row, col].Cb;
                        blue = blue / 128.0 + 0.5;
                        if (maxB < blue) maxB = blue;
                        if (minB > blue) minB = blue;
                        if (0.0 > blue || blue > 255.0) blue = CheckValue(blue, row, col);

                        var color = Color.FromArgb((int)red, (int)green, (int)blue);
                        bitmap.SetPixel(col, row, color);
                    }

                bitmap.Save(folder + "0L2A8897-3.bmp");
            }
        }

        private static double CheckValue(double value, double row, double col)
        {
            if (value < 0.0)
            {
                // Console.WriteLine("LB: {0} at {1}, {2}", value, row, col);
                return 0.0;
            }
            if (value > 255.0)
            {
                // Console.WriteLine("UB: {0} at {1}, {2}", value, row, col);
                return 255.0;
            }

            return value;
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
