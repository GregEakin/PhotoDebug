// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		HuffmanTableTests.cs
// AUTHOR:		Greg Eakin

using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using PhotoLib.Jpeg;

namespace PhotoTests.Jpeg
{

    [TestClass]
    public class HuffmanTableTests
    {
        #region Public Methods and Operators

        [TestMethod]
        public void BuildTreeDataTest()
        {
            var data1 = new byte[] { 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };
            var data2 = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15 };

            Assert.AreEqual(16, data1.Length);
            Assert.AreEqual(data2.Length, data1.Sum(b => b));
            Assert.IsTrue(data2.Length <= 256);
        }

        [TestMethod]
        public void HuffmanTableTest()
        {
            var data1 = new byte[] { 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };
            var data2 = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15 };

            var huffmanTable = new HuffmanTable(0, data1, data2);
            Assert.AreEqual(0, huffmanTable.Index);
            Assert.AreSame(data1, huffmanTable.Data1);
            Assert.AreSame(data2, huffmanTable.Data2);
            Assert.AreEqual(15, huffmanTable.Dictionary.Count);
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

            var dictionary = HuffmanTable.BuildTree(data1, data2);

            CollectionAssert.AreEqual(keys, dictionary.Keys);
        }

        [TestMethod]
        public void BuildTreeCodesTest()
        {
            var data1 = new byte[] { 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };
            var data2 = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15 };

            var codes = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15 };

            var dictionary = HuffmanTable.BuildTree(data1, data2);

            CollectionAssert.AreEqual(codes, dictionary.Values.Select(key => key.Code).ToArray());
        }

        [TestMethod]
        public void BuildTreeLengthTest()
        {
            var data1 = new byte[] { 0, 2, 2, 2, 3, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 };
            var data2 = new byte[] { 0, 5, 3, 6, 7, 2, 8, 0, 1, 9, 10, 11, 12, 13, 15 };

            var lengths = new byte[] { 2, 2, 3, 3, 4, 4, 5, 5, 5, 6, 7, 8, 9, 10, 11 };

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

            CollectionAssert.AreEqual(treeBits, HuffmanTable.ToTextTree(data1, data2));
        }

        #endregion

        [TestMethod]
        public void DecodeDifBitsTest()
        {
            Assert.AreEqual(0, HuffmanTable.DecodeDifBits(0, 0));

            Assert.AreEqual(-1, HuffmanTable.DecodeDifBits(1, 0));
            Assert.AreEqual(1, HuffmanTable.DecodeDifBits(1, 1));

            Assert.AreEqual(-3, HuffmanTable.DecodeDifBits(2, 0));
            Assert.AreEqual(-2, HuffmanTable.DecodeDifBits(2, 1));
            Assert.AreEqual(2, HuffmanTable.DecodeDifBits(2, 2));
            Assert.AreEqual(3, HuffmanTable.DecodeDifBits(2, 3));

            Assert.AreEqual(-7, HuffmanTable.DecodeDifBits(3, 0));
            Assert.AreEqual(-6, HuffmanTable.DecodeDifBits(3, 1));
            Assert.AreEqual(-5, HuffmanTable.DecodeDifBits(3, 2));
            Assert.AreEqual(-4, HuffmanTable.DecodeDifBits(3, 3));
            Assert.AreEqual(4, HuffmanTable.DecodeDifBits(3, 4));
            Assert.AreEqual(5, HuffmanTable.DecodeDifBits(3, 5));
            Assert.AreEqual(6, HuffmanTable.DecodeDifBits(3, 6));
            Assert.AreEqual(7, HuffmanTable.DecodeDifBits(3, 7));

            for (var bits = 4; bits < 16; bits++)
            {
                var x = 0x01u << bits;
                var y = 0x01u << (bits - 1);

                for (var codes = 0u; codes < (0x01u << bits); codes++)
                {
                    if (codes < y)
                    {
                        var expected = -x + 1 + codes;
                        Assert.AreEqual((short)expected, HuffmanTable.DecodeDifBits((ushort)bits, (ushort)codes));
                    }
                    else
                    {
                        var expected = codes;
                        Assert.AreEqual((short)expected, HuffmanTable.DecodeDifBits((ushort)bits, (ushort)codes));
                    }
                }
            }

            // Assert.AreEqual(32768, HuffmanTable.DecodeDifBits(16, 0));
        }
    }
}