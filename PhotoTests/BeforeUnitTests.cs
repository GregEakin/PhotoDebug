namespace PhotoTests
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Jpeg;
    using PhotoLib.Tiff;
    using System.Drawing.Imaging;
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

                    DumpImage(binaryReader, folder, offset, width, height);
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
                    Assert.AreEqual(3950u, startOfFrame.ScanLines);
                    Assert.AreEqual(2960u, startOfFrame.SamplesPerLine);
                    Assert.AreEqual(2, startOfFrame.Components.Length);
                    Assert.AreEqual(5920, startOfFrame.Width);

                    // var rowBuf0 = new short[startOfFrame.SamplesPerLine * startOfFrame.Components.Length];
                    // var rowBuf1 = new short[startOfFrame.SamplesPerLine * startOfFrame.Components.Length];
                    // var predictor = new[] { (short)(1 << (startOfFrame.Precision - 1)), (short)(1 << (startOfFrame.Precision - 1)) };
                    Assert.AreEqual(2, startOfImage.HuffmanTable.Tables.Count);
                    var table0 = startOfImage.HuffmanTable.Tables[0x00];
                    var table1 = startOfImage.HuffmanTable.Tables[0x01];

                    Console.WriteLine(table0.ToString());
                    Console.WriteLine(table1.ToString());

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

                    // DumpCompressedData(startOfImage);

                    using (var image1 = new Bitmap(startOfFrame.Width / 2, startOfFrame.ScanLines / 2))
                    {
                        // horz sampling == 1
                        startOfImage.ImageData.Reset();

                        var prevA = (ushort)(1u << (startOfFrame.Precision - 1));
                        var prevB = (ushort)(1u << (startOfFrame.Precision - 1));
                        for (var slice = 0; slice < data[0]; slice++)
                        {
                            for (var line = 0; line < startOfFrame.ScanLines / 2; line++)
                                ProcessLine211(slice, line, data[1] / 2, startOfImage, table0, table1, ref prevA, ref prevB, image1);
                        }
                        {
                            for (var line = 0; line < startOfFrame.ScanLines / 2; line++)
                                ProcessLine211(data[0], line, data[2] / 2, startOfImage, table0, table1, ref prevA, ref prevB, image1);
                        }

                        image1.Save(folder + "0L2A8897-3.bmp");
                    }
                }
            }
        }

        private static void ProcessLine211(int slice, int line, int width, StartOfImage startOfImage, HuffmanTable table0, HuffmanTable table1, ref ushort prevA, ref ushort prevB, Bitmap image1)
        {
            var red = new ushort[width];
            var green1 = new ushort[width];
            var green2 = new ushort[width];
            var blue = new ushort[width];

            for (var x = 0; x < width; x++)
            {
                red[x] = ProcessColor(startOfImage, table0, ref prevA);
                green1[x] = ProcessColor(startOfImage, table0, ref prevB);
            }

            for (var x = 0; x < width; x++)
            {
                green2[x] = ProcessColor(startOfImage, table0, ref prevA);
                blue[x] = ProcessColor(startOfImage, table0, ref prevB);
            }

            for (var x = 0; x < width; x++)
            {
                var r = red[x];
                var g = (green1[x] + green2[x])/2;
                var b = blue[x];
                var color = Color.FromArgb((byte)((int)r >> 8), (byte)((int)g >> 8), (byte)((int)b >> 8));
                image1.SetPixel(slice * width + x, line, color);
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

                    using (var image1 = new Bitmap(startOfFrame.Width / 3, startOfFrame.ScanLines, PixelFormat.Format48bppRgb))
                    {
                        // horz sampling == 1
                        startOfImage.ImageData.Reset();

                        var prevY = (ushort)(1u << (startOfFrame.Precision - 1));
                        var prevCb = (short)0;
                        var prevCr = (short)0;
                        for (var slice = 0; slice < data[0]; slice++)
                        {
                            for (var line = 0; line < startOfFrame.ScanLines; line++)
                                ProcessLine321(slice, line, data[1] / 4, startOfImage, table0, table1, ref prevY, ref prevCb, ref prevCr, image1);
                        }
                        {
                            for (var line = 0; line < startOfFrame.ScanLines; line++)
                                ProcessLine321(data[0], line, data[2] / 4, startOfImage, table0, table1, ref prevY, ref prevCb, ref prevCr, image1);
                        }

                        // Assert.IsTrue(startOfImage.ImageData.EndOfFile);
                        // var hufCode = startOfImage.ImageData.GetValue(table0);

                        image1.Save(folder + "0L2A8897-3.bmp");
                    }
                }
            }
        }

        // ...F ...F ...0 ...0 ...E ...0 ...5 ...4 ...1 ...F ...F ...3 ...5 ...F ...A ...6 ...F ...4 ...F ...1 ...4 ...E ...D ...2 ...5 ...1 ...0 ...E ...2 ...9 ...D ...B ...F ...1 ...E ...A ...E ...C ...C ...7
        // 1111 1111 0000 0000 1110 0000 0101 0100 0001 1111 1111 0011 0101 1111 1010 0110 1111 0100 1111 0001 0100 1110 1101 0010 0101 0001 0000 1110 0010 1001 1101 1011 1111 0001 1110 1010 1110 1100 1100 0111
        // 

        private static void ProcessLine321(int slice, int line, int width, StartOfImage startOfImage, HuffmanTable table0, HuffmanTable table1, ref ushort prevY, ref short prevCb, ref short prevCr, Bitmap image1)
        {
            var y1 = new ushort[width];
            var y2 = new ushort[width];
            var cb = new short[width];
            var cr = new short[width];

            for (var x = 0; x < width; x++)
            {
                y1[x] = ProcessColor(startOfImage, table0, ref prevY);
                y2[x] = ProcessColor(startOfImage, table0, ref prevY);
                cb[x] = ProcessColor(startOfImage, table1, ref prevCb);
                cr[x] = ProcessColor(startOfImage, table1, ref prevCr);
            }

            for (var x = 0; x < width; x++)
            {
                var r = y1[x] + 1.40200 * cr[x];
                var g = y1[x] - 0.34414 * cb[x] - 0.71414 * cr[x];
                var b = y1[x] + 1.77200 * cb[x];
                var color = Color.FromArgb((byte)((int)r>>8), (byte)((int)g >> 8), (byte)((int)b >> 8));
                image1.SetPixel(2 * slice * width + 2 * x, line, color);

                r = y2[x] + 1.40200 * cr[x];
                g = y2[x] - 0.34414 * cb[x] - 0.71414 * cr[x];
                b = y2[x] + 1.77200 * cb[x];
                color = Color.FromArgb((byte)((int)r >> 8), (byte)((int)g >> 8), (byte)((int)b >> 8));
                image1.SetPixel(2 * slice * width + 2 * x + 1, line, color);
            }
        }

        private static short ProcessColor(StartOfImage startOfImage, HuffmanTable table, ref short prev)
        {
            var hufCode = startOfImage.ImageData.GetValue(table);
            var difCode = startOfImage.ImageData.GetValue(hufCode);
            var dif = HuffmanTable.DecodeDifBits(hufCode, difCode);
            prev = (short)(prev + dif);
            return prev;
        }

        private static ushort ProcessColor(StartOfImage startOfImage, HuffmanTable table, ref ushort prev)
        {
            var hufCode = startOfImage.ImageData.GetValue(table);
            var difCode = startOfImage.ImageData.GetValue(hufCode);
            var dif = HuffmanTable.DecodeDifBits(hufCode, difCode);

            if (dif < -2000 || dif > 2000)
                Console.WriteLine(dif);

            if (dif >= 0)
                prev += (ushort)dif;
            else
                prev -= (ushort)(-dif);
            return prev;
        }

        private static void DumpCompressedData(StartOfImage startOfImage)
        {
            for (var i = 0; i < 20; i++)
            {
                var p = startOfImage.ImageData.RawData[i];
                Console.Write(" ...{0} ...{1}",
                    ((p & 0xF0) >> 4).ToString("X1"),
                    ((p & 0x0F) >> 0).ToString("X1"));
            }
            Console.WriteLine();

            for (var i = 0; i < 20; i++)
            {
                var p = startOfImage.ImageData.RawData[i];
                Console.Write(" {0}{1}{2}{3} {4}{5}{6}{7}",
                    ((p & 0x80) >> 7).ToString("X1"),
                    ((p & 0x40) >> 6).ToString("X1"),
                    ((p & 0x20) >> 5).ToString("X1"),
                    ((p & 0x10) >> 4).ToString("X1"),
                    ((p & 0x08) >> 3).ToString("X1"),
                    ((p & 0x04) >> 2).ToString("X1"),
                    ((p & 0x02) >> 1).ToString("X1"),
                    ((p & 0x01) >> 0).ToString("X1"));
            }
            Console.WriteLine();
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

        private static void DumpStartOfScan(StartOfScan startOfScan)
        {
            Console.WriteLine("Ns: {0}", startOfScan.Components.Length);
            foreach (var scanComponent in startOfScan.Components)
            {
                Console.WriteLine("    Cs {0}: Td {1}, Ta {2}", scanComponent.Id, scanComponent.Dc, scanComponent.Ac);
            }
        }
    }
}
