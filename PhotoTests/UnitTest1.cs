namespace PhotoTests
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Jpeg;
    using PhotoLib.Tiff;

    [TestClass]
    public class UnitTest1
    {
        #region Constants

        private const string FileName = @"C:\Users\Greg\Pictures\IMG_0511.CR2";

        #endregion

        #region Public Methods and Operators

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            if (!File.Exists(FileName))
            {
                throw new ArgumentException("{0} doesn't exists!", FileName);
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                CollectionAssert.AreEqual(new byte[] { 0x49, 0x49 }, rawImage.Header.ByteOrder);
                Assert.AreEqual(0x002A, rawImage.Header.TiffMagic);
                Assert.AreEqual(0x5243, rawImage.Header.CR2Magic);
                CollectionAssert.AreEqual(new byte[] { 0x02, 0x00 }, rawImage.Header.CR2Version);

                rawImage.DumpHeader(binaryReader);
            }
        }

        [TestMethod]
        public void TestMethod2()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var dir = rawImage.Directories.First();
                var length = dir.Entries.Length;
                Console.WriteLine("Entries = {0}", length);
                for (var i = 0; i < length; i++)
                {
                    Console.WriteLine(
                        "{0}: {1} {2} {3} {4}",
                        i,
                        dir.Entries[i].TagId,
                        dir.Entries[i].TagType,
                        dir.Entries[i].NumberOfValue,
                        dir.Entries[i].ValuePointer);
                }
                Console.WriteLine("Next offset {0}", dir.NextEntry);
            }
        }

        [TestMethod]
        public void TestMethod3()
        {
            // 1 Sensor Width                    : 5360 = 1340 * 4 = 2 * 1728 + 1904
            // 2 Sensor Height                   : 3516

            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = directory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                var strips = directory.Entries.First(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE

                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var lossless = startOfImage.Lossless;
                Assert.AreEqual(14, lossless.Precision);
                Assert.AreEqual(4, lossless.Components.Length);
                Assert.AreEqual(1340, lossless.SamplesPerLine);
                Assert.AreEqual(3516, lossless.ScanLines);
            }
        }

        [TestMethod]
        public void Bits()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = directory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var lossless = startOfImage.Lossless;
                Assert.AreEqual(14, lossless.Precision);

                var startOfScan = startOfImage.StartOfScan;
                Assert.AreEqual(0, startOfScan.Bb3 & 0x0F);

                Assert.AreEqual(14, lossless.Precision - (startOfScan.Bb3 & 0x0f));
            }
        }

        [TestMethod]
        public void Colors()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = directory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var lossless = startOfImage.Lossless;

                Assert.AreEqual(4, lossless.Components.Length); // clrs
                foreach (var component in lossless.Components)
                {
                    Assert.AreEqual(1, component.HFactor);  // sraw
                    Assert.AreEqual(1, component.VFactor);  // sraw
                }

                Assert.AreEqual(4, lossless.Components.Sum(comp => comp.HFactor * comp.VFactor));
            }
        }

        [TestMethod]
        public void PredictorSelectionValue()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = directory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                Assert.AreEqual(1, startOfImage.StartOfScan.Bb1);   // Do nothing
            }
        }

        [TestMethod]
        public void TestMethod5()
        {
            // 1 Sensor Width                    : 5360
            // 2 Sensor Height                   : 3516

            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = directory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS

                const int Width = 16;
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                for (var i = 0; i < 256; i += Width)
                {
                    Console.Write("0x{0}: ", (address + i).ToString("X8"));
                    var nextStep = (int)Math.Min(Width, length - i);
                    var data = binaryReader.ReadBytes(nextStep);
                    foreach (var b in data)
                    {
                        Console.Write("{0} ", b.ToString("X2"));
                    }
                    Console.WriteLine();
                }
                Console.WriteLine("...");

                address = address + length - 64;
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                for (var i = 0; i < 64; i += Width)
                {
                    Console.Write("0x{0}: ", (address + i).ToString("X8"));
                    var nextStep = (int)Math.Min(Width, length - i);
                    var data = binaryReader.ReadBytes(nextStep);
                    foreach (var b in data)
                    {
                        Console.Write("{0} ", b.ToString("X2"));
                    }
                    Console.WriteLine();
                }
            }
        }

        [TestMethod]
        public void TestMethod6()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
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
                Assert.AreEqual(0xFF, startOfImage.Mark);
                Assert.AreEqual(0xD8, startOfImage.Tag); // JPG_MARK_SOI

                var huffmanTable = startOfImage.HuffmanTable;
                Assert.AreEqual(0xFF, huffmanTable.Mark);
                Assert.AreEqual(0xC4, huffmanTable.Tag);

                // This file has two huffman tables: 0x00 and 0x01
                Assert.IsTrue(huffmanTable.Tables.ContainsKey(0x00));
                Assert.IsTrue(huffmanTable.Tables.ContainsKey(0x01));

                var lossless = startOfImage.Lossless;
                Assert.AreEqual(0xFF, lossless.Mark);
                Assert.AreEqual(0xC3, lossless.Tag);

                Assert.AreEqual(14, lossless.Precision);
                Assert.AreEqual(4, lossless.Components.Length);
                Assert.AreEqual(1340, lossless.SamplesPerLine);
                Assert.AreEqual(3516, lossless.ScanLines);

                Assert.AreEqual(5360, lossless.Width);  // Sensor width (bits)
                Assert.AreEqual(5360, lossless.SamplesPerLine * lossless.Components.Length);
                Assert.AreEqual(5360, x * y + z);

                foreach (var component in lossless.Components)
                {
                    // Console.WriteLine("== {0}: {1} {2} {3}", component.ComponentId, component.HFactor, component.VFactor, component.TableId);
                    Assert.AreEqual(0x01, component.HFactor);
                    Assert.AreEqual(0x01, component.VFactor);
                    Assert.AreEqual(0x00, component.TableId);
                }

                var startOfScan = startOfImage.StartOfScan;
                Assert.AreEqual(0xFF, startOfScan.Mark);
                Assert.AreEqual(0xDA, startOfScan.Tag);

                foreach (var scanComponent in startOfScan.Components)
                {
                    Console.WriteLine("{0}: {1} {2}", scanComponent.Id, scanComponent.Dc, scanComponent.Ac);
                }

                var imageData = startOfImage.ImageData;


            }
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        public void TestMethodB6()
        {
            const string Folder = @"C:\Users\Greg\Documents\Visual Studio 2012\Projects\PhotoDebug\Samples\";
            const string FileName2 = Folder + "IMG_0503.CR2";
            ProcessFile(FileName2);
        }

        [TestMethod]
        public void TestMethodBitmap()
        {
            const string Folder = @"C:\Users\Greg\Documents\Visual Studio 2012\Projects\PhotoDebug\Samples\";
            const string FileName2 = Folder + "IMG_0503.CR2";
            MakeBitMap(FileName2);
        }

        private static void LittleMakeBitMap(string fileName)
        {
            const int X = 160;
            const int Y = 160;
            const int Colors = 4;

            var bitmap = new Bitmap(X * Colors, Y, PixelFormat.Format48bppRgb);
            var dimension = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var picData = bitmap.LockBits(dimension, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            var pixelStartAddress = picData.Scan0;
            var bpp = picData.Stride / bitmap.Width;

            for (var jrow = 0; jrow < Y; jrow++)
            {
                var rowBuf = new byte[picData.Stride];

                for (var jcol = 0; jcol < X; jcol++)
                {
                    for (var jcolor = 0; jcolor < Colors; jcolor++)
                    {
                        var index = (jcol * Colors + jcolor) * bpp;

                        rowBuf[index + 0] = 0x00;
                        rowBuf[index + 1] = 0x00;
                        rowBuf[index + 2] = 0xff;
                        rowBuf[index + 3] = 0x7f;
                        rowBuf[index + 4] = 0xff;
                        rowBuf[index + 5] = 0xff;
                    }
                }

                var offset = jrow * picData.Stride;
                System.Runtime.InteropServices.Marshal.Copy(rowBuf, 0, pixelStartAddress + offset, rowBuf.Length);
            }
            bitmap.UnlockBits(picData);
            bitmap.Save(fileName + ".bmp");
        }

        private static void MakeBitMap(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Last();

                var strips = imageFileDirectory.Entries.First(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE
                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x1 = binaryReader.ReadUInt16();
                var y1 = binaryReader.ReadUInt16();
                var z1 = binaryReader.ReadUInt16();

                var address = imageFileDirectory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var lossless = startOfImage.Lossless;

                var rawSize = address + length - binaryReader.BaseStream.Position - 2;
                startOfImage.ImageData = new ImageData(binaryReader, (uint)rawSize);

                var colors = lossless.Components.Sum(comp => comp.HFactor * comp.VFactor);

                var table0 = startOfImage.HuffmanTable.Tables[0x00];

                var bitmap = new Bitmap(lossless.SamplesPerLine * colors, lossless.ScanLines, PixelFormat.Format48bppRgb);

                var dimension = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var picData = bitmap.LockBits(dimension, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                var pixelStartAddress = picData.Scan0;
                var bpp = picData.Stride / bitmap.Width;

                for (var jrow = 0; jrow < lossless.ScanLines; jrow++)
                {
                    var rowBuf = new byte[picData.Stride];
                    for (var jcol = 0; jcol < lossless.SamplesPerLine; jcol++)
                    {
                        for (var jcolor = 0; jcolor < colors; jcolor++)
                        {
                            var val = GetValue(startOfImage.ImageData, table0);
                            var bits = startOfImage.ImageData.GetSetOfBits(val);
                            var index = (jcol * colors + jcolor) * bpp;

                            rowBuf[index + 0] = (byte)(bits & 0xFF);    // Blue
                            rowBuf[index + 1] = (byte)((bits >> 8) & 0xFF);
                            rowBuf[index + 2] = 0x00; // (byte)(bits >> 8);      // Green
                            rowBuf[index + 3] = 0x00; // (byte)(bits & 0xFF);
                            rowBuf[index + 4] = 0x00;                   // Red
                            rowBuf[index + 5] = 0x00;  //High byte
                        }
                    }

                    var offset = jrow * rowBuf.Length;
                    System.Runtime.InteropServices.Marshal.Copy(rowBuf, 0, pixelStartAddress + offset, rowBuf.Length);
                }

                bitmap.UnlockBits(picData);
                bitmap.Save(fileName + ".bmp");
            }
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

                //for (var j = 0; j < lossless.ScanLines; j++)
                //    for (var i = 0; i < lossless.SamplesPerLine; i += 2)
                //    {
                //        var hufCode0 = GetValue(startOfImage, table0);
                //        var difCode0 = startOfImage.ImageData.GetSetOfBits(hufCode0);
                //        var dif0 = DecodeDifBits(difCode0, hufCode0);

                //        var hufCode1 = GetValue(startOfImage, table1);
                //        var difCode1 = startOfImage.ImageData.GetSetOfBits(hufCode1);
                //        var dif1 = DecodeDifBits(difCode1, hufCode1);

                //        if (i == 0)
                //        {
                //            rowBuf[0] = predictor[0] += dif0;
                //            rowBuf[1] = predictor[1] += dif1;
                //        }
                //        else
                //        {
                //            rowBuf[i + 0] = (short)(rowBuf[i - 2] + dif0);
                //            rowBuf[i + 1] = (short)(rowBuf[i - 1] + dif1);
                //        }

                //        var cr2Cols = lossless.SamplesPerLine;
                //        var cr2Slice = cr2Cols / 2;
                //        var cr2QuadRows = lossless.ScanLines / 2;
                //        var ibStart = j < cr2QuadRows
                //            ? 2 * j * cr2Cols
                //            : 2 * (j - cr2QuadRows) * cr2Cols + cr2Slice;
                //        Array.Copy(rowBuf, 0, ib, ibStart, cr2Slice);
                //        Array.Copy(rowBuf, cr2Slice, ib, ibStart + cr2Slice, cr2Slice);
                //    }
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
                hufIndex = imageData.GetNextBit(hufIndex);
                hufBits++;
            }
            while (!table.Dictionary.TryGetValue(hufIndex, out hCode) || hCode.Length != hufBits);

            return hCode.Code;
        }

        #endregion
    }
}