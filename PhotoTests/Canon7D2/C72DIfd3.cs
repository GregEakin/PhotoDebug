// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		C7D2Ifd3.cs
// AUTHOR:		Greg Eakin

using System.Linq;

namespace PhotoTests.Canon7D2
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PhotoLib.Tiff;

    [TestClass]
    public class C7D2Ifd3
    {
        //private const string FileName = @"C:..\..\Photos\7D2high.CR2";
        private const string FileName = @"D:\Users\Greg\Pictures\2018-10-11\0L2A4224.CR2";


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
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                CollectionAssert.AreEqual(new byte[] { 0x49, 0x49 }, rawImage.Header.ByteOrder);
                Assert.AreEqual(0x002A, rawImage.Header.TiffMagic);
                Assert.AreEqual(0x5243, rawImage.Header.CR2Magic);
                CollectionAssert.AreEqual(new byte[] { 0x02, 0x00 }, rawImage.Header.CR2Version);

                rawImage.DumpHeader(binaryReader);
            }
        }

        //== Tiff Directory [0x0000BF46]:
        // 0)  0x0103 UShort 16-bit: 6
        // 1)  0x0111 ULong 32-bit: 3213024
        // 2)  0x0117 ULong 32-bit: 22286138
        // 3)  0xC5D8 ULong 32-bit: 1
        // 4)  0xC5E0 ULong 32-bit: 3
        // 5)  0xC640 UShort 16-bit: [0x0000BFA0] (3): 2, 1728, 1904, 
        // 6)  0xC6C5 ULong 32-bit: 1

        [TestMethod]
        public void Compression()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x0103 UShort 16-bit: 6
                // var imageFileDirectory = rawImage[0x0000BF46];
                var imageFileDirectory = rawImage.Directories.Last();
                var imageFileEntry = imageFileDirectory[0x0103];
                Assert.AreEqual(3, imageFileEntry.TagType);
                Assert.AreEqual(6u, imageFileEntry.ValuePointer);
                Assert.AreEqual(1u, imageFileEntry.NumberOfValue);
            }
        }

        [TestMethod]
        public void StripOffset()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x0111 ULong 32-bit: 3213024
                // var imageFileDirectory = rawImage[0x0000BF46];
                var imageFileDirectory = rawImage.Directories.Last();
                var imageFileEntry = imageFileDirectory[0x0111];
                Assert.AreEqual(0x0111, imageFileEntry.TagId);
                Assert.AreEqual(4, imageFileEntry.TagType);
                // Assert.AreEqual(3213024u, imageFileEntry.ValuePointer);
                Assert.AreEqual(1u, imageFileEntry.NumberOfValue);
            }
        }

        [TestMethod]
        public void StripByteCounts()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x0117 ULong 32-bit: 22286138 
                // var imageFileDirectory = rawImage[0x0000BF46];
                var imageFileDirectory = rawImage.Directories.Last();
                var imageFileEntry = imageFileDirectory[0x0117];
                Assert.AreEqual(0x0117, imageFileEntry.TagId);
                Assert.AreEqual(4, imageFileEntry.TagType);
                // Assert.AreEqual(22286138u, imageFileEntry.ValuePointer);
                Assert.AreEqual(1u, imageFileEntry.NumberOfValue);
            }
        }

        [TestMethod]
        public void Cr2Slice()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0xC640 UShort 16-bit: [0x0000BFA0] (3): 2, 1728, 1904, 
                // var imageFileDirectory = rawImage[0x0000BF46];
                var imageFileDirectory = rawImage.Directories.Last();
                var imageFileEntry = imageFileDirectory[0xC640];
                Assert.AreEqual(3, imageFileEntry.TagType);
                Assert.AreEqual(0x00007294u, imageFileEntry.ValuePointer);
                Assert.AreEqual(3u, imageFileEntry.NumberOfValue);

                CollectionAssert.AreEqual(new[] { (ushort)1, (ushort)2784, (ushort)2784 },
                    RawImage.ReadUInts16(binaryReader, imageFileEntry));
            }
        }
    }
}