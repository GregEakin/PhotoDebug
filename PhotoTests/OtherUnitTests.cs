namespace PhotoTests
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PhotoLib.Jpeg;
    using PhotoLib.Tiff;
    using PhotoLib.Utilities;

    [TestClass]
    public class OtherUnitTests
    {
        #region Public Methods and Operators

        [Ignore]
        [TestMethod]
        public void DumpHuffmanTable()
        {
            const string Directory = @"..\..\..\Samples\";
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

        [Ignore]
        [TestMethod]
        public void TestMethod8()
        {
            const string Directory = @"..\..\..\Samples\";
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

        [Ignore]
        [TestMethod]
        public void TestMethodB()
        {
            // const string Directory = @"C:\Users\Greg\Documents\Visual Studio 2012\Projects\PhotoDebug\Samples\";
            // const string FileName2 = Directory + "IMG_0503.CR2";
            const string Directory = @"D:\Users\Greg\Pictures\2013-10-06 001\";
            const string FileName2 = Directory + "0L2A8892.CR2";

            using (var fileStream = File.Open(FileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Last();

                var strips = imageFileDirectory.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE
                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();

                var address = imageFileDirectory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var lossless = startOfImage.StartOfFrame;
                // Assert.AreEqual(4711440, lossless.SamplesPerLine * lossless.ScanLines); // IbSize (IB = new ushort[IbSize])

                var rawSize = address + length - binaryReader.BaseStream.Position - 2;
                // Assert.AreEqual(23852856, rawSize); // RawSize (Raw = new byte[RawSize]
                startOfImage.ImageData = new ImageData(binaryReader, (uint)rawSize);
                var table0 = startOfImage.HuffmanTable.Tables[0x00];

                var buffer = new byte[rawSize];

                var rp = 0;
                for (var jrow = 0; jrow < lossless.ScanLines; jrow++)
                {
                    for (var jcol = 0; jcol < lossless.SamplesPerLine; jcol++)
                    {
                        var val = startOfImage.ImageData.RawData[rp++];
                        if (val == 0xFF)
                        {
                            var code = startOfImage.ImageData.RawData[rp];
                            if (code == 0)
                            {
                                rp++;
                            }
                            else
                            {
                                Assert.Fail("Invalid code found {0}, {1}", rp, startOfImage.ImageData.RawData[rp]);
                            }
                        }

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

        [Ignore]
        [TestMethod]
        public void TestMethodB6()
        {
            // const string Folder = @"C:\Users\Greg\Documents\Visual Studio 2012\Projects\PhotoDebug\Samples\";
            // const string FileName2 = Folder + "IMG_0503.CR2";

            const string Folder = @"D:\Users\Greg\Pictures\2013-10-06 001\";
            const string FileName2 = Folder + "0L2A8892.CR2";
            const string Bitmap = Folder + "0L2A8892 B6.BMP";

            //const string Folder = @"C:\Users\Greg\Pictures\2013_06_02\";
            //const string FileName2 = Folder + "IMG_3559.CR2";
            //const string Bitmap = Folder + "IMG_3559.BMP";

            ProcessFile(FileName2, Bitmap);
        }

        private static void ProcessFile(string fileName, string bitmap)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Last();

                var strips = imageFileDirectory.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE
                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();

                var address = imageFileDirectory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var lossless = startOfImage.StartOfFrame;

                var rawSize = address + length - binaryReader.BaseStream.Position - 2;
                // Assert.AreEqual(23852858, rawSize); // RawSize (Raw = new byte[RawSize]
                startOfImage.ImageData = new ImageData(binaryReader, (uint)rawSize);

                var colors = lossless.Components.Sum(comp => comp.HFactor * comp.VFactor);
                var table0 = startOfImage.HuffmanTable.Tables[0x00];

                // var buffer = new byte[rawSize];
                using (var image1 = new Bitmap(500, 500))
                {
                    for (var jrow = 0; jrow < lossless.ScanLines; jrow++)
                    {
                        var rowBuf = new ushort[lossless.SamplesPerLine * colors];
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

                            DumpPixel(jcol, jrow, rowBuf, colors, image1);
                        }
                        // var pp = startOfImage.ImageData.Index;
                    }

                    image1.Save(bitmap);
                }

                // Assert.AreEqual(23852855, startOfImage.ImageData.Index);
                //Console.WriteLine("{0}: ", startOfImage.ImageData.BitsLeft);
                //for (var i = startOfImage.ImageData.Index; i < rawSize - 2; i++)
                //{
                //    Console.WriteLine("{0} ", startOfImage.ImageData.RawData[i].ToString("X2"));
                //}
            }
        }

        private static void DumpPixelDebug(int row, IList<short> rowBuf0, IList<short> rowBuf1)
        {
            const int X = 122; // 2116;
            const int Y = 40; // 1416 / 2;

            var q = row - Y;
            if (q < 0 || q >= 5)
            {
                return;
            }

            for (var p = 0; p < 5; p++)
            {
                var red = rowBuf0[2 * p + X + 0] - 2047;
                var green = rowBuf0[2 * p + X + 1] - 2047;
                var blue = rowBuf1[2 * p + X + 1] - 2047;
                var green2 = rowBuf1[2 * p + X + 0] - 2047;

                Console.WriteLine("{4}, {5}: {0}, {1}, {2}, {3}", red, green, blue, green2, p + 1, q + 1);
            }
        }

        private static void DumpPixel(int jcol, int jrow, ushort[] rowBuf, int colors, Bitmap image1)
        {
            var p = jcol - 50;
            var q = jrow - 30;
            if (p < 0 || p >= 500 || q < 0 || q >= 500)
            {
                return;
            }

            var bits1 = rowBuf[jcol * colors + 0] >> 2;
            if (bits1 > 0xFF)
            {
                bits1 = 0xFF;
            }

            var bits2 = rowBuf[jcol * colors + 1] >> 2;
            if (bits2 > 0xFF)
            {
                bits2 = 0xFF;
            }

            if (jrow % 2 == 0)
            {
                var red = bits1;
                var green = bits2 >> 1;
                var color = Color.FromArgb(red, green, 0);
                image1.SetPixel(p, q, color);
            }
            else
            {
                var green = bits1 >> 1;
                var blue = bits2;
                var color = Color.FromArgb(0, green, blue);
                image1.SetPixel(p, q, color);
            }
        }

        [TestMethod]
        public void SerialNums()
        {
            const uint Cam1 = 3071201378;
            const uint Cam1H = Cam1 & 0xFFFF0000 >> 8;
            const uint Cam1L = Cam1 & 0x0000FFFF;
            Assert.AreEqual("ED00053346", "{0}{1}".FormatWith(Cam1H.ToString("X4"), Cam1L.ToString("D5")));
            // var cam2 = "%04X%05d";
        }

        [TestMethod]
        public void TestMethodD()
        {
            const string Directory = @"..\..\Photos\";
            const string FileName2 = Directory + "5DIIIhigh.CR2";

            using (var fileStream = File.Open(FileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                var ifd0 = rawImage.Directories.First();
                var make = ifd0[0x010f];
                Assert.AreEqual("Canon", RawImage.ReadChars(binaryReader, make));
                var model = ifd0[0x0110];
                // Assert.AreEqual("Canon EOS 7D", RawImage.ReadChars(binaryReader, model));
                Assert.AreEqual("Canon EOS 5D Mark III", RawImage.ReadChars(binaryReader, model));

                var exif = ifd0[0x8769];
                binaryReader.BaseStream.Seek(exif.ValuePointer, SeekOrigin.Begin);
                var tags = new ImageFileDirectory(binaryReader);
                tags.DumpDirectory(binaryReader);

                var makerNotes = tags[0x927C];
                binaryReader.BaseStream.Seek(makerNotes.ValuePointer, SeekOrigin.Begin);
                var notes = new ImageFileDirectory(binaryReader);

                Assert.AreEqual(0x2A, notes.Entries.Length);
                Assert.AreEqual(0u, notes.NextEntry); // last
                var settings = notes[0x0001];
                Console.WriteLine("Camera Settings {0} {1}", settings.ValuePointer, settings.NumberOfValue);
                var focalLength = notes[0x0002];
                Console.WriteLine("Focal Length {0} {1}", focalLength.ValuePointer, focalLength.NumberOfValue);

                binaryReader.BaseStream.Seek(settings.ValuePointer, SeekOrigin.Begin);
                for (var i = 0; i < settings.NumberOfValue; i++)
                {
                    var x = binaryReader.ReadUInt16();
                    Console.WriteLine("{0} : {1}", i, x);
                }

                return;

                var size1 = notes[0x4001];
                Console.WriteLine("0x{0}, {1}, {2}, {3}", size1.TagId.ToString("X4"), size1.TagType, size1.NumberOfValue, size1.ValuePointer);
                var size2 = notes[0x4002];
                Console.WriteLine("0x{0}, {1}, {2}, {3}", size2.TagId.ToString("X4"), size2.TagType, size2.NumberOfValue, size2.ValuePointer);
                var size5 = notes[0x4005];
                Console.WriteLine("0x{0}, {1}, {2}, {3}", size5.TagId.ToString("X4"), size5.TagType, size5.NumberOfValue, size5.ValuePointer);

                var modelId = notes[0x0010];
                // Assert.AreEqual(2147484240, modelId.ValuePointer);
                Assert.AreEqual(2147484293, modelId.ValuePointer);

                var white = notes[0x4001];
                //var whiteData = RawImage.ReadUInts16(binaryReader, white);
                //Assert.AreEqual(1840, whiteData[0x003F]);
                //Assert.AreEqual(1024, whiteData[0x0040]);
                //Assert.AreEqual(1024, whiteData[0x0041]);
                //Assert.AreEqual(1956, whiteData[0x0042]);
                Console.WriteLine("Size {0}", white.NumberOfValue);
                var wb = new WhiteBalance(binaryReader, white);

                // rawImage.DumpHeader(binaryReader);
                //model ID from Makernotes, Tag #0x10
                //white balance information is taken from tag #0x4001

                var ifd3 = rawImage.Directories.Skip(2).First();
                //StripOffset, offset to RAW data : tag #0x111
                //StripByteCount, length of RAW data : tag #0x117
                //image slice layout (cr2_slice[]) : tag #0xc640
                //the RAW image dimensions is taken from lossless jpeg (0xffc3 section)            
            }
        }

        [TestMethod]
        public void TestMethodTags()
        {
            const string Directory = @"..\..\Photos\";
            const string FileName2 = Directory + "5DIIIhigh.CR2";

            using (var fileStream = File.Open(FileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                var ifd0 = rawImage.Directories.First();
                var make = ifd0[0x010f];
                Assert.AreEqual("Canon", RawImage.ReadChars(binaryReader, make));
                var model = ifd0[0x0110];
                // Assert.AreEqual("Canon EOS 7D", RawImage.ReadChars(binaryReader, model));
                Assert.AreEqual("Canon EOS 5D Mark III", RawImage.ReadChars(binaryReader, model));

                var exif = ifd0[0x8769];
                binaryReader.BaseStream.Seek(exif.ValuePointer, SeekOrigin.Begin);
                var tags = new ImageFileDirectory(binaryReader);

                var makerNotes = tags[0x927C];
                binaryReader.BaseStream.Seek(makerNotes.ValuePointer, SeekOrigin.Begin);
                var notes = new ImageFileDirectory(binaryReader);

                var white = notes[0x4001];
                Console.WriteLine("0x{0}, {1}, {2}, {3}", white.TagId.ToString("X4"), white.TagType, white.NumberOfValue, white.ValuePointer);
                // var wb = new WhiteBalance(binaryReader, white);
                ReadSomeData(binaryReader, white.ValuePointer);

                var size2 = notes[0x4002];
                Console.WriteLine("0x{0}, {1}, {2}, {3}", size2.TagId.ToString("X4"), size2.TagType, size2.NumberOfValue, size2.ValuePointer);
                ReadSomeData(binaryReader, size2.ValuePointer);

                var size5 = notes[0x4005];
                Console.WriteLine("0x{0}, {1}, {2}, {3}", size5.TagId.ToString("X4"), size5.TagType, size5.NumberOfValue, size5.ValuePointer);
                ReadSomeData(binaryReader, size5.ValuePointer);
            }
        }

        private static void ReadSomeData(BinaryReader binaryReader, uint valuePointer)
        {
            if (binaryReader.BaseStream.Position != valuePointer)
            {
                binaryReader.BaseStream.Seek(valuePointer, SeekOrigin.Begin);
            }

            var ar = 0;
            var length = binaryReader.ReadUInt16();
            Console.WriteLine("0x{0} Len = {1} Length", ar.ToString("X4"), length);
            ar += 2;
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
