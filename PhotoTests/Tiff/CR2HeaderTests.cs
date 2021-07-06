// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		CR2HeaderTests.cs
// AUTHOR:		Greg Eakin

using System.IO;
using Xunit;
using PhotoLib.Tiff;

namespace PhotoTests.Tiff
{
    public class CR2HeaderTests
    {
        private static readonly byte[] Data = { 0x49, 0x49, 0x2A, 0x00, 0x10, 0x00, 0x00, 0x00, 0x43, 0x52, 0x02, 0x00, 0x46, 0xBF, 0x00, 0x00 };

        [Fact]
        public void ByteOrder()
        {
            using var memory = new MemoryStream(Data);
            using var reader = new BinaryReader(memory);
            var cr2Header = new CR2Header(reader);
            Assert.Equal(new byte[] { 0x49, 0x49 }, cr2Header.ByteOrder);
        }

        [Fact]
        public void CR2Magic()
        {
            using var memory = new MemoryStream(Data);
            using var reader = new BinaryReader(memory);
            var cr2Header = new CR2Header(reader);
            Assert.Equal(0x5243, cr2Header.CR2Magic); // "CR2\0"
        }

        [Fact]
        public void CR2Version()
        {
            using var memory = new MemoryStream(Data);
            using var reader = new BinaryReader(memory);
            var cr2Header = new CR2Header(reader);
            Assert.Equal(new byte[] { 0x02, 0x00 }, cr2Header.CR2Version);
        }

        [Fact]
        public void RawIfdOffset()
        {
            using var memory = new MemoryStream(Data);
            using var reader = new BinaryReader(memory);
            var cr2Header = new CR2Header(reader);
            Assert.Equal(0x0000BF46u, cr2Header.RawIfdOffset);
        }

        [Fact]
        public void TiffMagic()
        {
            using var memory = new MemoryStream(Data);
            using var reader = new BinaryReader(memory);
            var cr2Header = new CR2Header(reader);
            Assert.Equal(0x002A, cr2Header.TiffMagic);
        }

        [Fact]
        public void TiffOffset()
        {
            using var memory = new MemoryStream(Data);
            using var reader = new BinaryReader(memory);
            var cr2Header = new CR2Header(reader);
            Assert.Equal(0x00000010u, cr2Header.TiffOffset);
        }
    }
}