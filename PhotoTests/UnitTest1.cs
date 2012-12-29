namespace PhotoTests
{
    using System;
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

                const int Width = 5360;
                // const int Height = 3516;

                var address = directory.Entries.First(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                var length = directory.Entries.First(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                var strips = directory.Entries.First(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE

                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();
                Assert.AreEqual(Width, x * y + z);

                var startOfImage = new StartOfImage(binaryReader, address, length);


                // binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                // var rawData = new RawData(binaryReader, Height, x, y, z);
                // Assert.AreEqual(length, rawData.Data.Length);
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

                var startOfImage = new StartOfImage(binaryReader, address, length);
                Assert.AreEqual(0xFF, startOfImage.Mark);
                Assert.AreEqual(0xD8, startOfImage.Tag); // JPG_MARK_SOI

                var huffmanTable = startOfImage.HuffmanTable;
                Assert.AreEqual(0xFF, huffmanTable.Mark);
                Assert.AreEqual(0xC4, huffmanTable.Tag);
                huffmanTable.DumpTable();

                var lossless = startOfImage.Lossless;
                Assert.AreEqual(0xFF, lossless.Mark);
                Assert.AreEqual(0xC3, lossless.Tag);

                var startOfScan = startOfImage.StartOfScan;
                Assert.AreEqual(0xFF, startOfScan.Mark);
                Assert.AreEqual(0xDA, startOfScan.Tag);

                var imageData = startOfImage.ImageData;
                Assert.AreEqual(0xFE, imageData.Mark);
                Assert.AreEqual(0xD5, imageData.Tag);
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
                huffmanTable.DumpTable();
            }
        }

        // [TestMethod]
        public void TestMethod8()
        {
            const string Directory = @"C:\Users\Greg\Documents\Visual Studio 2012\Projects\PhotoDebug\Samples\";
            // const string FileName2 = Directory + "huff_simple0.jpg";
            // const string FileName2 = Directory + "IMG_0503.JPG";
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
                huffmanTable.DumpTable();
            }
        }

        #endregion
    }
}