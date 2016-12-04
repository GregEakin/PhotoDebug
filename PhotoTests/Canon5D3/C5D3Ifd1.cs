// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		C5D3Ifd1.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;

namespace PhotoTests.Canon5D3
{
    [TestClass]
    public class C5D3Ifd1
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

        [TestMethod]
        public void ImageWidth()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x0100 UShort 16-bit: 5760
                var imageFileDirectory = rawImage[0x00000010];
                var imageFileEntry = imageFileDirectory[0x0100];
                Assert.AreEqual(3, imageFileEntry.TagType);
                Assert.AreEqual(5760u, imageFileEntry.ValuePointer);
                Assert.AreEqual(1u, imageFileEntry.NumberOfValue);
            }
        }

        [TestMethod]
        public void ImageLength()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x0101 UShort 16-bit: 3840
                var imageFileDirectory = rawImage[0x00000010];
                var imageFileEntry = imageFileDirectory[0x0101];
                Assert.AreEqual(3, imageFileEntry.TagType);
                Assert.AreEqual(3840u, imageFileEntry.ValuePointer);
                Assert.AreEqual(1u, imageFileEntry.NumberOfValue);
            }
        }

        [TestMethod]
        public void BitsPerSample()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x0102 UShort 16-bit: [0x000000EE] (3): 8, 8, 8, 
                var imageFileDirectory = rawImage[0x00000010];
                var imageFileEntry = imageFileDirectory[0x0102];
                Assert.AreEqual(3, imageFileEntry.TagType);
                Assert.AreEqual(238u, imageFileEntry.ValuePointer);
                Assert.AreEqual(3u, imageFileEntry.NumberOfValue);

                CollectionAssert.AreEqual(new[] { (ushort)8, (ushort)8, (ushort)8 },
                    RawImage.ReadUInts16(binaryReader, imageFileEntry));
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
                var imageFileDirectory = rawImage[0x00000010];
                var imageFileEntry = imageFileDirectory[0x0103];
                Assert.AreEqual(3, imageFileEntry.TagType);
                Assert.AreEqual(6u, imageFileEntry.ValuePointer);
                Assert.AreEqual(1u, imageFileEntry.NumberOfValue);
            }
        }

        [TestMethod]
        public void Maker()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x010F Ascii 8-bit: [0x000000F4] (6): Canon
                var imageFileDirectory = rawImage[0x00000010];
                var imageFileEntry = imageFileDirectory[0x010F];
                Assert.AreEqual(2, imageFileEntry.TagType);
                Assert.AreEqual(0x000000F4u, imageFileEntry.ValuePointer);
                Assert.AreEqual(6u, imageFileEntry.NumberOfValue);

                Assert.AreEqual("Canon", RawImage.ReadChars(binaryReader, imageFileEntry));
            }
        }

        [TestMethod]
        public void Model()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x0110 Ascii 8-bit: [0x000000FA] (22): Canon EOS 5D Mark III
                var imageFileDirectory = rawImage[0x00000010];
                var imageFileEntry = imageFileDirectory[0x0110];
                Assert.AreEqual(2, imageFileEntry.TagType);
                Assert.AreEqual(0x000000FAu, imageFileEntry.ValuePointer);
                Assert.AreEqual(22u, imageFileEntry.NumberOfValue);

                Assert.AreEqual("Canon EOS 5D Mark III", RawImage.ReadChars(binaryReader, imageFileEntry));
            }
        }

        // stripOffset 6)  0x0111 ULong 32-bit: 96332
        // orientation 7)  0x0112 UShort 16-bit: 1
        // stripByteCounts 8)  0x0117 ULong 32-bit: 2390306
        // xResolution 9)  0x011A Rational 2x32-bit: [0x0000011A] (2): 72/1 = 72
        // yResolution 10)  0x011B Rational 2x32-bit: [0x00000122] (2): 72/1 = 72
        // resolutionUnit 11)  0x0128 UShort 16-bit: 2, pixels per inch
        // dateTime 12)  0x0132 Ascii 8-bit: [0x0000012A] (20): 2013:07:13 01:10:00
        // 13)  0x013B Ascii 8-bit: [0x0000013E] (11): Greg Eakin

        [TestMethod]
        public void XmpMetadata()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x02BC Byte 8-bit: [0x000119C4] (8192): // XML packet containing XMP metadata
                var imageFileDirectory = rawImage[0x00000010];
                var imageFileEntry = imageFileDirectory[0x02BC];
                Assert.AreEqual(1, imageFileEntry.TagType);
                Assert.AreEqual(0x000119C4u, imageFileEntry.ValuePointer);
                Assert.AreEqual(8192u, imageFileEntry.NumberOfValue);

                var readChars = RawImage.ReadChars(binaryReader, imageFileEntry);

                const string Expected1 =
                    "<?xpacket begin='ï»¿' id='W5M0MpCehiHzreSzNTczkc9d'?><x:xmpmeta xmlns:x=\"adobe:ns:meta/\"><rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"><rdf:Description rdf:about=\"\" xmlns:xmp=\"http://ns.adobe.com/xap/1.0/\"><xmp:Rating>0</xmp:Rating></rdf:Description></rdf:RDF></x:xmpmeta>";
                Assert.AreEqual(Expected1, readChars.Substring(0, 291));
                // lots of white space between these two substrings.
                const string Expected2 = "<?xpacket end='w'?>";
                Assert.AreEqual(Expected2, readChars.Substring(8173));
            }
        }

        // 15)  0x8298 Ascii 8-bit: [0x0000017E] (11): Greg Eakin

        [TestMethod]
        public void ExifTags()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // 0x8769 Image File Directory: [0x000001BE] (1): 
                var imageFileDirectory = rawImage[0x00000010];
                var imageFileEntry = imageFileDirectory[0x8769];
                Assert.AreEqual(4, imageFileEntry.TagType);
                Assert.AreEqual(0x000001BEu, imageFileEntry.ValuePointer);
                Assert.AreEqual(1u, imageFileEntry.NumberOfValue);

                var readULongs = RawImage.ReadUInts(binaryReader, imageFileEntry);
                CollectionAssert.AreEqual(new[] { 0x829a0026 }, readULongs);
            }
        }

        #endregion
    }
}