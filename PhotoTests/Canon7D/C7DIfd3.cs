// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		C7DIfd3.cs
// AUTHOR:		Greg Eakin

namespace PhotoTests.Canon7D
{
    using System;
    using System.IO;
    using Xunit;
    using PhotoLib.Tiff;

    
    public class C7DIfd3
    {
        private const string FileName = @"P:\Source\7Dhigh.CR2";

        public C7DIfd3()
        {
            if (!File.Exists(FileName))
            {
                throw new ArgumentException("{0} doesn't exists!", FileName);
            }
        }

        [Fact]
        public void TestMethod1()
        {
            using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var binaryReader = new BinaryReader(fileStream);
            var rawImage = new RawImage(binaryReader);
            Assert.Equal(new byte[] { 0x49, 0x49 }, rawImage.Header.ByteOrder);
            Assert.Equal(0x002A, rawImage.Header.TiffMagic);
            Assert.Equal(0x5243, rawImage.Header.CR2Magic);
            Assert.Equal(new byte[] { 0x02, 0x00 }, rawImage.Header.CR2Version);

            rawImage.DumpHeader(binaryReader);
        }

        //== Tiff Directory [0x0000BF46]:
        // 0)  0x0103 UShort 16-bit: 6
        // 1)  0x0111 ULong 32-bit: 3213024
        // 2)  0x0117 ULong 32-bit: 22286138
        // 3)  0xC5D8 ULong 32-bit: 1
        // 4)  0xC5E0 ULong 32-bit: 3
        // 5)  0xC640 UShort 16-bit: [0x0000BFA0] (3): 2, 1728, 1904, 
        // 6)  0xC6C5 ULong 32-bit: 1

        [Fact]
        public void Compression()
        {
            using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var binaryReader = new BinaryReader(fileStream);
            var rawImage = new RawImage(binaryReader);

            // 0x0103 UShort 16-bit: 6
            var imageFileDirectory = rawImage[0x0000BF46];
            var imageFileEntry = imageFileDirectory[0x0103];
            Assert.Equal(3, imageFileEntry.TagType);
            Assert.Equal(6u, imageFileEntry.ValuePointer);
            Assert.Equal(1u, imageFileEntry.NumberOfValue);
        }

        [Fact]
        public void StripOffset()
        {
            using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var binaryReader = new BinaryReader(fileStream);
            var rawImage = new RawImage(binaryReader);

            // 0x0111 ULong 32-bit: 3213024
            var imageFileDirectory = rawImage[0x0000BF46];
            var imageFileEntry = imageFileDirectory[0x0111];
            Assert.Equal(0x0111, imageFileEntry.TagId);
            Assert.Equal(4, imageFileEntry.TagType);
            // Assert.Equal(3213024u, imageFileEntry.ValuePointer);
            Assert.Equal(1u, imageFileEntry.NumberOfValue);
        }

        [Fact]
        public void StripByteCounts()
        {
            using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var binaryReader = new BinaryReader(fileStream);
            var rawImage = new RawImage(binaryReader);

            // 0x0117 ULong 32-bit: 22286138 
            var imageFileDirectory = rawImage[0x0000BF46];
            var imageFileEntry = imageFileDirectory[0x0117];
            Assert.Equal(0x0117, imageFileEntry.TagId);
            Assert.Equal(4, imageFileEntry.TagType);
            // Assert.Equal(22286138u, imageFileEntry.ValuePointer);
            Assert.Equal(1u, imageFileEntry.NumberOfValue);
        }

        [Fact]
        public void Cr2Slice()
        {
            using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var binaryReader = new BinaryReader(fileStream);
            var rawImage = new RawImage(binaryReader);

            // 0xC640 UShort 16-bit: [0x0000BFA0] (3): 2, 1728, 1904, 
            var imageFileDirectory = rawImage[0x0000BF46];
            var imageFileEntry = imageFileDirectory[0xC640];
            Assert.Equal(3, imageFileEntry.TagType);
            Assert.Equal(0x0000BFA0u, imageFileEntry.ValuePointer);
            Assert.Equal(3u, imageFileEntry.NumberOfValue);

            Assert.Equal(new[] { (ushort)2, (ushort)1728, (ushort)1904 },
                RawImage.ReadUInts16(binaryReader, imageFileEntry));
        }
    }
}