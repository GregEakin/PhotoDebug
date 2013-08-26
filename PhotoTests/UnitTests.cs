namespace PhotoTests
{
    using System;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Jpeg;
    using PhotoLib.Tiff;

    [TestClass]
    public class UnitTests
    {
        #region Public Methods and Operators

        public void TestMethod7()
        {
            const string Directory = @"C:\Users\Greg\Documents\Visual Studio 2012\Projects\PhotoDebug\Samples\";
            const string FileName2 = Directory + "huff_simple0.jpg";

            using (var fileStream = File.Open(FileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);

                binaryReader.BaseStream.Seek(0x000000D0u, SeekOrigin.Begin);
                var huffmanTable = new DefineHuffmanTable(binaryReader);
                Assert.AreEqual(0xFF, huffmanTable.Mark);
                Assert.AreEqual(0xC4, huffmanTable.Tag);
                Console.WriteLine(huffmanTable);
            }
        }

        // [TestMethod]
        public void TestMethod8()
        {
            const string Directory = @"C:\Users\Greg\Documents\Visual Studio 2012\Projects\PhotoDebug\Samples\";
            const string FileName2 = Directory + "IMAG0086.jpg";

            using (var fileStream = File.Open(FileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var startOfImage = new StartOfImage(binaryReader, 0x00u, (uint)fileStream.Length);
                Assert.AreEqual(0xFF, startOfImage.Mark);
                Assert.AreEqual(0xD8, startOfImage.Tag); // JPG_MARK_SOI

                var huffmanTable = startOfImage.HuffmanTable;
                Assert.AreEqual(0xFF, huffmanTable.Mark);
                Assert.AreEqual(0xC4, huffmanTable.Tag);
                Console.WriteLine(huffmanTable);
            }
        }

        public void TestMethodA()
        {
            // it is the width of a
            // row.	The initial values at the beginning of each row is the RG/GB value of
            // its nearest previous row beginning.  For the first row, the initial row
            // values are 1/2 the bit range defined by the precision.  Thus for 12-bit
            // precision:
            //     Pix[Row, Col] = Val
            //     Pix[0,0] = (1 << (Precision - 1)) + Diff
            //     Pix[0,1] = (1 << (Precision - 1)) + Diff
            // and for n >= 1
            //     Pix[n,0] = Pix[n-2,0] + Diff
            //     Pix[n,1] = Pix[n-2,1] + Diff
            // while for any other Row/Column
            //     Pix[R,C] = Pix[R,C-2] + Diff
        }

        // [TestMethod]
        public void TestMethodB()
        {
            const string Directory = @"C:\Users\Greg\Documents\Visual Studio 2012\Projects\PhotoDebug\Samples\";
            const string FileName2 = Directory + "IMG_0503.CR2";

            using (var fileStream = File.Open(FileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Last();

                var strips = imageFileDirectory.Entries.First(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE
                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();

                var address = imageFileDirectory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var lossless = startOfImage.Lossless;
                Assert.AreEqual(4711440, lossless.SamplesPerLine * lossless.ScanLines); // IbSize (IB = new ushort[IbSize])

                var rawSize = address + length - binaryReader.BaseStream.Position - 2;
                Assert.AreEqual(23852856, rawSize); // RawSize (Raw = new byte[RawSize]
                startOfImage.ImageData = new ImageData(binaryReader, (uint)rawSize);
                var table0 = startOfImage.HuffmanTable.Tables[0x00];

                var buffer = new byte[rawSize];

                var rp = 0;
                for (var jrow = 0; jrow < lossless.ScanLines; jrow++)
                {
                    for (var jcol = 0; jcol < lossless.SamplesPerLine; jcol++)
                    {
                        var val = startOfImage.ImageData.RawData[rp++];
                        var jidx = jrow * lossless.SamplesPerLine + jcol;
                        var i = jidx / (y * lossless.ScanLines);
                        var j = i >= x;
                        if (j)
                            i = x;
                        jidx -= i * (y * lossless.ScanLines);
                        var row = jidx / (j ? y : z);
                        var col = jidx % (j ? y : z) + i * y;

                        buffer[row * lossless.SamplesPerLine + col] = val;
                    }
                }
            }
        }

        // [TestMethod]
        public void TestMethodB6()
        {
            const string Folder = @"C:\Users\Greg\Documents\Visual Studio 2012\Projects\PhotoDebug\Samples\";
            const string FileName2 = Folder + "IMG_0503.CR2";
            ProcessFile(FileName2);
        }

        private static void ProcessFile(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Last();

                var strips = imageFileDirectory.Entries.First(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE
                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();

                var address = imageFileDirectory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var lossless = startOfImage.Lossless;

                var rawSize = address + length - binaryReader.BaseStream.Position - 2;
                // Assert.AreEqual(23852858, rawSize); // RawSize (Raw = new byte[RawSize]
                startOfImage.ImageData = new ImageData(binaryReader, (uint)rawSize);

                var colors = lossless.Components.Sum(comp => comp.HFactor * comp.VFactor);
                var rowBuf = new ushort[lossless.SamplesPerLine * colors];

                var table0 = startOfImage.HuffmanTable.Tables[0x00];

                // var buffer = new byte[rawSize];

                for (var jrow = 0; jrow < lossless.ScanLines; jrow++)
                {
                    for (var jcol = 0; jcol < lossless.SamplesPerLine; jcol++)
                    {
                        for (var jcolor = 0; jcolor < colors; jcolor++)
                        {
                            //var pred = (ushort)0;
                            //var len = gethuff();
                            //var diff = getbits(len);
                            //var row = pred + diff;

                            var val = GetValue(startOfImage.ImageData, table0);
                            var bits = startOfImage.ImageData.GetSetOfBits(val);
                            rowBuf[jcol * colors + jcolor] = bits;
                        }
                    }
                    // var pp = startOfImage.ImageData.Index;
                }

                // Assert.AreEqual(23852855, startOfImage.ImageData.Index);
                Console.WriteLine("{0}: ", startOfImage.ImageData.BitsLeft);
                for (var i = startOfImage.ImageData.Index; i < rawSize - 2; i++)
                {
                    Console.WriteLine("{0} ", startOfImage.ImageData.RawData[i].ToString("X2"));
                }
            }
        }

        public void TestMethodC()
        {
            const string Directory = @"C:\Users\Greg\Documents\Visual Studio 2012\Projects\PhotoDebug\Samples\";
            const string FileName2 = Directory + "IMG_0503.CR2";

            using (var fileStream = File.Open(FileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Last();

                var strips = imageFileDirectory.Entries.First(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE
                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();

                var address = imageFileDirectory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var rawSize = address + length - binaryReader.BaseStream.Position - 2;
                startOfImage.ImageData = new ImageData(binaryReader, (uint)rawSize);

                var lossless = startOfImage.Lossless;

                Assert.AreEqual(4711440, lossless.SamplesPerLine * lossless.ScanLines);    // IbSize (IB = new ushort[IbSize])
                var ibSize = lossless.SamplesPerLine * lossless.ScanLines;
                var ib = new ushort[ibSize];

                Assert.AreEqual(23852858, rawSize);                                        // RawSize (Raw = new byte[RawSize]
                var rowBuf = new short[lossless.SamplesPerLine];
                Console.WriteLine("Image: 0x{0}", binaryReader.BaseStream.Position.ToString("X8"));

                var predictor = new[] { (short)(1 << lossless.Precision - 1), (short)(1 << lossless.Precision - 1) };
                var table0 = startOfImage.HuffmanTable.Tables[0x00];
                var table1 = startOfImage.HuffmanTable.Tables[0x01];

                for (var j = 0; j < lossless.ScanLines; j++)
                {
                    for (var i = 0; i < lossless.SamplesPerLine; i += 2)
                    {
                        var hufCode0 = GetValue(startOfImage.ImageData, table0);
                        var difCode0 = startOfImage.ImageData.GetSetOfBits(hufCode0);
                        var dif0 = DecodeDifBits(difCode0, hufCode0);

                        var hufCode1 = GetValue(startOfImage.ImageData, table1);
                        var difCode1 = startOfImage.ImageData.GetSetOfBits(hufCode1);
                        var dif1 = DecodeDifBits(difCode1, hufCode1);

                        if (i == 0)
                        {
                            rowBuf[0] = predictor[0] += dif0;
                            rowBuf[1] = predictor[1] += dif1;
                        }
                        else
                        {
                            rowBuf[i + 0] = (short)(rowBuf[i - 2] + dif0);
                            rowBuf[i + 1] = (short)(rowBuf[i - 1] + dif1);
                        }
                    }

                    var cr2Cols = lossless.SamplesPerLine;
                    var cr2Slice = cr2Cols / 2;
                    var cr2QuadRows = lossless.ScanLines / 2;
                    var ibStart = j < cr2QuadRows
                        ? 2 * j * cr2Cols
                        : 2 * (j - cr2QuadRows) * cr2Cols + cr2Slice;
                    Array.Copy(rowBuf, 0, ib, ibStart, cr2Slice);
                    Array.Copy(rowBuf, cr2Slice, ib, ibStart + cr2Slice, cr2Slice);
                }
            }
        }

        private static short DecodeDifBits(ushort difCode, ushort difBits)
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
