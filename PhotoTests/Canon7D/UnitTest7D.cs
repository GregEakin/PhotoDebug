namespace PhotoTests.Canon7D
{
    using System;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Jpeg;
    using PhotoLib.Tiff;

    [TestClass]
    public class UnitTest7D
    {
        #region Constants

        private const string FileName = @"..\..\Photos\7Dhigh.CR2";

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
        public void RawImageDumpData()
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
        public void RawImageSize()
        {
            // 1 Sensor Width                    : 5360 = 1340 * 4 = 2 * 1728 + 1904
            // 2 Sensor Height                   : 3516

            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = directory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                var strips = directory.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE

                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();
                Assert.AreEqual(2, x);
                Assert.AreEqual(1728, y);
                Assert.AreEqual(1904, z);

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var lossless = startOfImage.StartOfFrame;
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
                var address = directory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = directory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var lossless = startOfImage.StartOfFrame;
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
                var address = directory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = directory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var lossless = startOfImage.StartOfFrame;

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
                var address = directory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = directory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                Assert.AreEqual(1, startOfImage.StartOfScan.Bb1);   // Do nothing
            }
        }

        [TestMethod]
        public void DumpRawImageHex()
        {
            // 1 Sensor Width                    : 5360
            // 2 Sensor Height                   : 3516

            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.Single(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = directory.Entries.Single(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                DumpBlock(binaryReader, address, length, 256);

                address = address + length - 64;
                DumpBlock(binaryReader, address, length, 64);
            }
        }

        private static void DumpBlock(BinaryReader binaryReader, uint address, uint length, uint size)
        {
            const int Width = 16;
            binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
            for (var i = 0; i < size; i += Width)
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
        }

        [TestMethod]
        public void TestMethod6()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
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
                Assert.AreEqual(0xFF, startOfImage.Mark);
                Assert.AreEqual(0xD8, startOfImage.Tag); // JPG_MARK_SOI

                var huffmanTable = startOfImage.HuffmanTable;
                Assert.AreEqual(0xFF, huffmanTable.Mark);
                Assert.AreEqual(0xC4, huffmanTable.Tag);

                // This file has two huffman tables: 0x00 and 0x01
                Assert.AreEqual(2, huffmanTable.Tables.Count);
                Assert.IsTrue(huffmanTable.Tables.ContainsKey(0x00));
                Assert.IsTrue(huffmanTable.Tables.ContainsKey(0x01));

                var lossless = startOfImage.StartOfFrame;
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

        #endregion
    }
}
