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

        [Ignore]
        [TestMethod]
        public void TestMethodC5M3()
        {
            const string Folder = @"C:\Users\Greg\Pictures\2013_10_14\";
            const string FileName2 = Folder + "0L2A8897.CR2";
            const string Bitmap = Folder + "0L2A8897 Before.BMP";

            DumpBitmap(FileName2, Bitmap);
        }

        private static void DumpBitmap(string fileName2, string bitmap)
        {
            using (var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Last();

                var compressoin = imageFileDirectory.Entries.First(e => e.TagId == 0x0103 && e.TagType == 3).ValuePointer; // TIF_COMPRESSION
                Assert.AreEqual(6u, compressoin);  // JpegCompression

                var strips = imageFileDirectory.Entries.First(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE
                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();
                Console.WriteLine("x {0}, y {1}, z {2}", x, y, z);
                //Assert.AreEqual(1, x);
                //Assert.AreEqual(2960, y);
                //Assert.AreEqual(2960, z);

                var address = imageFileDirectory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
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

                using (var image1 = new Bitmap(startOfFrame.Width, startOfFrame.ScanLines))
                {
                    for (var k = 0; k < x; k++)
                    {
                        for (var j = 0; j < startOfFrame.ScanLines / 2; j++)
                        {
                            for (var i = 0; i < y / 2; i++)
                            {
                                var hufCode0 = GetValue(startOfImage.ImageData, table0);
                                var difCode0 = startOfImage.ImageData.GetSetOfBits(hufCode0);
                                var dif0 = DecodeDifBits(hufCode0, difCode0);

                                if (i == 0)
                                {
                                    rowBuf0[2 * i] = predictor[0] += dif0;
                                }
                                else
                                {
                                    rowBuf0[2 * i] = (short)(rowBuf0[2 * i - 2] + dif0);
                                }

                                var hufCode1 = GetValue(startOfImage.ImageData, table1);
                                var difCode1 = startOfImage.ImageData.GetSetOfBits(hufCode1);
                                var dif1 = DecodeDifBits(hufCode1, difCode1);

                                if (i == 0)
                                {
                                    rowBuf0[2 * i + 1] = predictor[1] += dif1;
                                }
                                else
                                {
                                    rowBuf0[2 * i + 1] = (short)(rowBuf0[2 * i - 1] + dif1);
                                }
                            }

                            for (var i = 0; i < z / 2; i++)
                            {
                                var hufCode0 = GetValue(startOfImage.ImageData, table0);
                                var difCode0 = startOfImage.ImageData.GetSetOfBits(hufCode0);
                                var dif0 = DecodeDifBits(hufCode0, difCode0);

                                if (i == 0)
                                {
                                    rowBuf1[2 * i] = (short)(rowBuf0[startOfFrame.SamplesPerLine - 2] + dif0);
                                }
                                else
                                {
                                    rowBuf1[2 * i] = (short)(rowBuf1[2 * i - 2] + dif0);
                                }

                                var hufCode1 = GetValue(startOfImage.ImageData, table1);
                                var difCode1 = startOfImage.ImageData.GetSetOfBits(hufCode1);
                                var dif1 = DecodeDifBits(hufCode1, difCode1);

                                if (i == 0)
                                {
                                    rowBuf1[2 * i + 1] = (short)(rowBuf0[startOfFrame.SamplesPerLine - 1] + dif1);
                                }
                                else
                                {
                                    rowBuf1[2 * i + 1] = (short)(rowBuf1[2 * i - 1] + dif1);
                                }
                            }

                            DumpPixel(k * y, j, rowBuf0, rowBuf1, image1);
                        }
                    }

                    for (var j = 0; j < startOfFrame.ScanLines / 2; j++)
                    {
                        for (var i = 0; i < z / 2; i++)
                        {
                            var hufCode0 = GetValue(startOfImage.ImageData, table0);
                            var difCode0 = startOfImage.ImageData.GetSetOfBits(hufCode0);
                            var dif0 = DecodeDifBits(hufCode0, difCode0);

                            if (i == 0)
                            {
                                rowBuf0[2 * i] = predictor[0] += dif0;
                            }
                            else
                            {
                                rowBuf0[2 * i] = (short)(rowBuf0[2 * i - 2] + dif0);
                            }

                            var hufCode1 = GetValue(startOfImage.ImageData, table1);
                            var difCode1 = startOfImage.ImageData.GetSetOfBits(hufCode1);
                            var dif1 = DecodeDifBits(hufCode1, difCode1);

                            if (i == 0)
                            {
                                rowBuf0[2 * i + 1] = predictor[1] += dif1;
                            }
                            else
                            {
                                rowBuf0[2 * i + 1] = (short)(rowBuf0[2 * i - 1] + dif1);
                            }
                        }

                        for (var i = 0; i < z / 2; i++)
                        {
                            var hufCode0 = GetValue(startOfImage.ImageData, table0);
                            var difCode0 = startOfImage.ImageData.GetSetOfBits(hufCode0);
                            var dif0 = DecodeDifBits(hufCode0, difCode0);

                            if (i == 0)
                            {
                                rowBuf1[2 * i] = (short)(rowBuf0[startOfFrame.SamplesPerLine - 2] + dif0);
                            }
                            else
                            {
                                rowBuf1[2 * i] = (short)(rowBuf1[2 * i - 2] + dif0);
                            }

                            var hufCode1 = GetValue(startOfImage.ImageData, table1);
                            var difCode1 = startOfImage.ImageData.GetSetOfBits(hufCode1);
                            var dif1 = DecodeDifBits(hufCode1, difCode1);

                            if (i == 0)
                            {
                                rowBuf1[2 * i + 1] = (short)(rowBuf0[startOfFrame.SamplesPerLine - 1] + dif1);
                            }
                            else
                            {
                                rowBuf1[2 * i + 1] = (short)(rowBuf1[2 * i - 1] + dif1);
                            }
                        }

                        DumpPixel(x * y, j, rowBuf0, rowBuf1, image1);
                    }

                    image1.Save(bitmap);
                }

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

        private static ushort GetValue(ImageData imageData, HuffmanTable table)
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
