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

        [TestMethod]
        public void TestMethodC7()
        {
            const string Folder = @"C:\Users\Greg\Pictures\2013_10_14\";
            const string FileName2 = Folder + "IMG_4195.CR2";
            const string Bitmap = Folder + "IMG_4195 C.BMP";

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
                //Assert.AreEqual(2, x);
                //Assert.AreEqual(1728, y);
                //Assert.AreEqual(1904, z);

                var address = imageFileDirectory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length) { ImageData = new ImageData(binaryReader, length) };

                // Step 1: Huffman Table
                var huffmanTable = startOfImage.HuffmanTable;
                var table0 = huffmanTable.Tables[0x00];
                var table1 = huffmanTable.Tables[0x01];
                DumpHuffmanTable(huffmanTable);

                // Step 2: Start of Frame
                var startOfFrame = startOfImage.StartOfFrame;
                DumpStartOfFrame(startOfFrame);
                //Assert.AreEqual(x * y + z, startOfFrame.Width); // Sensor width (bits)
                //Assert.AreEqual(x * y + z, startOfFrame.SamplesPerLine * startOfFrame.Components.Length);

                // Step 3: Start of Scan
                var startOfScan = startOfImage.StartOfScan;
                DumpStartOfScan(startOfScan);

                var rowBuf = new short[4, Math.Max(y, z) / 4];
                //var predictor = new[]
                //    {
                //        (short)(1 << (startOfFrame.Precision - 1)), (short)(1 << (startOfFrame.Precision - 1)), (short)(1 << (startOfFrame.Precision - 1)),
                //        (short)(1 << (startOfFrame.Precision - 1))
                //    };

                using (var image1 = new Bitmap(startOfFrame.Width, startOfFrame.ScanLines))
                {
                    for (var k = 0; k < x; k++)
                    {
                        ParseRow(startOfFrame.ScanLines, k, y, startOfImage.ImageData, table0, table1, rowBuf, image1);
                    }

                    ParseRow(startOfFrame.ScanLines, x, z, startOfImage.ImageData, table0, table1, rowBuf, image1);

                    // image1.Save(bitmap);
                }

                Console.WriteLine("EOF {0}", startOfImage.ImageData.RawData.Length - startOfImage.ImageData.Index);
            }
        }

        private static void ParseRow(int lines, int x, ushort y, ImageData imageData, HuffmanTable table0, HuffmanTable table1, short[,] rowBuf, Bitmap image1)
        {
            for (var j = 0; j < lines; j++)
            {
                for (var i = 0; i < y / 4; i++)
                {
                    for (var w = 0; w < 2; w++)
                    {
                        var hufCode0 = GetValue(imageData, table0);
                        var difCode0 = imageData.GetSetOfBits(hufCode0);
                        var dif0 = DecodeDifBits(hufCode0, difCode0);

                        if (i < 4)
                        {
                            rowBuf[w, i] = dif0; // predictor[i % 4] += dif0;
                        }
                        else
                        {
                            rowBuf[w, i] = dif0; // (short)(rowBuf[i % 4, i / 4 - 1] + dif0);
                        }
                    }
                    for (var w = 0; w < 2; w++)
                    {
                        var hufCode0 = GetValue(imageData, table1);
                        var difCode0 = imageData.GetSetOfBits(hufCode0);
                        var dif0 = DecodeDifBits(hufCode0, difCode0);

                        if (i < 4)
                        {
                            rowBuf[w + 2, i] = dif0; // predictor[i % 4] += dif0;
                        }
                        else
                        {
                            rowBuf[w + 2, i] = dif0; // (short)(rowBuf[i % 4, i / 4 - 1] + dif0);
                        }
                    }
                }

                DumpPixel(x * y, j, y / 4, rowBuf, image1);
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

        private static void DumpStartOfFrame(StartOfFrame startOfFrame)
        {
            var hMax = startOfFrame.Components.Max(c => c.HFactor);
            var vMax = startOfFrame.Components.Max(c => c.VFactor);
            Console.WriteLine(
                "lines {0}, samples per line {1} * {2} = {3}",
                startOfFrame.ScanLines,
                startOfFrame.SamplesPerLine,
                startOfFrame.Components.Length,
                startOfFrame.Width);
            Console.WriteLine("Nf: {0}", startOfFrame.Components.Length);
            Console.WriteLine("X={0}, Y={1}, Hmax={2}, Vmax={3}", startOfFrame.Width, startOfFrame.ScanLines, hMax, vMax);
            foreach (var component in startOfFrame.Components)
            {
                Console.Write("    C {0}: H {1}, V {2}", component.ComponentId, component.HFactor, component.VFactor);
                Console.WriteLine(" --> H {0}, V {1}", startOfFrame.Width / (hMax / component.HFactor), startOfFrame.ScanLines / (vMax / component.VFactor));
            }
        }

        private static void DumpHuffmanTable(DefineHuffmanTable huffmanTable)
        {
            Console.WriteLine("HuffmanTable {0}, {1}", huffmanTable.Length, huffmanTable.Tables.Count);
            foreach (var value in huffmanTable.Tables.Values)
            {
                Console.WriteLine("   Table {0}, {1}", value.Index, value.Dictionary.Count);
            }
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

                var lossless = startOfImage.StartOfFrame;
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
                            DumpPixel(k * y, j, y, rowBuf, image1);

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

        public static void DumpPixel(int col, int row, int len, short[,] rowBuf, Bitmap image1)
        {
            DumpPixelDebug(col, row, rowBuf);

            //const int X = 0; // 2116;
            //const int Y = 0; // 1416 / 2; // (3950 - 900) / 2;

            //var q = row - Y;
            //if (q >= image1.Height)
            //{
            //    return;
            //}

            //for (var p = 0; p < len && 4 * p + X + col < image1.Width; p += 4)
            //{
            //    var red = (rowBuf[0, p + X] - 2047) >> 4;
            //    if (red < 0)
            //    {
            //        red = 0;
            //    }
            //    else if (red > 0xFF)
            //    {
            //        red = 0xFF;
            //    }
            //    var color1 = Color.FromArgb(red, 0, 0);
            //    image1.SetPixel(4 * p + 0 + col, q + 0, color1);

            //    var green1 = (rowBuf[1, p + X] - 2047) >> 6;
            //    if (green1 < 0)
            //    {
            //        green1 = 0;
            //    }
            //    else if (green1 > 0xFF)
            //    {
            //        green1 = 0xfF;
            //    }
            //    var color2 = Color.FromArgb(0, green1, 0);
            //    image1.SetPixel(4 * p + 1 + col, q + 0, color2);

            //    var green2 = (rowBuf[2, p + X] - 2047) >> 6;
            //    if (green2 < 0)
            //    {
            //        green2 = 0;
            //    }
            //    else if (green2 > 0xFF)
            //    {
            //        green2 = 0xfF;
            //    }
            //    var color3 = Color.FromArgb(0, green2, 0);
            //    image1.SetPixel(4 * p + 2 + col, q + 0, color3);

            //    var blue = (rowBuf[3, p + X] - 2047) >> 4;
            //    if (blue < 0)
            //    {
            //        blue = 0;
            //    }
            //    else if (blue > 0xFF)
            //    {
            //        blue = 0xFF;
            //    }
            //    var color4 = Color.FromArgb(0, 0, blue);
            //    image1.SetPixel(4 * p + 3 + col, q + 0, color4);
            //}
        }

        private static void DumpPixelDebug(int col, int row, short[,] rowBuf)
        {
            const int X = 0; // 122; // 2116;
            const int Y = 0; // 40; // 1416 / 2; // (3950 - 900) / 2;

            var q = row - Y;
            if (q < 0 || q >= 50 || col > 0)
            {
                return;
            }

            for (var p = 0; p < 6; p++)
            {
                const int Offset = 0;
                var red = rowBuf[0, p + X] - Offset;
                var green = rowBuf[1, p + X] - Offset;
                var green2 = rowBuf[2, p + X] - Offset;
                var blue = rowBuf[3, p + X] - Offset;

                Console.WriteLine("col:{4}, row:{5}: {0}, {1}, {2}, {3}", red, green, green2, blue, p + 1 + X, q + 1 + Y);
            }
        }

        public static short DecodeDifBits(ushort difBits, ushort difCode)
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

        public static ushort GetValue(ImageData imageData, HuffmanTable table)
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
