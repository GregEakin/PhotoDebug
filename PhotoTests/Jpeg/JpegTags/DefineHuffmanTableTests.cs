// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		DefineHuffmanTests.cs
// AUTHOR:		Greg Eakin

using Xunit;
using PhotoLib.Jpeg.JpegTags;
using System;
using System.IO;
using System.Linq;

namespace PhotoTests.Jpeg.JpegTags
{
    public class DefineHuffmanTableTests
    {
        private static readonly byte[] Data =
            {
                0xFF, 0xC4, 0x00, 0x42, 0x00, 0x00, 0x01, 0x04, 0x02, 0x03, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x06, 0x04, 0x08, 0x05, 0x07, 0x03, 0x09, 0x00, 0x0A, 0x02, 0x01, 0x0C, 0x0B, 0x0D, 0x0E, 0x01, 0x00, 0x01, 0x04,
                0x02, 0x03, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x04, 0x08, 0x05, 0x07, 0x03, 0x09, 0x00, 0x0A, 0x02,
                0x01, 0x0C, 0x0B, 0x0D, 0x0E
            };

        [Fact]
        public void BadMark()
        {
            var badData = new byte[] { 0x00, 0x00 };
            using var memory = new MemoryStream(badData);
            using var reader = new BinaryReader(memory);
            Assert.Throws<ArgumentException>(() => new DefineHuffmanTable(reader));
        }

        [Fact]
        public void BadTag()
        {
            var badData = new byte[] { 0xFF, 0x00 };
            using var memory = new MemoryStream(badData);
            using var reader = new BinaryReader(memory);
            Assert.Throws<ArgumentException>(() => new DefineHuffmanTable(reader));
        }

        [Fact]
        public void Count()
        {
            using var memory = new MemoryStream(Data);
            using var reader = new BinaryReader(memory);
            var huffmanTable = new DefineHuffmanTable(reader);
            Assert.Equal(2, huffmanTable.Tables.Count);
        }

        [Fact]
        public void DataA1()
        {
            using var memory = new MemoryStream(Data);
            using var reader = new BinaryReader(memory);
            var huffmanTable = new DefineHuffmanTable(reader);
            var expected = new byte[] { 0x00, 0x01, 0x04, 0x02, 0x03, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            Assert.Equal(expected, huffmanTable.Tables.First().Value.Data1);
        }

        [Fact]
        public void DataA2()
        {
            using var memory = new MemoryStream(Data);
            using var reader = new BinaryReader(memory);
            var huffmanTable = new DefineHuffmanTable(reader);
            var expected = new byte[] { 0x06, 0x04, 0x08, 0x05, 0x07, 0x03, 0x09, 0x00, 0x0A, 0x02, 0x01, 0x0C, 0x0B, 0x0D, 0x0E };
            Assert.Equal(expected, huffmanTable.Tables.First().Value.Data2);
        }

        [Fact]
        public void DataB1()
        {
            using var memory = new MemoryStream(Data);
            using var reader = new BinaryReader(memory);
            var huffmanTable = new DefineHuffmanTable(reader);
            var expected = new byte[] { 0x00, 0x01, 0x04, 0x02, 0x03, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            Assert.Equal(expected, huffmanTable.Tables.Skip(1).Single().Value.Data1);
        }

        [Fact]
        public void DataB2()
        {
            using var memory = new MemoryStream(Data);
            using var reader = new BinaryReader(memory);
            var huffmanTable = new DefineHuffmanTable(reader);
            var expected = new byte[] { 0x06, 0x04, 0x08, 0x05, 0x07, 0x03, 0x09, 0x00, 0x0A, 0x02, 0x01, 0x0C, 0x0B, 0x0D, 0x0E };
            Assert.Equal(expected, huffmanTable.Tables.Skip(1).Single().Value.Data2);
        }

        [Fact]
        public void Length()
        {
            using var memory = new MemoryStream(Data);
            using var reader = new BinaryReader(memory);
            var huffmanTable = new DefineHuffmanTable(reader);
            Assert.Equal(0x42, huffmanTable.Length);
        }

        [Fact]
        public void LongLengthA()
        {
            var badData = new byte[] { 0xFF, 0xC4, 0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            using var memory = new MemoryStream(badData);
            using var reader = new BinaryReader(memory);
            Assert.Throws<ArgumentException>(() => new DefineHuffmanTable(reader));
        }

        [Fact]
        public void LongLengthB()
        {
            var badData = new byte[]
                {
                    0xFF, 0xC4, 0x00, 0x38, 0x00, 0x12, 0x11, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06,
                    0x03, 0x09, 0x07
                };

            using var memory = new MemoryStream(badData);
            using var reader = new BinaryReader(memory);
            Assert.Throws<ArgumentOutOfRangeException>(() => new DefineHuffmanTable(reader));
        }

        [Fact]
        public void Mark()
        {
            using var memory = new MemoryStream(Data);
            using var reader = new BinaryReader(memory);
            var huffmanTable = new DefineHuffmanTable(reader);
            Assert.Equal(0xFF, huffmanTable.Mark);
        }

        [Fact]
        public void ShortLengthA()
        {
            var badData = new byte[] { 0xFF, 0xC4, 0x00, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            using var memory = new MemoryStream(badData);
            using var reader = new BinaryReader(memory);
            Assert.Throws<ArgumentException>(() => new DefineHuffmanTable(reader));
        }

        [Fact]
        public void ShortLengthB()
        {
            var badData = new byte[]
                {
                    0xFF, 0xC4, 0x00, 0x24, 0x00, 0x12, 0x11, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06,
                    0x03, 0x09, 0x07
                };

            using var memory = new MemoryStream(badData);
            using var reader = new BinaryReader(memory);
            Assert.Throws<ArgumentOutOfRangeException>(() => new DefineHuffmanTable(reader));
        }

        [Fact]
        public void Tag()
        {
            using var memory = new MemoryStream(Data);
            using var reader = new BinaryReader(memory);
            var huffmanTable = new DefineHuffmanTable(reader);
            Assert.Equal(0xC4, huffmanTable.Tag);
        }
    }
}