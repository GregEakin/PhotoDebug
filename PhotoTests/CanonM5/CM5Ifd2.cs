// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		C5D3Ifd1.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;
using System.Linq;
using Xunit;
using PhotoLib.Jpeg.JpegTags;
using PhotoLib.Tiff;

namespace PhotoTests.CanonM5
{
    // The first IFD contains a small RGB version of the picture (one fourth the size) compressed in Jpeg, the EXIF part, and the Makernotes part. 
    // The second IFD contains a small RGB version (160x120 pixels) of the picture, compressed in Jpeg.
    // The third IFD contains a small RGB version of the picture, NOT compressed (even with compression==6), and one which no white balance, correction has been applied.
    // The fourth IFD contains the RAW data compressed in lossless Jpeg. 

    
    public class CM5Ifd2
    {
        private const string FileName = @"P:\Source\IMG_0012.CR2";

        public CM5Ifd2()
        {
            if (!File.Exists(FileName))
                throw new ArgumentException("{0} doesn't exists!", FileName);
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

        [Fact]
        public void TestMethod2()
        {
            using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var binaryReader = new BinaryReader(fileStream);
            var rawImage = new RawImage(binaryReader);
            var imageFileDirectory = rawImage.Directories.Skip(2).First();
            imageFileDirectory.DumpDirectory(binaryReader);
        }

        // 5)  0x0111 ULong 32-bit: 6664192u    -- Offset
        // 8)  0x0117 ULong 32-bit: 1440000u    -- Length
        [Fact]
        public void DumpImage1()
        {
            using var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var binaryReader = new BinaryReader(fileStream);
            var rawImage = new RawImage(binaryReader);
            var imageFileDirectory = rawImage.Directories.Skip(2).First();
            Assert.Equal(13, imageFileDirectory.Entries.Length);
            //Assert.Equal(
            //    new ushort[] { 0x0201, 0x0202 },
            //    imageFileDirectory.Entries.Select(e => e.TagId).ToArray());

            var offset = imageFileDirectory.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
            Assert.Equal(6664192u, offset);

            var length = imageFileDirectory.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
            Assert.Equal(1440000u, length);    

            binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
            var name = Path.Combine(Path.GetDirectoryName(FileName) ?? "./", Path.GetFileNameWithoutExtension(FileName) + "-2.rgb");
            DumpImage(name, binaryReader, length);
        }

        private static void DumpImage(string output, BinaryReader binaryReader, uint length)
        {
            using var x = File.Create(output);
            var bytes = (int) length;
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