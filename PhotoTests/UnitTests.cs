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
    public class UnitTests
    {
        #region Public Methods and Operators

        [Ignore]
        [TestMethod]
        public void TestMethodC7()
        {
            const string Folder = @"C:\Users\Greg\Pictures\2013_10_14\";
            const string FileName2 = Folder + "IMG_4195.CR2";
            const string Bitmap = Folder + "IMG_4195 C.BMP";

            DumpBitmap(FileName2, Bitmap);
        }

        [TestMethod]
        public void ReadFile()
        {
            const string Folder = @"C:\Users\Greg\Pictures\2013_10_14\";
            const string FileName2 = Folder + "IMG_4195.CR2";

            using (var fileStream = File.Open(FileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Last();

                var address = imageFileDirectory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                Assert.IsTrue(startOfImage.ImageData.EndOfFile);
                Assert.AreEqual(startOfImage.ImageData.RawData.Length, startOfImage.ImageData.Index + 1);
                Assert.AreEqual(7, startOfImage.ImageData.BitsLeft);
            }
        }

        private static
            void DumpBitmap(string fileName2, string bitmap)
        {
            using (var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Last();

                // compression for 7D RAW should be 4 -
                // compression for 7D mRAW and sRAW should be 3 -
                // compression for 5DIII RAW should be 2 -
                // compression for 5DIII mRAW and sRAW should be 3 -
                //var compressoin = imageFileDirectory.Entries.Single(e => e.TagId == 0x0103 && e.TagType == 3).ValuePointer; // TIF_COMPRESSION
                //Assert.AreEqual(6u, compressoin);  // JpegCompression

                var strips = imageFileDirectory.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE
                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();
                Console.WriteLine("x {0}, y {1}, z {2}", x, y, z);
                //Assert.AreEqual(2, x);
                //Assert.AreEqual(1728, y);
                //Assert.AreEqual(1904, z);

                var address = imageFileDirectory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
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

                var predictor = new[]
                    {
                        (short)(1 << (startOfFrame.Precision - 1)), (short)(1 << (startOfFrame.Precision - 1)), (short)(1 << (startOfFrame.Precision - 1)),
                        (short)(1 << (startOfFrame.Precision - 1))
                    };

                using (var image1 = new Bitmap(startOfFrame.SamplesPerLine, startOfFrame.ScanLines))
                {
                    for (var k = 0; k < x; k++)
                    {
                        ParseRow(startOfFrame.ScanLines, k, y, y, startOfImage.ImageData, predictor, table0, table1, image1);
                    }

                    ParseRow(startOfFrame.ScanLines, x, y, z, startOfImage.ImageData, predictor, table0, table1, image1);

                    image1.Save(bitmap);
                }

                Console.WriteLine("EOF {0}", startOfImage.ImageData.RawData.Length - startOfImage.ImageData.Index);
            }
        }

        private static void ParseRow(int lines, int x, ushort y1, ushort y2, ImageData imageData, short[] predictor, HuffmanTable table0, HuffmanTable table1, Bitmap image1)
        {
            for (var j = 0; j < lines; j++)
            {
                var rowBuf = new short[4, y2 / 4];
                for (var i = 0; i < y2 / 4; i++)  // i < y / component count
                {
                    for (var w = 0; w < 2; w++)
                    {
                        var hufCode0 = GetValue(imageData, table0);
                        var difCode0 = imageData.GetSetOfBits(hufCode0);
                        rowBuf[w, i] = (short)difCode0;
                    }

                    for (var w = 2; w < 4; w++)
                    {
                        var hufCode = GetValue(imageData, table1);
                        var difCode = imageData.GetSetOfBits(hufCode);

                        rowBuf[w, i] = (short)((short)(difCode << 2) >> 2);
                    }
                }

                DumpPixel(x * y1 / 4, j, rowBuf, image1);
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
            Console.WriteLine("Precision {0}", startOfFrame.Precision);
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

        public static void DumpPixel(int col, int row, short[,] rowBuf, Bitmap image1)
        {
            // DumpPixelDebug(col, row, rowBuf);

            const int X = 0; // 2116;
            const int Y = 0; // 1416 / 2; // (3950 - 900) / 2;

            var q = row - Y;
            if (q < 0 || q >= image1.Height)
            {
                return;
            }

            for (var p = 0; p + X < rowBuf.GetLength(1) && 2 * p + X + col < image1.Width; p++)
            {
                var y1 = rowBuf[0, p + X];
                var y2 = rowBuf[1, p + X];
                var cb1 = rowBuf[2, p + X];
                var cr1 = rowBuf[3, p + X];

                var cb2 = cb1;
                var cr2 = cr1;
                // cb2[column] = ( cb1[column-1] + cb1[column+1] + 1 ) / 2
                // cr2[column] = ( cr1[column-1] + cr1[column+1] + 1 ) / 2

                // R = Y + 1.40 Cr
                var r1 = y1 + 1.40 * cr1;
                var r2 = y2 + 1.40 * cr2;

                // G = Y - 0.34414 Cb - 0.71414 Cr
                var g1 = y1 - 0.34414 * cb1 - 0.71414 * cr1;
                var g2 = y2 - 0.34414 * cb2 - 0.71414 * cr2;

                // B = Y + 1.772 Cb
                var b1 = y1 + 1.772 * cb1;
                var b2 = y2 + 1.772 * cb2;

                var color1 = Color.FromArgb(Bound(r1), Bound(g1), Bound(b1));
                var color2 = Color.FromArgb(Bound(r2), Bound(g2), Bound(b2));
                image1.SetPixel((p + col) * 2 + 0, q + 0, color1);
                image1.SetPixel((p + col) * 2 + 1, q + 0, color2);
            }
        }

        private static int Bound(double value)
        {
            value *= 32;
            if (value < 0)
            {
                value = 0;
            }
            else if (value > 0xFF)
            {
                value = 0xFF;
            }
            return (int)value;
        }

        private static void DumpPixelDebug1(int col, int row, short[,] rowBuf)
        {
            var q = row;

            for (var p = 0; p < 1; p++)
            {
                const int Offset = 0;
                var red = rowBuf[0, p] - Offset;
                var green = rowBuf[1, p] - Offset;
                var green2 = rowBuf[2, p] - Offset;
                var blue = rowBuf[3, p] - Offset;

                Console.WriteLine("col:{4}, row:{5}: {0}, {1}, {2}, {3}", red, green, green2, blue, p + 1 + col, q + 1);
            }
        }

        private static void DumpPixelDebug2(int col, int row, short[,] rowBuf)
        {
            var q = row;

            for (var p = 0; p < rowBuf.GetLongLength(1); p++)
            {
                var oops = true;
                for (var i = 0; oops && i < rowBuf.GetLength(0); i++)
                {
                    var point = rowBuf[i, p];
                    var b1 = -500 < point;
                    var b2 = point < 500;
                    oops = b1 && b2;
                }

                if (oops)
                    continue;

                const int Offset = 0;
                var red = rowBuf[0, p] - Offset;
                var green = rowBuf[1, p] - Offset;
                var green2 = rowBuf[2, p] - Offset;
                var blue = rowBuf[3, p] - Offset;

                Console.WriteLine("col:{4}, row:{5}: {0}, {1}, {2}, {3}", red, green, green2, blue, p + 1 + col, q + 1);
            }
        }

        private static void DumpPixelDebug(int col, int row, short[,] rowBuf)
        {
            // col:1729, row:4: -870, -1, -28, 30
            const int X = 0; // 122; // 2116;
            const int Y = 0; // 40; // 1416 / 2; // (3950 - 900) / 2;

            var q = row - Y;
            if (q < 0 || q >= 50 || col != 1728)
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

                Console.WriteLine("col:{4}, row:{5}: {0}, {1}, {2}, {3}", red, green, green2, blue, p + 1 + X + col, q + 1 + Y);
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
