namespace PhotoTests
{
    using System;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib;


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
        public void SwapTest1()
        {
            var y = SwapBytes(0x55AA);
            Assert.AreEqual((ushort)0xAA55, y);
        }

        [TestMethod]
        public void SwapTest2()
        {
            var y = SwapBytes(0xAA55);
            Assert.AreEqual((ushort)0x55AA, y);
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
                var length = dir.Length;
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
            const string TestFile = @"C:\Users\Greg\Pictures\Oops.jpg";
            // 1 Sensor Width                    : 5360
            // 2 Sensor Height                   : 3516

            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.First(e => e.TagId == 0x0111).ValuePointer;
                var length = directory.Entries.First(e => e.TagId == 0x0117).ValuePointer;

                var strips = directory.Entries.First(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer;
                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();

                var width = x * y + z;
                using (var outfile = File.Create(TestFile))
                {
                    var binaryWriter = new BinaryWriter(outfile);
                    binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                    for (var i = 0; i < length; i += width)
                    {
                        var nextStep = (int)Math.Min(width, length - i);
                        var data = binaryReader.ReadBytes(nextStep);
                        binaryWriter.Write(data);
                    }
                }
            }

            Assert.IsTrue(File.Exists(TestFile));
        }

        [TestMethod]
        public void TestMethod4()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var directory = rawImage.Directories.Last();

                var compression = directory.Entries.First(e => e.TagId == 0x0103 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(6u, compression);

                // 1 Sensor Width                    : 5360
                // 2 Sensor Height                   : 3516

                // var address = directory.Entries.First(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                // var length = directory.Entries.First(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                var strips = directory.Entries.First(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer;

                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();
                Assert.AreEqual(5360, x * y + z);
                // cr2_slices[0]*cr2_slices[1] + cr2_slices[2] = image_width.
            }
        }

        [TestMethod]
        public void TestMethod5()
        {
            const string TestFile = @"C:\Users\Greg\Pictures\Oops.jpg";
            // 1 Sensor Width                    : 5360
            // 2 Sensor Height                   : 3516

            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.First(e => e.TagId == 0x0111).ValuePointer;
                var length = directory.Entries.First(e => e.TagId == 0x0117).ValuePointer;

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
            const string TestFile = @"C:\Users\Greg\Pictures\Oops.jpg";
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.First(e => e.TagId == 0x0111).ValuePointer;
                var length = directory.Entries.First(e => e.TagId == 0x0117).ValuePointer;

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);

                // FF D8 Start of Image - w/o data segment
                var b1 = binaryReader.ReadUInt16();
                b1 = SwapBytes(b1);
                Assert.AreEqual(0xFFD8, b1);

                // FF C4 Define Huffman Table(s)
                var b2 = binaryReader.ReadUInt16();
                b2 = SwapBytes(b2);
                Assert.AreEqual(0xFFC4, b2);

                // 00 42 Length of DHT marker segment (-2)
                var b3 = binaryReader.ReadUInt16();
                b3 = SwapBytes(b3);
                Assert.AreEqual(0x0042, b3);

                var size = (b3 - 2) / 2;
                Assert.AreEqual(32, size);

                for (var i = 0; i < 2; i++)
                {
                    // Read 32 bytes, table 0 -- bits
                    var b4 = binaryReader.ReadByte();
                    Assert.AreEqual(i, b4);

                    var sum = 0;
                    for (var j = 0; j < 16; j++)
                    {
                        sum += binaryReader.ReadByte();
                    }
                    Assert.AreEqual(15, sum);

                    for (var j = 0; j < sum; j++)
                    {
                        binaryReader.ReadByte();
                    }
                }

                // FF C3 Lossless (sequential)
                var b8 = binaryReader.ReadUInt16();
                b8 = SwapBytes(b8);
                Assert.AreEqual(0xFFC3, b8);

                var b9 = binaryReader.ReadUInt16();
                b9 = SwapBytes(b9);
                Assert.AreEqual(0x0014, b9);

                var precision = binaryReader.ReadByte();
                Assert.AreEqual(14, precision);

                var scanLines = binaryReader.ReadUInt16();
                scanLines = SwapBytes(scanLines);
                Assert.AreEqual(3516, scanLines);

                var samplesPerLine = binaryReader.ReadUInt16();
                samplesPerLine = SwapBytes(samplesPerLine);
                Assert.AreEqual(1340, samplesPerLine);

                var componentCount = binaryReader.ReadByte();
                Assert.AreEqual(4, componentCount);

                var width = samplesPerLine * componentCount;
                Assert.AreEqual(5360, width);

                for (var i = 0; i < componentCount; i++)
                {
                    var compId = binaryReader.ReadByte();
                    var sampleFactors = binaryReader.ReadByte();
                    var qTableId = binaryReader.ReadByte();

                    Assert.AreEqual(i + 1, compId);
                    Assert.AreEqual(0x11, sampleFactors);
                    Assert.AreEqual(0x00, qTableId);
                    // var sampleHFactor = (byte)(sampleFactors >> 4);
                    // var sampleVFactor = (byte)(sampleFactors & 0x0f);
                    // frame.AddComponent(compId, sampleHFactor, sampleVFactor, qTableId);
                }

                // FF DA Start of Scan
                var bA = binaryReader.ReadUInt16();
                bA = SwapBytes(bA);
                Assert.AreEqual(0xFFDA, bA);

                // Length
                var bB = binaryReader.ReadUInt16();
                bB = SwapBytes(bB);
                Assert.AreEqual(0x000E, bB);

                var bBnum = binaryReader.ReadByte();
                Assert.AreEqual(4, bBnum);
                var components = new byte[bBnum];
                for (var i = 0; i < bBnum; i++)
                {
                    var id = binaryReader.ReadByte();
                    var info = binaryReader.ReadByte();
                    var dc = (info >> 4) & 0x0f;
                    var ac = info & 0x0f;
                    components[i] = id;
                    // id, acTables[ac], dcTables[dc]

                    Assert.AreEqual(i + 1, id);
                }
                var bB1 = binaryReader.ReadByte();  // startSpectralSelection
                var bB2 = binaryReader.ReadByte();  // endSpectralSelection
                var bB3 = binaryReader.ReadByte();  // successiveApproximation

                // FE D5 
                var bC = binaryReader.ReadUInt16();
                bC = SwapBytes(bC);
                Assert.AreEqual(0xFED5, bC);

                var imageSize = width * scanLines;
                Assert.AreEqual(18845760, imageSize);

                var pos = binaryReader.BaseStream.Position - 2;
                var rawSize = address + length - pos;
                Assert.AreEqual(22286030, rawSize);
                // var imageData = new ushort[rawSize];

                binaryReader.BaseStream.Seek(pos, SeekOrigin.Begin);
                var rawData = binaryReader.ReadBytes((int)rawSize);
                Assert.AreEqual(0xFE, rawData[0]);
                Assert.AreEqual(0xD5, rawData[1]);
                Assert.AreEqual(0x5F, rawData[2]);
                Assert.AreEqual(0xBD, rawData[3]);

                Assert.AreEqual(0xB6, rawData[rawData.Length - 4]);
                Assert.AreEqual(0xD1, rawData[rawData.Length - 3]);
                Assert.AreEqual(0xFF, rawData[rawData.Length - 2]);
                Assert.AreEqual(0xD9, rawData[rawData.Length - 1]);

                // GetBits(rawData);
                // GetLosslessJpgRow(null, rawData, TB0, TL0, TB1, TL1, Prop);

                // for (var iRow = 0; iRow < height; iRow++)
                {
                    // var rowBuf = new ushort[width];
                    // GetLosslessJpgRow(rowBuf, rawData, TL0, TB0, TL1, TB1, Prop);
                    // PutUnscrambleRowSlice(rowBuf, imageData, iRow, Prop);
                }
            }
        }

        [TestMethod]
        public void TestMethod7()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.First(e => e.TagId == 0x0111).ValuePointer;

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);

                // FF D8 Start of Image - w/o data segment
                var b1 = binaryReader.ReadUInt16();
                b1 = SwapBytes(b1);
                Assert.AreEqual(0xFFD8, b1);

                // FF C4 Define Huffman Table(s)
                var b2 = binaryReader.ReadUInt16();
                b2 = SwapBytes(b2);
                Assert.AreEqual(0xFFC4, b2);

                Huffman(binaryReader);
            }
        }

        #endregion

        #region Methods

        private static
        ushort SwapBytes(ushort data)
        {
            var upper = (data & (ushort)0x00FF) << 8;
            var lower = (data & (ushort)0xFF00) >> 8;
            return (ushort)(lower | upper);
        }

        private static void Huffman(BinaryReader jpegReader)
        {
            //var dcTables = new JpegHuffmanTable[4];
            //var acTables = new JpegHuffmanTable[4];

            // DHT non-SOF Marker - Huffman Table is required for decoding
            // the JPEG stream, when we receive a marker we load in first
            // the table length (16 bits), the table class (4 bits), table
            // identifier (4 bits), then we load in 16 bytes and each byte
            // represents the count of bytes to load in for each of the 16
            // bytes. We load this into an array to use later and move on 4
            // huffman tables can only be used in an image.
            var x = jpegReader.ReadUInt16();
            x = SwapBytes(x);
            var huffmanLength = (x - 2);

            // Keep looping until we are out of length.
            var index = huffmanLength;

            // Multiple tables may be defined within a DHT marker. This
            // will keep reading until there are no tables left, most
            // of the time there are just one tables.
            while (index > 0)
            {
                // Read the identifier information and class
                // information about the Huffman table, then read the
                // 16 byte codelength in and read in the Huffman values
                // and put it into table info.
                var huffmanInfo = jpegReader.ReadByte();
                var tableClass = (byte)(huffmanInfo >> 4);
                var huffmanIndex = (byte)(huffmanInfo & 0x0f);
                var codeLength = new short[16];

                for (var i = 0; i < codeLength.Length; i++)
                    codeLength[i] = jpegReader.ReadByte();

                var huffmanValueLen = 0;
                for (var i = 0; i < 16; i++)
                    huffmanValueLen += codeLength[i];
                index -= (huffmanValueLen + 17);

                var huffmanVal = new short[huffmanValueLen];
                for (var i = 0; i < huffmanVal.Length; i++)
                {
                    huffmanVal[i] = jpegReader.ReadByte();
                }

                //                // Assign DC Huffman Table.
                //                if (tableClass == HuffmanTable.JPEG_DC_TABLE)
                //                    dcTables[(int)huffmanIndex] = new JpegHuffmanTable(codeLength, huffmanVal);
                //
                //                // Assign AC Huffman Table.
                //                else if (tableClass == HuffmanTable.JPEG_AC_TABLE)
                //                    acTables[(int)huffmanIndex] = new JpegHuffmanTable(codeLength, huffmanVal);
            }
        }

        #endregion
    }
}