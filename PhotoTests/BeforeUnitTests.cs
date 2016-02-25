using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg;
using PhotoLib.Tiff;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

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
                    var data = RawImage.ReadUInts16(binaryReader, imageFileEntry);
                    CollectionAssert.AreEqual(new[] { (ushort)1, (ushort)2960, (ushort)2960 }, data);

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

                    // DumpCompressedData(startOfImage);

                    // horz sampling == 1
                    startOfImage.ImageData.Reset();

                    var memory = new ushort[startOfFrame.Width, startOfFrame.ScanLines];
                    for (var slice = 0; slice < data[0]; slice++)
                    {
                        for (var line = 0; line < startOfFrame.ScanLines / 2; line++)
                            ProcessLine14211(slice, line, data[1] / 2, startOfImage, table0, table1, memory);
                    }
                    {
                        for (var line = 0; line < startOfFrame.ScanLines / 2; line++)
                            ProcessLine14211(data[0], line, data[2] / 2, startOfImage, table0, table1, memory);
                    }

                    MakeBitmap(memory, folder);
                }
            }
        }

        private static void MakeBitmap(ushort[,] memory, string folder)
        {
            var x = memory.GetLength(0);
            var y = memory.GetLength(1);

            using (var bitmap = new Bitmap(x, y))
            {
                for (var row = 0; row < y; row++)
                    for (var col = 0; col < x; col++)
                    {
                        if (row % 2 == 0 && col % 2 == 0)
                        {
                            var r = (byte)Math.Min((memory[col, row] >> 4), 255);
                            var color = Color.FromArgb(r, 0, 0);
                            bitmap.SetPixel(col, row, color);
                        }
                        else if ((row % 2 == 1 && col % 2 == 0) || (row % 2 == 0 && col % 2 == 1))
                        {
                            var g = (byte)Math.Min((memory[col, row] >> 5), 255);
                            var color = Color.FromArgb(0, g, 0);
                            bitmap.SetPixel(col, row, color);
                        }
                        else if (row % 2 == 1 && col % 2 == 1)
                        {
                            var b = (byte)Math.Min((memory[col, row] >> 4), 255);
                            var color = Color.FromArgb(0, 0, b);
                            bitmap.SetPixel(col, row, color);
                        }
                    }

                bitmap.Save(folder + "0L2A8897-3.bmp");
            }
        }

        private static void ProcessLine14211(int slice, int line, int width, StartOfImage startOfImage, HuffmanTable table0, HuffmanTable table1, ushort[,] memory)
        {
            var diff = new short[width * 2, 2];

            for (var x = 0; x < width; x++)
            {
                diff[2 * x, 0] = ProcessColor(startOfImage, table0);
                diff[2 * x + 1, 0] = ProcessColor(startOfImage, table1);
            }

            for (var x = 0; x < width; x++)
            {
                diff[2 * x, 1] = ProcessColor(startOfImage, table0);
                diff[2 * x + 1, 1] = ProcessColor(startOfImage, table1);
            }

            var pp = new[] { (ushort)0x047F, (ushort)0x047F };
            if (slice != 0)
            {
                pp[0] = memory[2 * slice * width - 2, 2 * line];
                pp[1] = memory[2 * slice * width - 1, 2 * line];
            }

            for (var y = 0; y < 2; y++)
            {
                var row = 2 * line + y;
                for (var x = 0; x < 2 * width; x++)
                {
                    var col = 2 * slice * width + x;
                    var p = (ushort)((pp[x % 2] + diff[x, y]) % 0x4000);
                    pp[x % 2] = p;
                    memory[col, row] = p;
                }
            }
        }

        [TestMethod]
        public void DumpImage3SRawTest()
        {
            const string Folder = @"D:\Users\Greg\Pictures\2013_10_14\";
            DumpImage3SRaw(Folder, "IMG_4194.CR2");
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
                    var data = RawImage.ReadUInts16(binaryReader, imageFileEntry);
                    CollectionAssert.AreEqual(new[] { (ushort)5, (ushort)864, (ushort)864 }, data);

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
                    Assert.AreEqual(3, startOfFrame.Components.Length); // sraw/sraw2

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

                    var memory = new DataBuf[startOfFrame.Width / 3, startOfFrame.ScanLines];
                    for (var slice = 0; slice < data[0]; slice++)
                    {
                        for (var line = 0; line < startOfFrame.ScanLines; line++)
                            ProcessLine15321(slice, line, data[1] / 4, startOfImage, table0, table1, memory);
                    }
                    {
                        for (var line = 0; line < startOfFrame.ScanLines; line++)
                            ProcessLine15321(data[0], line, data[2] / 4, startOfImage, table0, table1, memory);
                    }

                    // Assert.IsTrue(startOfImage.ImageData.EndOfFile);
                    // var hufCode = startOfImage.ImageData.GetValue(table0);

                    MakeBitmap2(memory, folder);
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

        static ushort prevY = (ushort)0x4000;
        static short prevCb = (short)0;
        static short prevCr = (short)0;

        private static void ProcessLine15321(int slice, int line, int width, StartOfImage startOfImage, HuffmanTable table0, HuffmanTable table1, DataBuf[,] memory)
        {
            var diff = new DiffBuf[width];
            for (var x = 0; x < width; x++)
            {
                diff[x].Y1 = ProcessColor(startOfImage, table0);
                diff[x].Y2 = ProcessColor(startOfImage, table0);
                diff[x].Cb = ProcessColor(startOfImage, table1);
                diff[x].Cr = ProcessColor(startOfImage, table1);
            }

            //if (slice != 0)
            //{
            //    prevY = memory[2 * slice * width - 3, line].Y;
            //    prevCb = memory[2 * slice * width - 3, line].Cb;
            //    prevCr = memory[2 * slice * width - 3, line].Cr;
            //}

            for (var x = 0; x < width; x++)
            {
                var p1 = (ushort)((prevY + diff[x].Y1) % 0x8000 );
                prevY = p1;

                var p2 = (ushort)((prevY + diff[x].Y2) % 0x8000);
                prevY = p2;

                var p3 = (short)((prevCb + diff[x].Cb) % 0x8000);
                prevCb = p3;

                var p4 = (short)((prevCr + diff[x].Cr) % 0x8000);
                prevCr = p4;

                var col = 2 * slice * width + 2 * x;
                memory[col, line].Y = p1;
                memory[col, line].Cb = p3;
                memory[col, line].Cr = p4;

                memory[col + 1, line].Y = p2;
                memory[col + 1, line].Cb = p3;
                memory[col + 1, line].Cr = p4;
            }
        }

        private static void MakeBitmap2(DataBuf[,] memory, string folder)
        {
            var x = memory.GetLength(0);
            var y = memory.GetLength(1);

            using (var bitmap = new Bitmap(x, y))
            {
                for (var row = 0; row < y; row++)
                    for (var col = 0; col < x; col++)
                    {
                        var r = memory[col, row].Y + 1.40200 * memory[col, row].Cr;
                        var g = memory[col, row].Y - 0.34414 * memory[col, row].Cb - 0.71414 * memory[col, row].Cr;
                        var b = memory[col, row].Y + 1.77200 * memory[col, row].Cb;
                        var color = Color.FromArgb((byte)((int)r >> 8), (byte)((int)g >> 8), (byte)((int)b >> 8));
                        bitmap.SetPixel(col, row, color);
                    }

                bitmap.Save(folder + "0L2A8897-3.bmp");
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
    }
}
