// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		C7DIfd1.cs
// AUTHOR:		Greg Eakin

namespace PhotoTests.Canon7D
{
    using System;
    using System.IO;
    using Xunit;
    using PhotoLib.Tiff;

    
    public class C7DIfd0
    {
        private const string FileName = @"C:..\..\Photos\7Dhigh.CR2";

        public C7DIfd0()
        {
            if (!File.Exists(FileName))
            {
                throw new ArgumentException("{0} doesn't exists!", FileName);
            }
        }

        [Fact]
        public void TestMethod1()
        {
            using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read);
            using var binaryReader = new BinaryReader(fileStream);
            var rawImage = new RawImage(binaryReader);
            Assert.Equal(new byte[] { 0x49, 0x49 }, rawImage.Header.ByteOrder);
            Assert.Equal(0x002A, rawImage.Header.TiffMagic);
            Assert.Equal(0x5243, rawImage.Header.CR2Magic);
            Assert.Equal(new byte[] { 0x02, 0x00 }, rawImage.Header.CR2Version);

            rawImage.DumpHeader(binaryReader);
        }

        [Fact]
        public void ImageWidth()
        {
            using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read);
            using var binaryReader = new BinaryReader(fileStream);
            var rawImage = new RawImage(binaryReader);

            // 0x0100 UShort 16-bit: 5184
            var imageFileDirectory = rawImage[0x00000010];
            var imageFileEntry = imageFileDirectory[0x0100];
            Assert.Equal(3, imageFileEntry.TagType);
            Assert.Equal(5184u, imageFileEntry.ValuePointer);
            Assert.Equal(1u, imageFileEntry.NumberOfValue);
        }

        [Fact]
        public void ImageLength()
        {
            using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read);
            using var binaryReader = new BinaryReader(fileStream);
            var rawImage = new RawImage(binaryReader);

            // 0x0101 UShort 16-bit: 3456
            var imageFileDirectory = rawImage[0x00000010];
            var imageFileEntry = imageFileDirectory[0x0101];
            Assert.Equal(3, imageFileEntry.TagType);
            Assert.Equal(3456u, imageFileEntry.ValuePointer);
            Assert.Equal(1u, imageFileEntry.NumberOfValue);
        }

        [Fact]
        public void Maker()
        {
            using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read);
            using var binaryReader = new BinaryReader(fileStream);
            var rawImage = new RawImage(binaryReader);

            // 0x010F Ascii 8-bit: [0x000000F4] (6): Canon
            var imageFileDirectory = rawImage[0x00000010];
            var imageFileEntry = imageFileDirectory[0x010F];
            Assert.Equal(2, imageFileEntry.TagType);
            Assert.Equal(0x000000F4u, imageFileEntry.ValuePointer);
            Assert.Equal(6u, imageFileEntry.NumberOfValue);

            var readChars = RawImage.ReadChars(binaryReader, imageFileEntry);
            Assert.Equal("Canon", readChars);
        }

        [Fact]
        public void Model()
        {
            using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read);
            using var binaryReader = new BinaryReader(fileStream);
            var rawImage = new RawImage(binaryReader);

            // 0x0110 Ascii 8-bit: [0x000000FA] (13): Canon EOS 7D
            var imageFileDirectory = rawImage[0x00000010];
            var imageFileEntry = imageFileDirectory[0x0110];
            Assert.Equal(2, imageFileEntry.TagType);
            Assert.Equal(0x000000FAu, imageFileEntry.ValuePointer);
            Assert.Equal(13u, imageFileEntry.NumberOfValue);

            Assert.Equal("Canon EOS 7D", RawImage.ReadChars(binaryReader, imageFileEntry));
        }
    }
}