// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		C5D3Ifd1.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg.JpegTags;
using PhotoLib.Tiff;

namespace PhotoTests.Canon5D3
{
    // The first IFD contains a small RGB version of the picture (one fourth the size) compressed in Jpeg, the EXIF part, and the Makernotes part. 
    // The second IFD contains a small RGB version (160x120 pixels) of the picture, compressed in Jpeg.
    // The third IFD contains a small RGB version of the picture, NOT compressed (even with compression==6), and one which no white balance, correction has been applied.
    // The fourth IFD contains the RAW data compressed in lossless Jpeg. 

    [TestClass]
    public class C5D3Ifd2
    {
        private const string FileName = @"C:..\..\..\Samples\311A6647.CR2";

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            if (!File.Exists(FileName))
                throw new ArgumentException("{0} doesn't exists!", FileName);
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
        public void TestMethod2()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Skip(2).First();
                imageFileDirectory.DumpDirectory(binaryReader);
            }
        }

        // 5)  0x0111 ULong 32-bit: 2794548u   -- Offset
        // 8)  0x0117 ULong 32-bit: 1403040u   -- Length
        [TestMethod]
        public void DumpImage()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Skip(2).First();
                Assert.AreEqual(13, imageFileDirectory.Entries.Length);
                //CollectionAssert.AreEqual(
                //    new ushort[] { 0x0201, 0x0202 },
                //    imageFileDirectory.Entries.Select(e => e.TagId).ToArray());

                var offset = imageFileDirectory.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(2794548u, offset);

                var length = imageFileDirectory.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(1403040u, length);

                binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
                var name = Path.Combine(Path.GetDirectoryName(FileName) ?? "./", Path.GetFileNameWithoutExtension(FileName) + "-2.rgb");
                DumpImage(name, binaryReader, length);
            }
        }

        private static void DumpImage(string output, BinaryReader binaryReader, uint length)
        {
            using (var x = File.Create(output))
            {
                var bytes = (int)length;
                var buffer = new byte[32768];
                int read;
                while (bytes > 0 && (read = binaryReader.BaseStream.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
                {
                    x.Write(buffer, 0, read);
                    bytes -= read;
                }
            }
        }
    }
}