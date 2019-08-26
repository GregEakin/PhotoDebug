// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		C5D3Ifd3.cs
// AUTHOR:		Greg Eakin

using System.Linq;

namespace PhotoTests.Canon5D3
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PhotoLib.Tiff;

    [TestClass]
    public class C5D3Ifd3
    {
        private const string FileName = @"d:\Users\Greg\Pictures\2018-08-29\0L2A3743.CR2";

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Assert.IsTrue(File.Exists(FileName), "Image file {0} doesn't exists!", FileName);
        }

        //== Tiff Directory [0x00011964]:
        //0)  0x0103 UShort 16-bit: 6
        //1)  0x0111 ULong 32-bit: 4223344
        //2)  0x0117 ULong 32-bit: 25591542
        //3)  0xC5D8 ULong 32-bit: 1
        //4)  0xC5E0 ULong 32-bit: 1
        //5)  0xC640 UShort 16-bit: [0x000119BE] (3): 1, 2960, 2960, 
        //6)  0xC6C5 ULong 32-bit: 1

        [TestMethod]
        public void DumpImageFileDirectory()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Skip(3).First();
                imageFileDirectory.DumpDirectory(binaryReader);
            }
        }

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

                // 0x0111 ULong 32-bit: 4223344
                var imageFileDirectory = rawImage.Directories.Last();
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
                var imageFileDirectory = rawImage.Directories.Last();
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
                var imageFileDirectory = rawImage.Directories.Last();
                var imageFileEntry = imageFileDirectory[0xC640];
                Assert.AreEqual(3, imageFileEntry.TagType);
                Assert.AreEqual(0x000119BEu, imageFileEntry.ValuePointer);
                Assert.AreEqual(3u, imageFileEntry.NumberOfValue);

                CollectionAssert.AreEqual(new[] { (ushort)1, (ushort)2960, (ushort)2960 },
                    RawImage.ReadUInts16(binaryReader, imageFileEntry));
            }
        }
    }
}