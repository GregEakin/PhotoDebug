// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		C5D3Ifd3.cs
// AUTHOR:		Greg Eakin

using System.Linq;

namespace PhotoTests.CanonM5
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PhotoLib.Tiff;

    // The first IFD contains a small RGB version of the picture (one fourth the size) compressed in Jpeg, the EXIF part, and the Makernotes part. 
    // The second IFD contains a small RGB version (160x120 pixels) of the picture, compressed in Jpeg.
    // The third IFD contains a small RGB version of the picture, NOT compressed (even with compression==6), and one which no white balance, correction has been applied.
    // The fourth IFD contains the RAW data compressed in lossless Jpeg. 

    [TestClass]
    public class CM5Ifd3
    {
        private const string FileName = @"C:..\..\..\Samples\IMG_0012.CR2";

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

        //== Tiff Directory [0x0000B50C]:
        //0)  0x0103 UShort 16-bit: 6
        //1)  0x0111 ULong 32-bit: 8104448
        //2)  0x0117 ULong 32-bit: 29690604
        //3)  0xC5D8 ULong 32-bit: 1
        //4)  0xC5E0 ULong 32-bit: 1
        //5)  0xC640 UShort 16-bit: [0x000119BE] (3): 0, 0, 6288, 
        //6)  0xC6C5 ULong 32-bit: 1

        [TestMethod]
        public void Compression()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x0103 UShort 16-bit: 6
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

                // 0x0111 ULong 32-bit: 8104448
                var imageFileDirectory = rawImage.Directories.Last();
                var imageFileEntry = imageFileDirectory[0x0111];
                Assert.AreEqual(4, imageFileEntry.TagType);
                Assert.AreEqual(0x0111, imageFileEntry.TagId);
                Assert.AreEqual(8104448u, imageFileEntry.ValuePointer);
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

                // 0x0117 ULong 32-bit: 29690604 
                var imageFileDirectory = rawImage.Directories.Last();
                var imageFileEntry = imageFileDirectory[0x0117];
                Assert.AreEqual(4, imageFileEntry.TagType);
                Assert.AreEqual(0x0117, imageFileEntry.TagId);
                Assert.AreEqual(29690604u, imageFileEntry.ValuePointer);
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

                // 0xC640 UShort 16-bit: [0x0000B566] (3): 0, 0, 6288, 
                var imageFileDirectory = rawImage.Directories.Last();
                var imageFileEntry = imageFileDirectory[0xC640];
                Assert.AreEqual(3, imageFileEntry.TagType);
                Assert.AreEqual(0x0000B566u, imageFileEntry.ValuePointer);
                Assert.AreEqual(3u, imageFileEntry.NumberOfValue);

                CollectionAssert.AreEqual(new[] { (ushort)0, (ushort)0, (ushort)6288 },
                    RawImage.ReadUInts16(binaryReader, imageFileEntry));
            }
        }
    }
}