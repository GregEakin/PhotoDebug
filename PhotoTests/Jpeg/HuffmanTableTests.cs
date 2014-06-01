using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PhotoLib.Jpeg;

namespace PhotoTests.Jpeg
{
    using System;

    [TestClass]
    public class HuffmanTableTests
    {
        #region Public Methods and Operators

        [TestMethod]
        public void BuildTree()
        {
            var treeData = new byte[] { 0xFF, 0xC4, 0, 34, 0, 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15, 14 };

            var treeBits = new[]
                {
                    "00", "01", "100", "101", "1100", "1101", "11100", "11101", "11110", "111110", "1111110", "11111110", "111111110", "1111111110",
                    "11111111110"
                };

            using (var memory = new MemoryStream(treeData))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new DefineHuffmanTable(reader);
                var table = huffmanTable.Tables.First().Value;
                CollectionAssert.AreEqual(treeBits, table.BuildTextTree());
            }
        }

        [TestMethod]
        public void PrintBits1()
        {
            var z = HuffmanTable.PrintBits(0x01, 0x00);
            Assert.AreEqual("1", z);
        }

        [TestMethod]
        public void PrintBits2()
        {
            var z = HuffmanTable.PrintBits(0x02, 0x02);
            Assert.AreEqual("010", z);
        }

        [TestMethod]
        public void DcCodeTestFour()
        {
            for (var i = 8; i < 16; i++)
            {
                var expected = i;
                Assert.AreEqual(expected, HuffmanTable.DcValueEncoding(4, (byte)i));
            }
        }

        [TestMethod]
        public void DcCodeTestFourNegative()
        {
            for (var i = 0; i < 8; i++)
            {
                var expected = i - 15;
                Assert.AreEqual(expected, HuffmanTable.DcValueEncoding(4, (byte)i));
            }
        }

        [TestMethod]
        public void DcCodeTestOne()
        {
            Assert.AreEqual(1, HuffmanTable.DcValueEncoding(1, 1));
            Assert.AreEqual(-1, HuffmanTable.DcValueEncoding(1, 0));
        }

        [TestMethod]
        public void DcCodeTestSimple()
        {
            // 0 1111 1111 1
            Assert.AreEqual(-512, HuffmanTable.DcValueEncoding(10, (ushort)0x01FFu));
        }

        [TestMethod]
        public void DcCodeTestZero()
        {
            Assert.AreEqual(0, HuffmanTable.DcValueEncoding(0, 0));
        }

        [TestMethod]
        public void BuildTreeKeysTest()
        {
            var data1 = new byte[] { 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };
            var data2 = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15 };

            var keys = new[] { 0, 1, 4, 5, 12, 13, 28, 29, 30, 62, 126, 254, 510, 1022, 2046 };


            Assert.AreEqual(16, data1.Length);
            Assert.AreEqual(data2.Length, data1.Sum(b => b));
            Assert.IsTrue(data2.Length <= 256);
            var dictionary = HuffmanTable.BuildTree(data1, data2);

            foreach (var key in dictionary.Keys)
            {
                Console.Write("{0}, ", key);
            }
            CollectionAssert.AreEqual(keys, dictionary.Keys);
        }

        [TestMethod]
        public void BuildTreeCodesTest()
        {
            var data1 = new byte[] { 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };
            var data2 = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15 };

            var codes = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15, };

            Assert.AreEqual(16, data1.Length);
            Assert.AreEqual(data2.Length, data1.Sum(b => b));
            Assert.IsTrue(data2.Length <= 256);
            var dictionary = HuffmanTable.BuildTree(data1, data2);

            CollectionAssert.AreEqual(codes, dictionary.Values.Select(key => key.Code).ToArray());
        }

        [TestMethod]
        public void BuildTreeLengthTest()
        {
            var data1 = new byte[] { 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };
            var data2 = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15 };

            var lengths = new byte[] { 2, 2, 3, 3, 4, 4, 5, 5, 5, 6, 7, 8, 9, 10, 11, };

            Assert.AreEqual(16, data1.Length);
            Assert.AreEqual(data2.Length, data1.Sum(b => b));
            Assert.IsTrue(data2.Length <= 256);
            var dictionary = HuffmanTable.BuildTree(data1, data2);

            CollectionAssert.AreEqual(lengths, dictionary.Values.Select(key => key.Length).ToArray());
        }

        [TestMethod]
        public void TextTreeTest()
        {
            var data1 = new byte[] { 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };
            var data2 = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15 };

            var treeBits = new[]
                {
                    "00", "01", 
                    "100", "101", 
                    "1100", "1101", 
                    "11100", "11101", "11110", 
                    "111110", 
                    "1111110", 
                    "11111110", 
                    "111111110", 
                    "1111111110",
                    "11111111110"
                };


            Assert.AreEqual(16, data1.Length);
            Assert.AreEqual(data2.Length, data1.Sum(b => b));
            Assert.IsTrue(data2.Length <= 256);

            CollectionAssert.AreEqual(treeBits, HuffmanTable.ToTextTree(data1, data2));
        }

        #endregion
    }
}