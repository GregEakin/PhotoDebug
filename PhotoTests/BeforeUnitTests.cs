using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg;
using PhotoLib.Tiff;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace PhotoTests
{
    [TestClass]
    public class BeforeUnitTests
    {
        [TestMethod]
        public void DumpImage0Test()
        {
            const string Folder = @"D:\Users\Greg\Pictures\2016-02-21 Studio\";
            DumpImage0(Folder, "Studio 015.CR2");
        }

        private static void DumpImage0(string folder, string file)
        {
            var fileName2 = folder + file;
            using (var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                // Images #0 and #1 are compressed in lossy (classic) JPEG
                {
                    var image = rawImage.Directories.First();
                    var offset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(90660u, offset);
                    var length = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(1138871u, length);
                    var orientation = image.Entries.Single(e => e.TagId == 0x0112 && e.TagType == 3).ValuePointer;
                    // Assert.AreEqual(1u, orientation);

                    DumpImage(binaryReader, folder + "0L2A8897-0.JPG", offset, length);
                }
            }
        }

        [TestMethod]
        public void DumpImage1Test()
        {
            const string Folder = @"D:\Users\Greg\Pictures\2016-02-21 Studio\";
            DumpImage1(Folder, "Studio 015.CR2");
        }

        private static void DumpImage1(string folder, string file)
        {
            var fileName2 = folder + file;
            using (var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                // Images #0 and #1 are compressed in lossy (classic) JPEG
                {
                    var image = rawImage.Directories.Skip(1).First();
                    var offset = image.Entries.Single(e => e.TagId == 0x0201 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(80324u, offset);
                    var length = image.Entries.Single(e => e.TagId == 0x0202 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(10334u, length);

                    DumpImage(binaryReader, folder + "0L2A8897-1.JPG", offset, length);
                }
            }
        }

        [TestMethod]
        public void DumpImage2Test()
        {
            const string Folder = @"D:\Users\Greg\Pictures\2016-02-21 Studio\";
            DumpImage2(Folder, "Studio 015.CR2");
        }

        private static void DumpImage2(string folder, string file)
        {
            var fileName2 = folder + file;
            using (var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                // Image #2 is RGB, 16 bits per color, little endian.
                // Length = 3 * 16 bits * nb pixels
                {
                    var image = rawImage.Directories.Skip(2).First();
                    var width = image.Entries.Single(e => e.TagId == 0x0100 && e.TagType == 3).ValuePointer;
                    Assert.AreEqual(592u, width);
                    var height = image.Entries.Single(e => e.TagId == 0x0101 && e.TagType == 3).ValuePointer;
                    Assert.AreEqual(395u, height);
                    var offset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(1229532u, offset);
                    var samples = image.Entries.Single(e => e.TagId == 0x0115 && e.TagType == 3).ValuePointer;
                    Assert.AreEqual(3u, samples);
                    var rows = image.Entries.Single(e => e.TagId == 0x0116 && e.TagType == 3).ValuePointer;
                    Assert.AreEqual(395u, rows);
                    var count = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                    Assert.AreEqual(1403040u, count);

                    Assert.AreEqual(count, width * height * samples * 2);

                    DumpImage(binaryReader, folder + file + ".RGB", offset, width, height);
                }
            }
        }

        [TestMethod]
        public void DumpImage3Test()
        {
            const string Folder = @"D:\Users\Greg\Pictures\2016-02-21 Studio\";
            DumpImage3(Folder, "Studio 015.CR2");
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
                    var compression = image.Entries.Single(e => e.TagId == 0x0103 && e.TagType == 3).ValuePointer;
                    Assert.AreEqual(6u, compression);
                    var offset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(0x2D42DCu, offset);
                    var count = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(0x1501476u, count);

                    // 0xC640 UShort 16-bit: [0x000119BE] (3): 1, 2960, 2960, 
                    var imageFileEntry = image.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3);
                    var slices = imageFileEntry.ValuePointer;
                    // Assert.AreEqual(0x000119BEu, slices);
                    var number = imageFileEntry.NumberOfValue;
                    Assert.AreEqual(3u, number);
                    var sizes = RawImage.ReadUInts16(binaryReader, imageFileEntry);
                    CollectionAssert.AreEqual(new[] { (ushort)1, (ushort)2960, (ushort)2960 }, sizes);

                    binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
                    var startOfImage = new StartOfImage(binaryReader, offset, count); // { ImageData = new ImageData(binaryReader, count) };

                    var startOfFrame = startOfImage.StartOfFrame;
                    Assert.AreEqual(3950u, startOfFrame.ScanLines);      // = 3840 + 110
                    Assert.AreEqual(2960u, startOfFrame.SamplesPerLine); // = 5920 / 2
                    Assert.AreEqual(2, startOfFrame.Components.Length);
                    Assert.AreEqual(5920, startOfFrame.Width);           // = 5760 + 160

                    // var rowBuf0 = new short[startOfFrame.SamplesPerLine * startOfFrame.Components.Length];
                    // var rowBuf1 = new short[startOfFrame.SamplesPerLine * startOfFrame.Components.Length];
                    // var predictor = new[] { (short)(1 << (startOfFrame.Precision - 1)), (short)(1 << (startOfFrame.Precision - 1)) };
                    Assert.AreEqual(2, startOfImage.HuffmanTable.Tables.Count);
                    var table0 = startOfImage.HuffmanTable.Tables[0x00];
                    var table1 = startOfImage.HuffmanTable.Tables[0x01];

                    //Console.WriteLine(table0.ToString());
                    //Console.WriteLine(table1.ToString());

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
                    for (var line = 0; line < startOfFrame.ScanLines; line++)   // 0 .. 3950
                        memory[line] = ProcessLine14211(line, startOfFrame.SamplesPerLine, startOfImage, pp, table0, table1); // 2960
                    Assert.AreEqual(1, startOfImage.ImageData.DistFromEnd);
                    MakeBitmap(memory, folder, sizes);
                }
            }
        }

        private static ushort[] ProcessLine14211(int row, int width, StartOfImage startOfImage, ushort[] pp, HuffmanTable table0, HuffmanTable table1)
        {
            var diff = new short[2 * width];
            for (var x = 0; x < width; x++)
            {
                diff[2 * x + 0] = ProcessColor(startOfImage, table0);
                diff[2 * x + 1] = ProcessColor(startOfImage, table1);
            }

            var memory = new ushort[2 * width];
            for (var x = 0; x < width; x++)     //  0..2960
                for (var c = 0; c < 2; c++)     //  0..2
                {
                    if (x == 0)
                    {
                        var pred = pp[c];
                        pp[c] += (ushort)diff[2 * x + c];
                        memory[2 * x + c] = (ushort)(pred + diff[2 * x + c]);
                    }
                    else
                    {
                        var pred = memory[2 * x + c - 2];
                        memory[2 * x + c] = (ushort)(pred + diff[2 * x + c]);
                    }
                }

            return memory;
        }

        private static void MakeBitmap(ushort[][] memory, string folder, ushort[] sizes)
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
            //            var scan0 = data.Scan0 + data.Stride * brow;

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
                        var slice = index / (sizes[1] * y);
                        if (slice >= sizes[0])
                            slice = sizes[0];
                        index -= slice * (sizes[1] * y);
                        var brow = index / sizes[slice < sizes[0] ? 1 : 2];
                        var bcol = index % sizes[slice < sizes[0] ? 1 : 2] + slice * sizes[1];

                        var val = rdata[mcol];
                        PixelSet(bitmap, brow, bcol, val);
                    }
                }

                bitmap.Save(folder + "0L2A8897-3.bmp");
            }
        }

        [TestMethod]
        public void CheckSlices()
        {
            var sizes = new[] { 2, 5, 7 };
            const int y = 6;
            const int x = 17;

            Assert.AreEqual(x, sizes[0] * sizes[1] + sizes[2]);

            for (var mrow = 0; mrow < y; mrow++)
            {
                for (var mcol = 0; mcol < x; mcol++)
                {
                    var index = mrow * x + mcol;
                    var slice = index / (sizes[1] * y);
                    if (slice >= sizes[0])
                        slice = sizes[0];
                    index -= slice * (sizes[1] * y);
                    var brow = index / sizes[slice < sizes[0] ? 1 : 2];
                    var bcol = index % sizes[slice < sizes[0] ? 1 : 2] + slice * sizes[1];
                }
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

        private static void PixelSet(IntPtr scan0, int row, int col, short value)
        {
            if (row % 2 == 0 && col % 2 == 0)
            {
                Marshal.WriteInt16(scan0, 3 * col + 0, 0);
                Marshal.WriteInt16(scan0, 3 * col + 1, 0);
                Marshal.WriteInt16(scan0, 3 * col + 2, value);
            }
            else if ((row % 2 == 1 && col % 2 == 0) || (row % 2 == 0 && col % 2 == 1))
            {
                Marshal.WriteInt16(scan0, 3 * col + 0, 0);
                Marshal.WriteInt16(scan0, 3 * col + 1, value);
                Marshal.WriteInt16(scan0, 3 * col + 2, 0);
            }
            else if (row % 2 == 1 && col % 2 == 1)
            {
                Marshal.WriteInt16(scan0, 3 * col + 0, value);
                Marshal.WriteInt16(scan0, 3 * col + 1, 0);
                Marshal.WriteInt16(scan0, 3 * col + 2, 0);
            }
        }

        [TestMethod]
        public void DumpImage3SRawTest()
        {
            // 2592 x 1728, Canon EOS 7D, 1/160 sec. f/1.8 85mm, SRAW   
            // const string Folder = @"D:\Users\Greg\Pictures\2013_10_14\";
            // DumpImage3SRaw(Folder, "IMG_4194.CR2");
            const string Folder = @"D:\Users\Greg\Pictures\2016-02-26\";
            DumpImage3SRaw(Folder, "003.CR2");
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
                    var compression = image.Entries.Single(e => e.TagId == 0x0103 && e.TagType == 3).ValuePointer;
                    Assert.AreEqual(6u, compression);
                    var offset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(0x2D42DCu, offset);
                    var count = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(0x1501476u, count);

                    // 0xC640 UShort 16-bit: [0x000119BE] (3): 1, 2960, 2960, 
                    var imageFileEntry = image.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3);
                    var slices = imageFileEntry.ValuePointer;
                    // Assert.AreEqual(0x000119BEu, slices);
                    var number = imageFileEntry.NumberOfValue;
                    Assert.AreEqual(3u, number);
                    var sizes = RawImage.ReadUInts16(binaryReader, imageFileEntry);
                    CollectionAssert.AreEqual(new[] { (ushort)5, (ushort)864, (ushort)864 }, sizes);

                    binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
                    var startOfImage = new StartOfImage(binaryReader, offset, count); // { ImageData = new ImageData(binaryReader, count) };

                    var startOfFrame = startOfImage.StartOfFrame;
                    Assert.AreEqual(1728u, startOfFrame.ScanLines);
                    Assert.AreEqual(2592u, startOfFrame.SamplesPerLine);
                    Assert.AreEqual(7776, startOfFrame.Width);

                    // var rowBuf0 = new short[startOfFrame.SamplesPerLine * startOfFrame.Components.Length];
                    // var rowBuf1 = new short[startOfFrame.SamplesPerLine * startOfFrame.Components.Length];
                    // var predictor = new[] { (short)(1 << (startOfFrame.Precision - 1)), (short)(1 << (startOfFrame.Precision - 1)) };
                    Assert.AreEqual(2, startOfImage.HuffmanTable.Tables.Count);
                    var table0 = startOfImage.HuffmanTable.Tables[0x00];
                    var table1 = startOfImage.HuffmanTable.Tables[0x01];

                    Console.WriteLine(table0.ToString());
                    Console.WriteLine(table1.ToString());

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

                    var memory = new DataBuf[startOfFrame.ScanLines, startOfFrame.Width / 3];
                    for (var slice = 0; slice < sizes[0]; slice++)
                    {
                        for (var line = 0; line < startOfFrame.ScanLines; line++)
                            ProcessLine15321(slice, startOfFrame.ScanLines, line, sizes[1] / 4, startOfImage, table0, table1, memory);
                    }
                    {
                        for (var line = 0; line < startOfFrame.ScanLines; line++)
                            ProcessLine15321(sizes[0], startOfFrame.ScanLines, line, sizes[2] / 4, startOfImage, table0, table1, memory);
                    }

                    Assert.AreEqual(3, startOfImage.ImageData.DistFromEnd);

                    MakeBitmap(memory, folder);
                }
            }
        }

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

        // ...F ...F ...0 ...0 ...E ...0 ...5 ...4 ...1 ...F ...F ...3 ...5 ...F ...A ...6 ...F ...4 ...F ...1 ...4 ...E ...D ...2 ...5 ...1 ...0 ...E ...2 ...9 ...D ...B ...F ...1 ...E ...A ...E ...C ...C ...7
        // 1111 1111 0000 0000 1110 0000 0101 0100 0001 1111 1111 0011 0101 1111 1010 0110 1111 0100 1111 0001 0100 1110 1101 0010 0101 0001 0000 1110 0010 1001 1101 1011 1111 0001 1110 1010 1110 1100 1100 0111
        // 

        static DataBuf[] Prev;
        static double minY = double.MaxValue; static double maxY = double.MinValue;
        static double minCb = double.MaxValue; static double maxCb = double.MinValue;
        static double minCr = double.MaxValue; static double maxCr = double.MinValue;

        // This process two lines at a time
        private static void ProcessLine15321(int slice, int lines, int line, int width, StartOfImage startOfImage, HuffmanTable table0, HuffmanTable table1, DataBuf[,] memory)
        {
            var diff = new DiffBuf[width];
            for (var x = 0; x < width; x++)
            {
                // YUYV
                diff[x].Y1 = ProcessColor(startOfImage, table0);
                diff[x].Y2 = ProcessColor(startOfImage, table0);
                diff[x].Cb = ProcessColor(startOfImage, table1);
                diff[x].Cr = ProcessColor(startOfImage, table1);
            }

            // Debug: Dump the diff data.
            //{
            //    var y1 = 0.0; var minY = double.MaxValue; var maxY = double.MinValue;
            //    var y2 = 0.0;
            //    var cb = 0.0; var minCb = double.MaxValue; var maxCb = double.MinValue;
            //    var cr = 0.0; var minCr = double.MaxValue; var maxCr = double.MinValue;

            //    for (var x = 0; x < width; x++)
            //    {
            //        y1 += diff[x].Y1;
            //        y2 += diff[x].Y2;
            //        if (minY > y1 + y2) minY = y1 + y2;
            //        if (maxY < y1 + y2) maxY = y1 + y2;

            //        cb += diff[x].Cb;
            //        if (minCb > cb) minCb = cb;
            //        if (maxCb < cb) maxCb = cb;

            //        cr += diff[x].Cr;
            //        if (minCr > cb) minCr = cr;
            //        if (maxCr < cb) maxCr = cr;
            //    }

            //    // if (line == 1000 || line == 0 || line == 1 || line == 999)
            //    {
            //        Console.Write("{0}, {1}, {2}, {3}, {4}, {5},  ", slice, line, y1, y2, cb, cr);
            //        Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5},  ", minY, maxY, minCb, maxCb, minCr, maxCb);
            //    }
            //}

            if (slice == 0)
            {
                if (line == 0)
                {
                    Prev = new DataBuf[lines];
                    Prev[0] = new DataBuf { Y = 0x8000 - 3720, Cb = 0, Cr = 0 };
                }
                else
                    Prev[line] = new DataBuf { Y = 0x4000 - 3720, Cb = 0, Cr = 0 };
            }

            for (var x = 0; x < width; x++)
            {
                var y1 = (ushort)((Prev[line].Y + diff[x].Y1));
                var y2 = (ushort)((Prev[line].Y + diff[x].Y1 + diff[x].Y2));
                var cb = (short)((Prev[line].Cb + diff[x].Cb));
                var cr = (short)((Prev[line].Cr + diff[x].Cr));

                Prev[line].Y = y2;
                Prev[line].Cb = cb;
                Prev[line].Cr = cr;

                var col = 2 * slice * width + 2 * x;
                memory[line, col].Y = y1;
                memory[line, col].Cb = cb;
                memory[line, col].Cr = cr;

                memory[line, col + 1].Y = y2;
                memory[line, col + 1].Cb = cb;
                memory[line, col + 1].Cr = cr;

                // debug: check the bounds of the running sum
                if (minY > Prev[line].Y) minY = Prev[line].Y;
                if (maxY < Prev[line].Y) maxY = Prev[line].Y;

                if (minCb > Prev[line].Cb) minCb = Prev[line].Cb;
                if (maxCb < Prev[line].Cb) maxCb = Prev[line].Cb;

                if (minCr > Prev[line].Cr) minCr = Prev[line].Cr;
                if (maxCr < Prev[line].Cr) maxCr = Prev[line].Cr;
            }

            // debug: report bounds of the running sum
            if (slice == 5)
            {
                Console.WriteLine("{0}, {1}, {2}, {3}", line, Prev[line].Y, Prev[line].Cb, Prev[line].Cr);

                if (line == lines - 1)
                {
                    Console.WriteLine();
                    Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", minY, maxY, minCb, maxCb, minCr, maxCr);
                }
            }
        }
        private static short check(double value)
        {
            if (value > 0x7FFF)
                return (short)0x7FFF;
            if (value < 0x0000)
                return (short)0x0000;
            return (short)value;
        }

        private static void MakeBitmap(DataBuf[,] memory, string folder)
        {
            var y = memory.GetLength(0);
            var x = memory.GetLength(1);

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

            using (var bitmap = new Bitmap(x, y))
            {
                for (var row = 0; row < y; row++)
                    for (var col = 0; col < x; col++)
                    {
                        //var r = memory[row, col].Y + 1.40200 * memory[row, col].Cr;
                        //var g = memory[row, col].Y - 0.34414 * memory[row, col].Cb - 0.71414 * memory[col, row].Cr;
                        //var b = memory[row, col].Y + 1.77200 * memory[row, col].Cb;
                        var c = (memory[row, col].Y) >> 7;
                        var color = Color.FromArgb((byte)c, (byte)c, (byte)c);
                        bitmap.SetPixel(col, row, color);
                    }

                bitmap.Save(folder + "0L2A8897-3.bmp");
            }
        }

        internal static Image imageFromArray(byte[] array)
        {
            var width = 1472;
            var height = array.Length / width / 2;
            using (var b = new Bitmap(width, height, PixelFormat.Format16bppGrayScale))
            {
                var size = new Rectangle(0, 0, width, height);
                var bmData = b.LockBits(size, ImageLockMode.ReadWrite, PixelFormat.Format16bppGrayScale);
                var stride = bmData.Stride;
                var scan0 = bmData.Scan0;
                Marshal.Copy(array, 0, scan0, array.Length);
                b.UnlockBits(bmData);
                b.RotateFlip(RotateFlipType.Rotate90FlipX);
                return b;
            }
        }

        internal static void Bitmaps(byte[] byteArray, int offsetInBytes, short shortValue)
        {
            var width = 10;
            var height = 10;
            using (var bitmap = new Bitmap(width, height, PixelFormat.Format16bppGrayScale))
            {
                var size = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

                // Lock the unmanaged bits for efficient writing.
                var data = bitmap.LockBits(size, ImageLockMode.ReadWrite, bitmap.PixelFormat);

                // Bulk copy pixel data from a byte array:
                Marshal.Copy(byteArray, 0, data.Scan0, byteArray.Length);

                // Or, for one pixel at a time:
                Marshal.WriteInt16(data.Scan0, offsetInBytes, shortValue);

                // When finished, unlock the unmanaged bits
                bitmap.UnlockBits(data);
            }
        }

        private static short ProcessColor(StartOfImage startOfImage, HuffmanTable table)
        {
            var hufBits = startOfImage.ImageData.GetValue(table);
            var difCode = startOfImage.ImageData.GetValue(hufBits);
            var difValue = HuffmanTable.DecodeDifBits(hufBits, difCode);
            return difValue;
        }

        private static void DumpImage(BinaryReader binaryReader, string folder, uint offset, uint width, uint height)
        {
            using (var image1 = new Bitmap((int)width, (int)height)) // , PixelFormat.Format48bppRgb))
            {
                binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                    {
                        var r = binaryReader.ReadUInt16();
                        var g = binaryReader.ReadUInt16();
                        var b = binaryReader.ReadUInt16();
                        // var color = Color.FromArgb(r, g, b);
                        var color = Color.FromArgb((byte)(r >> 5), (byte)(g >> 5), (byte)(b >> 5));
                        image1.SetPixel(x, y, color);
                    }

                image1.Save(folder + "0L2A8897-2.bmp");
            }
        }

        private static void DumpImage(BinaryReader binaryReader, string filename, uint offset, uint length)
        {
            using (var fout = File.Open(filename, FileMode.Create, FileAccess.Write))
            {
                binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

                //Create a byte array to act as a buffer
                var buffer = new byte[32];
                for (var i = 0; i < length;)
                {
                    //Read from the source file
                    //The Read method returns the number of bytes read
                    int n = binaryReader.Read(buffer, 0, buffer.Length);

                    //Write the contents of the buffer to the destination file
                    fout.Write(buffer, 0, n);

                    i += n;
                }

                //Flush the contents of the buffer to the file
                fout.Flush();
            }
        }

        public void FastStuff(string filename)
        {
            byte[] data;
            data = File.ReadAllBytes(filename);
        }
    }
}
