namespace PhotoTests
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Jpeg;
    using PhotoLib.Tiff;

    using PhotoLib.Utilities;

    [TestClass]
    public class UnitTests
    {
        #region Public Methods and Operators

        private static void DumpPixel(int col, int row, short[,] rowBuf, Bitmap image1)
        {
            if (col > 0)
            {
                return;
            }

            const int X = 0; // 2116;
            const int Y = 0; // 1416 / 2; // (3950 - 900) / 2;

            var q = row - Y;
            if (q < 0 || q >= 500)
            {
                return;
            }

            for (var p = 0; p < 500 && p + X < rowBuf.GetLength(1); p++)
            {
                var red = (rowBuf[0, p + X] - 2047) >> 4;
                if (red < 0)
                {
                    red = 0;
                }
                else if (red > 0xFF)
                {
                    red = 0xFF;
                }

                var green = ((rowBuf[1, p + X] - 2047) >> 5) + ((rowBuf[2, p + X] - 2047) >> 5);
                if (green < 0)
                {
                    green = 0;
                }
                else if (green > 0xFF)
                {
                    green = 0xfF;
                }

                var blue = (rowBuf[3, p + X] - 2047) >> 4;
                if (blue < 0)
                {
                    blue = 0;
                }
                else if (blue > 0xFF)
                {
                    blue = 0xFF;
                }

                var color = Color.FromArgb(red, green, blue);
                image1.SetPixel(p, q, color);
            }
        }

        [TestMethod]
        public void TestMethodC5M3()
        {
            const string Folder = @"C:\Users\Greg\Pictures\2013-10-06 001\";
            const string FileName2 = Folder + "0L2A8892.CR2";
            const string Bitmap = Folder + "0L2A8892 C.BMP";

            DumpBitmap(FileName2, Bitmap);
        }

        private static void DumpBitmap(string fileName2, string bitmap)
        {
            using (var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Last();

                var strips = imageFileDirectory.Entries.First(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE
                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();
                Console.WriteLine("x {0}, y {1}, z {2}", x, y, z);

                var address = imageFileDirectory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var rawSize = address + length - binaryReader.BaseStream.Position - 2;
                startOfImage.ImageData = new ImageData(binaryReader, (uint)rawSize);

                var lossless = startOfImage.Lossless;
                Console.WriteLine("lines {0}, samples per line {1} * {2}", lossless.ScanLines, lossless.SamplesPerLine, lossless.Components.Length);

                var rowBuf = new short[4, lossless.SamplesPerLine];
                Console.WriteLine("Image: 0x{0}", binaryReader.BaseStream.Position.ToString("X8"));

                var predictor = new[] { (short)(1 << (lossless.Precision - 1)), (short)(1 << (lossless.Precision - 1)) };
                var table0 = startOfImage.HuffmanTable.Tables[0x00];
                var table1 = startOfImage.HuffmanTable.Tables[0x01];

                Assert.AreEqual(x * y + z, lossless.Width); // Sensor width (bits)
                Assert.AreEqual(x * y + z, lossless.SamplesPerLine * lossless.Components.Length);

                using (var image1 = new Bitmap(500, 500))
                {
                    for (var k = 0; k < x; k++)
                    {
                        ParseRow(lossless, y, startOfImage, table0, rowBuf, predictor, k, image1);
                    }

                    ParseRow(lossless, z, startOfImage, table0, rowBuf, predictor, x, image1);

                    image1.Save(bitmap);

                    Console.WriteLine("EOF {0}", startOfImage.ImageData.RawData.Length - startOfImage.ImageData.Index);
                }
            }
        }

        private static void ParseRow(
            StartOfFrame lossless, ushort y, StartOfImage startOfImage, HuffmanTable table0, short[,] rowBuf, short[] predictor, int x, Bitmap image1)
        {
            var i1 = 4 / lossless.Components.Length;

            for (var j = 0; j < lossless.ScanLines / i1; j++)
            {
                for (var g = 0; g < i1; g++)
                {
                    for (var i = 0; i < y / lossless.Components.Length; i++)
                    {
                        for (var h = 0; h < lossless.Components.Length; h++)
                        {
                            var hufCode = GetValue(startOfImage.ImageData, table0);
                            var difCode = startOfImage.ImageData.GetSetOfBits(hufCode);
                            var dif = DecodeDifBits(hufCode, difCode);

                            if (i == 0)
                            {
                                rowBuf[g * i1 + h, i] = predictor[h] += dif;
                            }
                            else
                            {
                                rowBuf[g * i1 + h, i] = (short)(rowBuf[g * i1 + h, i - 1] + dif);
                            }
                        }
                    }
                }

                DumpPixel(x * y, j, rowBuf, image1);
            }
        }

        [TestMethod]
        public void TestMethodC7()
        {
            const string Folder = @"C:\Users\Greg\Pictures\2013-10-06 001\";
            const string FileName2 = Folder + "IMG_3479.CR2";
            const string Bitmap = Folder + "IMG_3479 C.BMP";

            DumpBitmap7(FileName2, Bitmap);
        }

        private static void DumpBitmap7(string fileName2, string bitmap)
        {
            using (var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Last();

                var strips = imageFileDirectory.Entries.First(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE
                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();
                Console.WriteLine("x {0}, y {1}, z {2}", x, y, z);

                var address = imageFileDirectory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var rawSize = address + length - binaryReader.BaseStream.Position - 2;
                // Assert.AreEqual(23852856, rawSize);                                        // RawSize (Raw = new byte[RawSize]
                startOfImage.ImageData = new ImageData(binaryReader, (uint)rawSize);

                var lossless = startOfImage.Lossless;
                Console.WriteLine("lines {0}, samples per line {1} * {2}", lossless.ScanLines, lossless.SamplesPerLine, lossless.Components.Length);

                // Assert.AreEqual(4711440, lossless.SamplesPerLine * lossless.ScanLines);    // IbSize (IB = new ushort[IbSize])
                // var ibSize = lossless.SamplesPerLine * lossless.ScanLines;
                // var ib = new ushort[ibSize];

                var rowBuf = new short[4, lossless.SamplesPerLine];
                Console.WriteLine("Image: 0x{0}", binaryReader.BaseStream.Position.ToString("X8"));

                var predictor = new[] { (short)(1 << (lossless.Precision - 1)), (short)(1 << (lossless.Precision - 1)), (short)(1 << (lossless.Precision - 1)), (short)(1 << (lossless.Precision - 1)) };
                var table0 = startOfImage.HuffmanTable.Tables[0x00];
                var table1 = startOfImage.HuffmanTable.Tables[0x01];

                Assert.AreEqual(x * y + z, lossless.Width); // Sensor width (bits)
                Assert.AreEqual(x * y + z, lossless.SamplesPerLine * lossless.Components.Length);

                using (var image1 = new Bitmap(500, 500))
                {
                    for (var k = 0; k < x; k++)
                    {
                        for (var j = 0; j < lossless.ScanLines / (4 / lossless.Components.Length); j++)
                        {
                            for (var i = 0; i < y / lossless.Components.Length; i++)
                            {
                                var hufCode0 = GetValue(startOfImage.ImageData, table0);
                                var difCode0 = startOfImage.ImageData.GetSetOfBits(hufCode0);
                                var dif0 = DecodeDifBits(hufCode0, difCode0);

                                if (i == 0)
                                {
                                    rowBuf[0, i] = predictor[0] += dif0;
                                }
                                else
                                {
                                    rowBuf[0, i] = (short)(rowBuf[3, i - 1] + dif0);
                                }

                                var hufCode1 = GetValue(startOfImage.ImageData, table0);
                                var difCode1 = startOfImage.ImageData.GetSetOfBits(hufCode1);
                                var dif1 = DecodeDifBits(hufCode1, difCode1);

                                if (i == 0)
                                {
                                    rowBuf[1, i] = predictor[1] += dif1;
                                }
                                else
                                {
                                    rowBuf[1, i] = (short)(rowBuf[0, i - 1] + dif1);
                                }

                                var hufCode2 = GetValue(startOfImage.ImageData, table0);
                                var difCode2 = startOfImage.ImageData.GetSetOfBits(hufCode2);
                                var dif2 = DecodeDifBits(hufCode2, difCode2);

                                if (i == 0)
                                {
                                    rowBuf[2, i] = predictor[2] += dif2;
                                }
                                else
                                {
                                    rowBuf[2, i] = (short)(rowBuf[1, i - 1] + dif2);
                                }

                                var hufCode3 = GetValue(startOfImage.ImageData, table0);
                                var difCode3 = startOfImage.ImageData.GetSetOfBits(hufCode3);
                                var dif3 = DecodeDifBits(hufCode3, difCode3);

                                if (i == 0)
                                {
                                    rowBuf[3, i] = predictor[3] += dif3;
                                }
                                else
                                {
                                    rowBuf[3, i] = (short)(rowBuf[2, i - 1] + dif3);
                                }
                            }

                            // DumpPixelDebug(j, rowBuf0, rowBuf1);
                            DumpPixel(k * y, j, rowBuf, image1);

                            //var cr2Cols = lossless.SamplesPerLine;
                            //var cr2Slice = cr2Cols / 2;
                            //var cr2QuadRows = lossless.ScanLines / 2;
                            //var ibStart = j < cr2QuadRows ? 2 * j * cr2Cols : 2 * (j - cr2QuadRows) * cr2Cols + cr2Slice;
                            //Array.Copy(rowBuf, 0, ib, ibStart, cr2Slice);
                            //Array.Copy(rowBuf, cr2Slice, ib, ibStart + cr2Slice, cr2Slice);
                        }
                    }

                    //for (var j = 0; j < lossless.ScanLines / (4 / lossless.Components.Length); j++)
                    //{
                    //    for (var i = 0; i < z / lossless.Components.Length; i++)
                    //    {
                    //        var hufCode0 = GetValue(startOfImage.ImageData, table0);
                    //        var difCode0 = startOfImage.ImageData.GetSetOfBits(hufCode0);
                    //        var dif0 = DecodeDifBits(hufCode0, difCode0);

                    //        if (i == 0)
                    //        {
                    //            rowBuf0[2 * i] = predictor[0] += dif0;
                    //        }
                    //        else
                    //        {
                    //            rowBuf0[2 * i] = (short)(rowBuf0[2 * i - 2] + dif0);
                    //        }

                    //        var hufCode1 = GetValue(startOfImage.ImageData, table0);
                    //        var difCode1 = startOfImage.ImageData.GetSetOfBits(hufCode1);
                    //        var dif1 = DecodeDifBits(hufCode1, difCode1);

                    //        if (i == 0)
                    //        {
                    //            rowBuf0[2 * i + 1] = predictor[1] += dif1;
                    //        }
                    //        else
                    //        {
                    //            rowBuf0[2 * i + 1] = (short)(rowBuf0[2 * i - 1] + dif1);
                    //        }

                    //        var hufCode2 = GetValue(startOfImage.ImageData, table0);
                    //        var difCode2 = startOfImage.ImageData.GetSetOfBits(hufCode2);
                    //        var dif2 = DecodeDifBits(hufCode2, difCode2);

                    //        if (i == 0)
                    //        {
                    //            // rowBuf1[2 * i] = predictor[0] += dif0;
                    //            rowBuf1[2 * i] = (short)(rowBuf0[lossless.SamplesPerLine - 2] + dif2);
                    //        }
                    //        else
                    //        {
                    //            rowBuf1[2 * i] = (short)(rowBuf1[2 * i - 2] + dif2);
                    //        }

                    //        var hufCode3 = GetValue(startOfImage.ImageData, table0);
                    //        var difCode3 = startOfImage.ImageData.GetSetOfBits(hufCode3);
                    //        var dif3 = DecodeDifBits(hufCode3, difCode3);

                    //        if (i == 0)
                    //        {
                    //            // rowBuf1[2 * i + 1] = predictor[1] += dif1;
                    //            rowBuf1[2 * i + 1] = (short)(rowBuf0[lossless.SamplesPerLine - 1] + dif3);
                    //        }
                    //        else
                    //        {
                    //            rowBuf1[2 * i + 1] = (short)(rowBuf1[2 * i - 1] + dif3);
                    //        }
                    //    }

                    // DumpPixelDebug(j, rowBuf0, rowBuf1);
                    //DumpPixel(x * y, j, rowBuf0, rowBuf1, image1);

                    //var cr2Cols = lossless.SamplesPerLine;
                    //var cr2Slice = cr2Cols / 2;
                    //var cr2QuadRows = lossless.ScanLines / 2;
                    //var ibStart = j < cr2QuadRows ? 2 * j * cr2Cols : 2 * (j - cr2QuadRows) * cr2Cols + cr2Slice;
                    //Array.Copy(rowBuf, 0, ib, ibStart, cr2Slice);
                    //Array.Copy(rowBuf, cr2Slice, ib, ibStart + cr2Slice, cr2Slice);
                    //}

                    image1.Save(bitmap);

                    Console.WriteLine("EOF {0}", startOfImage.ImageData.RawData.Length - startOfImage.ImageData.Index);
                }
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

        #endregion
    }
}
