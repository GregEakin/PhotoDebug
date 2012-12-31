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
            var treeData = new byte[] { 0xFF, 0xC4, 0, 34, 0, 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15, 14 };

            var treeBits = new string[]
                {
                    "00", "01", "100", "101", "1100", "1101", "11100", "11101", "11110", "111110", "1111110", "11111110", "111111110", "1111111110",
                    "11111111110"
                };

            using (var memory = new MemoryStream(treeData))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
                var table = huffmanTable.Tables.First().Value;
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
                CollectionAssert.AreEqual(expected, huffmanTable.Tables.First().Value.Data1);
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
                CollectionAssert.AreEqual(expected, huffmanTable.Tables.First().Value.Data2);
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
                CollectionAssert.AreEqual(expected, huffmanTable.Tables.Skip(1).Single().Value.Data1);
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
                CollectionAssert.AreEqual(expected, huffmanTable.Tables.Skip(1).Single().Value.Data2);
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
        public void DcCodeTestSimple()
        {
            // 0 1111 1111 1
            Assert.AreEqual(-512, DefineHuffmanTable.DcValueEncoding(10, (ushort)0x01FFu));
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
            var badData = new byte[] { 0xFF, 0xC4, 0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            using (var memory = new MemoryStream(badData))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
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
            var badData = new byte[] { 0xFF, 0xC4, 0x00, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            using (var memory = new MemoryStream(badData))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
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

        [TestMethod]
        public void Luminance()
        {
            var simpleData = new byte[]
                {
                     0xFF, 0xC4, 0x01, 0xA2, 0x00, 0x00, 0x00, 0x07, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00,
                     0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x05, 0x03, 0x02, 0x06, 0x01, 0x00, 0x07, 0x08, 0x09, 0x0A,
                     0x0B, 0x01, 0x00, 0x02, 0x02, 0x03, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
                     0x00, 0x00, 0x01, 0x00, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x10, 0x00,
                     0x02, 0x01, 0x03, 0x03, 0x02, 0x04, 0x02, 0x06, 0x07, 0x03, 0x04, 0x02, 0x06, 0x02, 0x73, 0x01,
                     0x02, 0x03, 0x11, 0x04, 0x00, 0x05, 0x21, 0x12, 0x31, 0x41, 0x51, 0x06, 0x13, 0x61, 0x22, 0x71,
                     0x81, 0x14, 0x32, 0x91, 0xA1, 0x07, 0x15, 0xB1, 0x42, 0x23, 0xC1, 0x52, 0xD1, 0xE1, 0x33, 0x16,
                     0x62, 0xF0, 0x24, 0x72, 0x82, 0xF1, 0x25, 0x43, 0x34, 0x53, 0x92, 0xA2, 0xB2, 0x63, 0x73, 0xC2,
                     0x35, 0x44, 0x27, 0x93, 0xA3, 0xB3, 0x36, 0x17, 0x54, 0x64, 0x74, 0xC3, 0xD2, 0xE2, 0x08, 0x26,
                     0x83, 0x09, 0x0A, 0x18, 0x19, 0x84, 0x94, 0x45, 0x46, 0xA4, 0xB4, 0x56, 0xD3, 0x55, 0x28, 0x1A,
                     0xF2, 0xE3, 0xF3, 0xC4, 0xD4, 0xE4, 0xF4, 0x65, 0x75, 0x85, 0x95, 0xA5, 0xB5, 0xC5, 0xD5, 0xE5,
                     0xF5, 0x66, 0x76, 0x86, 0x96, 0xA6, 0xB6, 0xC6, 0xD6, 0xE6, 0xF6, 0x37, 0x47, 0x57, 0x67, 0x77,
                     0x87, 0x97, 0xA7, 0xB7, 0xC7, 0xD7, 0xE7, 0xF7, 0x38, 0x48, 0x58, 0x68, 0x78, 0x88, 0x98, 0xA8,
                     0xB8, 0xC8, 0xD8, 0xE8, 0xF8, 0x29, 0x39, 0x49, 0x59, 0x69, 0x79, 0x89, 0x99, 0xA9, 0xB9, 0xC9,
                     0xD9, 0xE9, 0xF9, 0x2A, 0x3A, 0x4A, 0x5A, 0x6A, 0x7A, 0x8A, 0x9A, 0xAA, 0xBA, 0xCA, 0xDA, 0xEA,
                     0xFA, 0x11, 0x00, 0x02, 0x02, 0x01, 0x02, 0x03, 0x05, 0x05, 0x04, 0x05, 0x06, 0x04, 0x08, 0x03,
                     0x03, 0x6D, 0x01, 0x00, 0x02, 0x11, 0x03, 0x04, 0x21, 0x12, 0x31, 0x41, 0x05, 0x51, 0x13, 0x61,
                     0x22, 0x06, 0x71, 0x81, 0x91, 0x32, 0xA1, 0xB1, 0xF0, 0x14, 0xC1, 0xD1, 0xE1, 0x23, 0x42, 0x15,
                     0x52, 0x62, 0x72, 0xF1, 0x33, 0x24, 0x34, 0x43, 0x82, 0x16, 0x92, 0x53, 0x25, 0xA2, 0x63, 0xB2,
                     0xC2, 0x07, 0x73, 0xD2, 0x35, 0xE2, 0x44, 0x83, 0x17, 0x54, 0x93, 0x08, 0x09, 0x0A, 0x18, 0x19,
                     0x26, 0x36, 0x45, 0x1A, 0x27, 0x64, 0x74, 0x55, 0x37, 0xF2, 0xA3, 0xB3, 0xC3, 0x28, 0x29, 0xD3,
                     0xE3, 0xF3, 0x84, 0x94, 0xA4, 0xB4, 0xC4, 0xD4, 0xE4, 0xF4, 0x65, 0x75, 0x85, 0x95, 0xA5, 0xB5,
                     0xC5, 0xD5, 0xE5, 0xF5, 0x46, 0x56, 0x66, 0x76, 0x86, 0x96, 0xA6, 0xB6, 0xC6, 0xD6, 0xE6, 0xF6,
                     0x47, 0x57, 0x67, 0x77, 0x87, 0x97, 0xA7, 0xB7, 0xC7, 0xD7, 0xE7, 0xF7, 0x38, 0x48, 0x58, 0x68,
                     0x78, 0x88, 0x98, 0xA8, 0xB8, 0xC8, 0xD8, 0xE8, 0xF8, 0x39, 0x49, 0x59, 0x69, 0x79, 0x89, 0x99,
                     0xA9, 0xB9, 0xC9, 0xD9, 0xE9, 0xF9, 0x2A, 0x3A, 0x4A, 0x5A, 0x6A, 0x7A, 0x8A, 0x9A, 0xAA, 0xBA,
                     0xCA, 0xDA, 0xEA, 0xFA, 0xFF, 0xDA, 0x00, 0x0C, 0x03, 0x01, 0x00, 0x02, 0x11, 0x03, 0x11, 0x00,
                     0x3F, 0x00, 0xFC, 0xFF, 0x00, 0xE2, 0xAF, 0xEF, 0xF3, 0x15, 0x7F, 0xFF, 0xD9                
                };

            using (var memory = new MemoryStream(simpleData))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);

                var table = huffmanTable.Tables.First().Value;

                var bits = new byte[] { 0xFC, 0xFF, 0xE2, 0xAF, 0xEF, 0xF3, 0x15, 0x7F };
                var x = (byte)0;
                var len = 0;

                foreach (var bit in bits)
                {
                    for (var i = 7; i >= 0; i--)
                    {
                        var mask = (byte)(1 << i);
                        x = (byte)(x << 1);
                        x |= (bit & mask) > 0 ? (byte)1 : (byte)0;
                        len++;

                        HuffmanTable.HCode hCode;
                        if (table.Dictionary.TryGetValue(x, out hCode) && hCode.Length == len)
                        {
                            var z = 0x1FF;
                            var value = DefineHuffmanTable.DcValueEncoding(table.Dictionary[x].Code, z);

                            Console.WriteLine("Found {0} {1} {2}", x.ToString("X2"), table.Dictionary[x].Code.ToString("X2"), value);
                            x = 0;
                            len = 0;
                        }
                    }
                }
            }
        }
        #endregion
    }
}