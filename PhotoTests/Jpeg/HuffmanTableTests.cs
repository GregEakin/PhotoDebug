namespace PhotoTests.Jpeg
{
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Jpeg;

    public class HuffmanTableTests
    {
        #region Public Methods and Operators

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

        #endregion
    }
}