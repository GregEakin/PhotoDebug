namespace PhotoTests
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Jpeg;
    using PhotoLib.Tiff;

    [TestClass]
    public class BeforeUnitTests
    {
        #region Public Methods and Operators

        private static void DumpPixelDebug(int col, int row, short[] rowBuf0, short[] rowBuf1)
        {
            const int X = 122; // 2116;
            const int Y = 40; // 1416 / 2; // (3950 - 900) / 2;

            var q = row - Y;
            if (q < 0 || q >= 5 || col > 0)
            {
                return;
            }

            for (var p = 0; p < 5; p++)
            {
                var red = rowBuf0[2 * p + X + 0] - 2047;
                var green = rowBuf0[2 * p + X + 1] - 2047;
                var green2 = rowBuf1[2 * p + X + 0] - 2047;
                var blue = rowBuf1[2 * p + X + 1] - 2047;

                Console.WriteLine("{4}, {5}: {0}, {1}, {2}, {3}", red, green, blue, green2, p + 1, q + 1);
            }
        }

        private static void DumpPixel(int col, int row, short[] rowBuf0, short[] rowBuf1, Bitmap image1)
        {
            DumpPixelDebug(col, row, rowBuf0, rowBuf1);

            const int X = 0; // 122; // 2116;
            const int Y = 0; // 40; // 1416 / 2; // (3950 - 900) / 2;

            var q = row - Y;
            if (2 * q + 1 >= image1.Height)
            {
                return;
            }

            for (var p = 0; p < rowBuf0.Length / 2 && 2 * p + X + col + 1 < image1.Width; p++)
            {
                var red = (rowBuf0[2 * p + X + 0] - 2047) >> 5;
                if (red < 0)
                {
                    red = 0;
                }
                else if (red > 0xFF)
                {
                    red = 0xFF;
                }

                var green = ((rowBuf0[2 * p + X + 1] - 2047) >> 6) + ((rowBuf1[2 * p + X + 0] - 2047) >> 6);
                if (green < 0)
                {
                    green = 0;
                }
                else if (green > 0xFF)
                {
                    green = 0xfF;
                }

                var blue = (rowBuf1[2 * p + X + 1] - 2047) >> 5;
                if (blue < 0)
                {
                    blue = 0;
                }
                else if (blue > 0xFF)
                {
                    blue = 0xFF;
                }

                var color = Color.FromArgb(red, green, blue);
                image1.SetPixel(2 * p + 0 + col, 2 * q, color);
                image1.SetPixel(2 * p + 1 + col, 2 * q, color);
                image1.SetPixel(2 * p + 0 + col, 2 * q + 1, color);
                image1.SetPixel(2 * p + 1 + col, 2 * q + 1, color);
            }
        }

        [TestMethod]
        public void TestMethodC5M3()
        {
            // const string Folder = @"D:\Users\Greg\Pictures\2013_10_14\";
            // DumpIFDs(Folder, "0L2A8897.CR2");
            //// DumpBitmap(Folder, "0L2A8897.CR2", "0L2A8897 Before.BMP");

            const string Folder = @"D:\Users\Greg\Pictures\2016-02-21 Studio\";
            DumpIFDs(Folder, "Studio 016.CR2");
        }

        private static void DumpIFDs(string folder, string file)
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

                    // DumpImage(binaryReader, folder + "0L2A8897-0.JPG", offset, length);
                }

                // Images #0 and #1 are compressed in lossy (classic) JPEG
                {
                    var image = rawImage.Directories.Skip(1).First();
                    var offset = image.Entries.Single(e => e.TagId == 0x0201 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(80324u, offset);
                    var length = image.Entries.Single(e => e.TagId == 0x0202 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(10334u, length);

                    // DumpImage(binaryReader, folder + "0L2A8897-1.JPG", offset, length);
                }

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
                }

                // Image #3 is a raw image compressed in ITU-T81 lossless JPEG
                {
                    var image = rawImage.Directories.Skip(3).First();
                    var compression = image.Entries.Single(e => e.TagId == 0x0103 && e.TagType == 3).ValuePointer;
                    var offset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(0x2D42DCu, offset);
                    var count = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(0x1501476u, count);

                    // 0xC640 UShort 16-bit: [0x000119BE] (3): 1, 2960, 2960, 
                    var imageFileEntry = image.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3);
                    var slices = imageFileEntry.ValuePointer;
                    Assert.AreEqual(0x000119BEu, slices);
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
                    DumpStartOfScan(startOfScan);
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

                    using (var image1 = new Bitmap(startOfFrame.Width / 2, startOfFrame.ScanLines / 2))
                    {
                        //      horz sampling == 1
                        startOfImage.ImageData.Reset();

                        DumpCompressedData(startOfImage);

                        var prevR = 1 << (startOfFrame.Precision - 1);
                        var prevG = 1 << (startOfFrame.Precision - 1);
                        var prevB = 1 << (startOfFrame.Precision - 1);
                        for (var slice = 0; slice < data[0]; slice++)
                        {
                            for (var line = 0; line < startOfFrame.ScanLines / 2; line++)
                                ProcessLine(slice, line, data[1] / 2, startOfImage, table0, ref prevR, ref prevG, ref prevB, image1);
                        }
                        {
                            for (var line = 0; line < startOfFrame.ScanLines / 2; line++)
                                ProcessLine(data[0], line, data[2] / 2, startOfImage, table0, ref prevR, ref prevG, ref prevB, image1);
                        }

                        image1.Save(folder + "oops.bmp");
                    }

                    Console.WriteLine("R {0} G {1} B {2}", maxR, maxG, maxB);
                }
            }
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

        //   F    F    0    0    2    1    4    7    F C    7    5    1    6    6    7    6    A B    C    3    A    3    5    8    8    5    A    5    B    0    7    0    9    6    2    9    6    E    3 
        //1111 1111 0000 0000 0010 0001 0100 0111 1111 1100 0111 0101 0001 0110 0110 0111 0110 1010 1011 1100 0011 1010 0011 0101 1000 1000 0101 1010 0101 1011 0000 0111 0000 1001 0110 0010 1001 0110 1110 0011 
        //1111 1111           1222 2222 2222 2233 3333 3334 4444 4444 4444 5556 6666 666

        static long maxR = 0L;
        static long maxG = 0L;
        static long maxB = 0L;

        private static void ProcessLine(int slice, int line, int width, StartOfImage startOfImage, HuffmanTable table0, ref int prevR, ref int prevG, ref int prevB, Bitmap image1)
        {
            const double divisor = 12726122.0/512.0;

            var red = new int[width];
            var green1 = new int[width];
            var green2 = new int[width];
            var blue = new int[width];

            for (var x = 0; x < width; x++)
            {
                red[x] = ProcessColor(startOfImage, table0, ref prevR);
                green1[x] = ProcessColor(startOfImage, table0, ref prevG);
            }

            for (var x = 0; x < width; x++)
            {
                green2[x] = ProcessColor(startOfImage, table0, ref prevG);
                blue[x] = ProcessColor(startOfImage, table0, ref prevB);
            }

            for (var x = 0; x < width; x++)
            {
                if (maxR < red[x]) maxR = red[x];
                if (maxG < green1[x]) maxG = green1[x];
                if (maxG < green2[x]) maxG = green2[x];
                if (maxB < blue[x]) maxB = red[x];

                var r = red[x] / divisor;
                var g = (green1[x] + green2[x]) / (divisor * 2);
                var b = blue[x] / divisor;
                var color = Color.FromArgb((byte)r, (byte)g, (byte)b);
                image1.SetPixel(slice * width + x, line, color);
            }

            //if (slice == 0)
            //    for (var i = line; i <= line && i < 200; i++)
            //        Console.WriteLine("{0}:{1} R {2} G {3} G {4} B {5}", line, i, red[i], green1[i], green2[i], blue[i]);
        }

        private static int ProcessColor(StartOfImage startOfImage, HuffmanTable table0, ref int prev)
        {
            var hufCode = GetValue(startOfImage.ImageData, table0);
            var difCode = GetValue(startOfImage.ImageData, hufCode);
            var dif = DecodeDifBits(hufCode, difCode);
            prev = prev + dif;
            return prev;
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

        private static void DumpBitmap(string folder, string file1, string file2)
        {
            var fileName2 = folder + file1;
            using (var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Last();

                var compressoin = imageFileDirectory.Entries.Single(e => e.TagId == 0x0103 && e.TagType == 3).ValuePointer; // TIF_COMPRESSION
                Assert.AreEqual(6u, compressoin);  // JpegCompression

                var strips = imageFileDirectory.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE
                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();
                Console.WriteLine("x {0}, y {1}, z {2}", x, y, z);
                //Assert.AreEqual(1, x);
                //Assert.AreEqual(2960, y);
                //Assert.AreEqual(2960, z);

                var address = imageFileDirectory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length) { ImageData = new ImageData(binaryReader, length) };

                //var startOfScan = startOfImage.StartOfScan;
                //DumpStartOfScan(startOfScan);

                var startOfFrame = startOfImage.StartOfFrame;
                Console.WriteLine("lines {0}, samples per line {1} * {2} = {3}", startOfFrame.ScanLines, startOfFrame.SamplesPerLine, startOfFrame.Components.Length, startOfFrame.Width);
                // Assert.AreEqual(x * y + z, lossless.Width); // Sensor width (bits)
                // Assert.AreEqual(x * y + z, lossless.SamplesPerLine * lossless.Components.Length);

                var rowBuf0 = new short[startOfFrame.SamplesPerLine * startOfFrame.Components.Length];
                var rowBuf1 = new short[startOfFrame.SamplesPerLine * startOfFrame.Components.Length];
                var predictor = new[] { (short)(1 << (startOfFrame.Precision - 1)), (short)(1 << (startOfFrame.Precision - 1)) };
                var table0 = startOfImage.HuffmanTable.Tables[0x00];
                var table1 = startOfImage.HuffmanTable.Tables[0x01];

                //using (var image1 = new Bitmap(startOfFrame.Width, startOfFrame.ScanLines))
                //{
                //    for (var k = 0; k < x; k++)
                //    {
                //        for (var j = 0; j < startOfFrame.ScanLines / 2; j++)
                //        {
                //            for (var i = 0; i < y / 2; i++)
                //            {
                //                var hufCode0 = GetValue(startOfImage.ImageData, table0);
                //                var difCode0 = startOfImage.ImageData.GetSetOfBits(hufCode0);
                //                var dif0 = DecodeDifBits(hufCode0, difCode0);

                //                if (i == 0)
                //                {
                //                    rowBuf0[2 * i] = predictor[0] += dif0;
                //                }
                //                else
                //                {
                //                    rowBuf0[2 * i] = (short)(rowBuf0[2 * i - 2] + dif0);
                //                }

                //                var hufCode1 = GetValue(startOfImage.ImageData, table1);
                //                var difCode1 = startOfImage.ImageData.GetSetOfBits(hufCode1);
                //                var dif1 = DecodeDifBits(hufCode1, difCode1);

                //                if (i == 0)
                //                {
                //                    rowBuf0[2 * i + 1] = predictor[1] += dif1;
                //                }
                //                else
                //                {
                //                    rowBuf0[2 * i + 1] = (short)(rowBuf0[2 * i - 1] + dif1);
                //                }
                //            }

                //            for (var i = 0; i < z / 2; i++)
                //            {
                //                var hufCode0 = GetValue(startOfImage.ImageData, table0);
                //                var difCode0 = startOfImage.ImageData.GetSetOfBits(hufCode0);
                //                var dif0 = DecodeDifBits(hufCode0, difCode0);

                //                if (i == 0)
                //                {
                //                    rowBuf1[2 * i] = (short)(rowBuf0[startOfFrame.SamplesPerLine - 2] + dif0);
                //                }
                //                else
                //                {
                //                    rowBuf1[2 * i] = (short)(rowBuf1[2 * i - 2] + dif0);
                //                }

                //                var hufCode1 = GetValue(startOfImage.ImageData, table1);
                //                var difCode1 = startOfImage.ImageData.GetSetOfBits(hufCode1);
                //                var dif1 = DecodeDifBits(hufCode1, difCode1);

                //                if (i == 0)
                //                {
                //                    rowBuf1[2 * i + 1] = (short)(rowBuf0[startOfFrame.SamplesPerLine - 1] + dif1);
                //                }
                //                else
                //                {
                //                    rowBuf1[2 * i + 1] = (short)(rowBuf1[2 * i - 1] + dif1);
                //                }
                //            }

                //            DumpPixel(k * y, j, rowBuf0, rowBuf1, image1);
                //        }
                //    }

                //    for (var j = 0; j < startOfFrame.ScanLines / 2; j++)
                //    {
                //        for (var i = 0; i < z / 2; i++)
                //        {
                //            var hufCode0 = GetValue(startOfImage.ImageData, table0);
                //            var difCode0 = startOfImage.ImageData.GetSetOfBits(hufCode0);
                //            var dif0 = DecodeDifBits(hufCode0, difCode0);

                //            if (i == 0)
                //            {
                //                rowBuf0[2 * i] = predictor[0] += dif0;
                //            }
                //            else
                //            {
                //                rowBuf0[2 * i] = (short)(rowBuf0[2 * i - 2] + dif0);
                //            }

                //            var hufCode1 = GetValue(startOfImage.ImageData, table1);
                //            var difCode1 = startOfImage.ImageData.GetSetOfBits(hufCode1);
                //            var dif1 = DecodeDifBits(hufCode1, difCode1);

                //            if (i == 0)
                //            {
                //                rowBuf0[2 * i + 1] = predictor[1] += dif1;
                //            }
                //            else
                //            {
                //                rowBuf0[2 * i + 1] = (short)(rowBuf0[2 * i - 1] + dif1);
                //            }
                //        }

                //        for (var i = 0; i < z / 2; i++)
                //        {
                //            var hufCode0 = GetValue(startOfImage.ImageData, table0);
                //            var difCode0 = startOfImage.ImageData.GetSetOfBits(hufCode0);
                //            var dif0 = DecodeDifBits(hufCode0, difCode0);

                //            if (i == 0)
                //            {
                //                rowBuf1[2 * i] = (short)(rowBuf0[startOfFrame.SamplesPerLine - 2] + dif0);
                //            }
                //            else
                //            {
                //                rowBuf1[2 * i] = (short)(rowBuf1[2 * i - 2] + dif0);
                //            }

                //            var hufCode1 = GetValue(startOfImage.ImageData, table1);
                //            var difCode1 = startOfImage.ImageData.GetSetOfBits(hufCode1);
                //            var dif1 = DecodeDifBits(hufCode1, difCode1);

                //            if (i == 0)
                //            {
                //                rowBuf1[2 * i + 1] = (short)(rowBuf0[startOfFrame.SamplesPerLine - 1] + dif1);
                //            }
                //            else
                //            {
                //                rowBuf1[2 * i + 1] = (short)(rowBuf1[2 * i - 1] + dif1);
                //            }
                //        }

                //        DumpPixel(x * y, j, rowBuf0, rowBuf1, image1);
                //    }

                //    image1.Save(bitmap);
                //}

                Console.WriteLine("EOF {0}", startOfImage.ImageData.RawData.Length - startOfImage.ImageData.Index);
            }
        }

        private static short DecodeDifBits(ushort difBits, ushort difCode)
        {
            short dif0;
            if ((difCode & (0x01u << (difBits - 1))) != 0)
            {
                // msb is 1, thus decoded DifCode is positive
                dif0 = (short)difCode;
            }
            else
            {
                // msb is 0, thus DifCode is negative
                var mask = (1 << difBits) - 1;
                var m1 = difCode ^ mask;
                dif0 = (short)(0 - m1);
            }
            return dif0;
        }

        private static byte GetValue(ImageData imageData, HuffmanTable table)
        {
            var hufIndex = (ushort)0;
            var hufBits = (ushort)0;
            HuffmanTable.HCode hCode;
            do
            {
                hufIndex = imageData.GetNextShort(hufIndex);
                hufBits++;
            }
            while (!table.Dictionary.TryGetValue(hufIndex, out hCode) || hCode.Length != hufBits);

            return hCode.Code;
        }

        private static ushort GetValue(ImageData imageData, int bits)
        {
            var hufIndex = (ushort)0;
            for (var i = 0; i < bits; i++)
                hufIndex = imageData.GetNextShort(hufIndex);

            return hufIndex;
        }

        private static void DumpStartOfScan(StartOfScan startOfScan)
        {
            Console.WriteLine("Ns: {0}", startOfScan.Components.Length);
            foreach (var scanComponent in startOfScan.Components)
            {
                Console.WriteLine("    Cs {0}: Td {1}, Ta {2}", scanComponent.Id, scanComponent.Dc, scanComponent.Ac);
            }
        }

        #endregion
    }
}
