// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		C5D3Ifd3.cs
// AUTHOR:		Greg Eakin

namespace PhotoTests.Canon5D3
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PhotoLib.Tiff;

    [TestClass]
    public class C5D3Ifd3
    {
        #region Constants

        private const string FileName = @"C:..\..\Photos\5DIIIhigh.CR2";

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

        //== Tiff Direcotry [0x00011964]:
        //0)  0x0103 UShort 16-bit: 6
        //1)  0x0111 ULong 32-bit: 4223344
        //2)  0x0117 ULong 32-bit: 25591542
        //3)  0xC5D8 ULong 32-bit: 1
        //4)  0xC5E0 ULong 32-bit: 1
        //5)  0xC640 UShort 16-bit: [0x000119BE] (3): 1, 2960, 2960, 
        //6)  0xC6C5 ULong 32-bit: 1

        [TestMethod]
        public void Compression()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x0103 UShort 16-bit: 6
                var imageFileDirectory = rawImage[0x00011964];
                var imageFileEntry = imageFileDirectory[0x0103];
                Assert.AreEqual(3, imageFileEntry.TagType);
                Assert.AreEqual(6u, imageFileEntry.ValuePointer);
                Assert.AreEqual(1u, imageFileEntry.NumberOfValue);
            }
        }

        [TestMethod]
        public void StipOffset()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x0111 ULong 32-bit: 4223344
                var imageFileDirectory = rawImage[0x00011964];
                var imageFileEntry = imageFileDirectory[0x0111];
                Assert.AreEqual(4, imageFileEntry.TagType);
                Assert.AreEqual(0x0111, imageFileEntry.TagId);
                // Assert.AreEqual(4223344u, imageFileEntry.ValuePointer);
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

                // 0x0117 ULong 32-bit: 25591542 
                var imageFileDirectory = rawImage[0x00011964];
                var imageFileEntry = imageFileDirectory[0x0117];
                Assert.AreEqual(4, imageFileEntry.TagType);
                Assert.AreEqual(0x0117, imageFileEntry.TagId);
                // Assert.AreEqual(25591542u, imageFileEntry.ValuePointer);
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

                // 0xC640 UShort 16-bit: [0x000119BE] (3): 1, 2960, 2960, 
                var imageFileDirectory = rawImage[0x00011964];
                var imageFileEntry = imageFileDirectory[0xC640];
                Assert.AreEqual(3, imageFileEntry.TagType);
                Assert.AreEqual(0x000119BEu, imageFileEntry.ValuePointer);
                Assert.AreEqual(3u, imageFileEntry.NumberOfValue);

                CollectionAssert.AreEqual(new[] { (ushort)1, (ushort)2960, (ushort)2960 },
                    RawImage.ReadUInts16(binaryReader, imageFileEntry));
            }
        }

        #endregion
    }
}