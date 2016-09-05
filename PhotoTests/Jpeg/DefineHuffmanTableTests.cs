using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg.JpegTags;
using System;
using System.IO;
using System.Linq;
using static PhotoTests.Utilities.ExpectedException;

namespace PhotoTests.Jpeg
{
    [TestClass]
    public class DefineHuffmanTableTests
    {
        #region Static Fields

        private static readonly byte[] Data =
            {
                0xFF, 0xC4, 0x00, 0x42, 0x00, 0x00, 0x01, 0x04, 0x02, 0x03, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x06, 0x04, 0x08, 0x05, 0x07, 0x03, 0x09, 0x00, 0x0A, 0x02, 0x01, 0x0C, 0x0B, 0x0D, 0x0E, 0x01, 0x00, 0x01, 0x04,
                0x02, 0x03, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x04, 0x08, 0x05, 0x07, 0x03, 0x09, 0x00, 0x0A, 0x02,
                0x01, 0x0C, 0x0B, 0x0D, 0x0E
            };

        #endregion

        #region Public Methods and Operators

        [TestMethod]
        public void BadMark()
        {
            var badData = new byte[] { 0x00, 0x00 };
            using (var memory = new MemoryStream(badData))
            using (var reader = new BinaryReader(memory))
            {
                AssertThrows<ArgumentException>(() => new DefineHuffmanTable(reader) );
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BadTag()
        {
            var badData = new byte[] { 0xFF, 0x00 };
            using (var memory = new MemoryStream(badData))
            using (var reader = new BinaryReader(memory))
            {
                var huffmanTable = new DefineHuffmanTable(reader);
            }
        }

        [TestMethod]
        public void Count()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var huffmanTable = new DefineHuffmanTable(reader);
                Assert.AreEqual(2, huffmanTable.Tables.Count());
            }
        }

        [TestMethod]
        public void DataA1()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var huffmanTable = new DefineHuffmanTable(reader);
                var expected = new byte[] { 0x00, 0x01, 0x04, 0x02, 0x03, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                CollectionAssert.AreEqual(expected, huffmanTable.Tables.First().Value.Data1);
            }
        }

        [TestMethod]
        public void DataA2()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var huffmanTable = new DefineHuffmanTable(reader);
                var expected = new byte[] { 0x06, 0x04, 0x08, 0x05, 0x07, 0x03, 0x09, 0x00, 0x0A, 0x02, 0x01, 0x0C, 0x0B, 0x0D, 0x0E };
                CollectionAssert.AreEqual(expected, huffmanTable.Tables.First().Value.Data2);
            }
        }

        [TestMethod]
        public void DataB1()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var huffmanTable = new DefineHuffmanTable(reader);
                var expected = new byte[] { 0x00, 0x01, 0x04, 0x02, 0x03, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                CollectionAssert.AreEqual(expected, huffmanTable.Tables.Skip(1).Single().Value.Data1);
            }
        }

        [TestMethod]
        public void DataB2()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var huffmanTable = new DefineHuffmanTable(reader);
                var expected = new byte[] { 0x06, 0x04, 0x08, 0x05, 0x07, 0x03, 0x09, 0x00, 0x0A, 0x02, 0x01, 0x0C, 0x0B, 0x0D, 0x0E };
                CollectionAssert.AreEqual(expected, huffmanTable.Tables.Skip(1).Single().Value.Data2);
            }
        }

        [TestMethod]
        public void Length()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var huffmanTable = new DefineHuffmanTable(reader);
                Assert.AreEqual(0x42, huffmanTable.Length);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LongLengthA()
        {
            var badData = new byte[] { 0xFF, 0xC4, 0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            using (var memory = new MemoryStream(badData))
            using (var reader = new BinaryReader(memory))
            {
                var huffmanTable = new DefineHuffmanTable(reader);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void LongLengthB()
        {
            var badData = new byte[]
                {
                    0xFF, 0xC4, 0x00, 0x38, 0x00, 0x12, 0x11, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06,
                    0x03, 0x09, 0x07
                };

            using (var memory = new MemoryStream(badData))
            using (var reader = new BinaryReader(memory))
            {
                var huffmanTable = new DefineHuffmanTable(reader);
            }
        }

        [TestMethod]
        public void Mark()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var huffmanTable = new DefineHuffmanTable(reader);
                Assert.AreEqual(0xFF, huffmanTable.Mark);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ShortLengthA()
        {
            var badData = new byte[] { 0xFF, 0xC4, 0x00, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            using (var memory = new MemoryStream(badData))
            using (var reader = new BinaryReader(memory))
            {
                var huffmanTable = new DefineHuffmanTable(reader);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ShortLengthB()
        {
            var badData = new byte[]
                {
                    0xFF, 0xC4, 0x00, 0x24, 0x00, 0x12, 0x11, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06,
                    0x03, 0x09, 0x07
                };

            using (var memory = new MemoryStream(badData))
            using (var reader = new BinaryReader(memory))
            {
                var huffmanTable = new DefineHuffmanTable(reader);
            }
        }

        [TestMethod]
        public void Tag()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var huffmanTable = new DefineHuffmanTable(reader);
                Assert.AreEqual(0xC4, huffmanTable.Tag);
            }
        }

        #endregion
    }
}