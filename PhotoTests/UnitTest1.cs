namespace PhotoTests
{
    using System;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib;
    using PhotoLib.Jpeg;

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
                var address = directory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = directory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS

                var strips = directory.Entries.First(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE
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

                var compression = directory.Entries.First(e => e.TagId == 0x0103 && e.TagType == 3).ValuePointer; // TIF_COMPRESSION
                Assert.AreEqual(6u, compression);

                // 1 Sensor Width                    : 5360
                // 2 Sensor Height                   : 3516

                // var address = directory.Entries.First(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                // var length = directory.Entries.First(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                var strips = directory.Entries.First(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE

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

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = directory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS

                StartOfImage(binaryReader, address, length);
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
                var address = directory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);

                // FF D8 Start of Image - w/o data segment
                var b1 = binaryReader.ReadUInt16();
                b1 = SwapBytes(b1);
                Assert.AreEqual(0xFFD8, b1); // JPG_MARK_SOI

                var huffmanTable = new HuffmanTable(binaryReader);
                Assert.AreEqual(0xFF, huffmanTable.Mark);
            }
        }

        #endregion

        #region Methods

        private static void Lossless(BinaryReader binaryReader)
        {
            // FF C3 Lossless (sequential)
            var b8 = binaryReader.ReadUInt16();
            b8 = SwapBytes(b8);
            Assert.AreEqual(0xFFC3, b8); // JPG_MARK_SOF3

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

            //var imageSize = width * scanLines;
            //Assert.AreEqual(18845760, imageSize);

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
        }

        private static void StartOfImage(BinaryReader binaryReader, uint address, uint length)
        {
            binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);

            // FF D8 Start of Image - w/o data segment
            var b1 = binaryReader.ReadUInt16();
            b1 = SwapBytes(b1);
            Assert.AreEqual(0xFFD8, b1); // JPG_MARK_SOI

            var huffmanTable = new HuffmanTable(binaryReader);
            Assert.AreEqual(0xFF, huffmanTable.Mark);
            Assert.AreEqual(0xC4, huffmanTable.Tag);

            Lossless(binaryReader);

            var startOfScan = new StartOfScan(binaryReader);
            Assert.AreEqual(0xFF, startOfScan.Mark);
            Assert.AreEqual(0xDA, startOfScan.Tag);

            var imageData = new ImageData(binaryReader, address, length);
            Assert.AreEqual(0xFE, imageData.Mark);
            Assert.AreEqual(0xD5, imageData.Tag);
        }

        private static ushort SwapBytes(ushort data)
        {
            var upper = (data & (ushort)0x00FF) << 8;
            var lower = (data & (ushort)0xFF00) >> 8;
            return (ushort)(lower | upper);
        }

        #endregion
    }
}