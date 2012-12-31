namespace PhotoTests.Jpeg
{
    using System;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Jpeg;

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
        [ExpectedException(typeof(ArgumentException))]
        public void BadMark()
        {
            var badData = new byte[] { 0x00, 0x00 };
            using (var memory = new MemoryStream(badData))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BadTag()
        {
            var badData = new byte[] { 0xFF, 0x00 };
            using (var memory = new MemoryStream(badData))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
            }
        }

        [TestMethod]
        public void BuildTree()
        {
            var treeData = new byte[]
                { 0xFF, 0xC4, 0, 34, 0, 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15, 14 };

            var treeBits = new string[]
                {
                    "00", "01", "100", "101", "1100", "1101", "11100", "11101", "11110", "111110", "1111110", "11111110", "111111110", "1111111110",
                    "11111111110"
                };

            using (var memory = new MemoryStream(treeData))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
                var table = huffmanTable.Tables.First();
                CollectionAssert.AreEqual(treeBits, DefineHuffmanTable.BuildTree(table));
            }
        }

        [TestMethod]
        public void Count()
        {
            using (var memory = new MemoryStream(Data))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
                Assert.AreEqual(2, huffmanTable.Tables.Count());
            }
        }

        [TestMethod]
        public void DataA1()
        {
            using (var memory = new MemoryStream(Data))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
                var expected = new byte[] { 0x00, 0x01, 0x04, 0x02, 0x03, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                CollectionAssert.AreEqual(expected, huffmanTable.Tables.First().Data1);
            }
        }

        [TestMethod]
        public void DataA2()
        {
            using (var memory = new MemoryStream(Data))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
                var expected = new byte[] { 0x06, 0x04, 0x08, 0x05, 0x07, 0x03, 0x09, 0x00, 0x0A, 0x02, 0x01, 0x0C, 0x0B, 0x0D, 0x0E };
                CollectionAssert.AreEqual(expected, huffmanTable.Tables.First().Data2);
            }
        }

        [TestMethod]
        public void DataB1()
        {
            using (var memory = new MemoryStream(Data))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
                var expected = new byte[] { 0x00, 0x01, 0x04, 0x02, 0x03, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                CollectionAssert.AreEqual(expected, huffmanTable.Tables.Skip(1).Single().Data1);
            }
        }

        [TestMethod]
        public void DataB2()
        {
            using (var memory = new MemoryStream(Data))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
                var expected = new byte[] { 0x06, 0x04, 0x08, 0x05, 0x07, 0x03, 0x09, 0x00, 0x0A, 0x02, 0x01, 0x0C, 0x0B, 0x0D, 0x0E };
                CollectionAssert.AreEqual(expected, huffmanTable.Tables.Skip(1).Single().Data2);
            }
        }

        [TestMethod]
        public void DcCodeTestFour()
        {
            for (var i = 8; i < 16; i++)
            {
                var expected = i;
                Assert.AreEqual(expected, DefineHuffmanTable.DcValueEncoding(4, (byte)i));
            }
        }

        [TestMethod]
        public void DcCodeTestFourNegative()
        {
            for (var i = 0; i < 8; i++)
            {
                var expected = i - 15;
                Assert.AreEqual(expected, DefineHuffmanTable.DcValueEncoding(4, (byte)i));
            }
        }

        [TestMethod]
        public void DcCodeTestOne()
        {
            Assert.AreEqual(1, DefineHuffmanTable.DcValueEncoding(1, 1));
            Assert.AreEqual(-1, DefineHuffmanTable.DcValueEncoding(1, 0));
        }

        [TestMethod]
        public void DcCodeTestZero()
        {
            Assert.AreEqual(0, DefineHuffmanTable.DcValueEncoding(0, 0));
        }

        [TestMethod]
        public void Length()
        {
            using (var memory = new MemoryStream(Data))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
                Assert.AreEqual(0x42, huffmanTable.Length);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LongLengthA()
        {
            var badData = new byte[]
                { 0xFF, 0xC4, 0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            using (var memory = new MemoryStream(badData))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(EndOfStreamException))]
        public void LongLengthB()
        {
            var badData = new byte[]
                {
                    0xFF, 0xC4, 0x00, 0x38, 0x00, 0x12, 0x11, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06,
                    0x03, 0x09, 0x07
                };

            using (var memory = new MemoryStream(badData))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
            }
        }

        [TestMethod]
        public void Mark()
        {
            using (var memory = new MemoryStream(Data))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
                Assert.AreEqual(0xFF, huffmanTable.Mark);
            }
        }

        [TestMethod]
        public void PrintBits1()
        {
            var z = DefineHuffmanTable.PrintBits(0x01, 0x00);
            Assert.AreEqual("1", z);
        }

        [TestMethod]
        public void PrintBits2()
        {
            var z = DefineHuffmanTable.PrintBits(0x02, 0x02);
            Assert.AreEqual("010", z);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ShortLengthA()
        {
            var badData = new byte[]
                { 0xFF, 0xC4, 0x00, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            using (var memory = new MemoryStream(badData))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ShortLengthB()
        {
            var badData = new byte[]
                {
                    0xFF, 0xC4, 0x00, 0x24, 0x00, 0x12, 0x11, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06,
                    0x03, 0x09, 0x07
                };

            using (var memory = new MemoryStream(badData))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
            }
        }

        [TestMethod]
        public void Tag()
        {
            using (var memory = new MemoryStream(Data))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
                Assert.AreEqual(0xC4, huffmanTable.Tag);
            }
        }

        #endregion
    }
}