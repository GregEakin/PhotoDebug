namespace PhotoTests.Jpeg
{
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Jpeg;

    [TestClass]
    public class HuffmanTableTests
    {
        private readonly byte[] data =
            {
                            0xFF, 0xC4, 0x00, 0x42, 0x00, 0x00, 0x01, 0x04, 0x02, 0x03, 0x01, 0x01, 0x01, 0x01,
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x04, 0x08, 0x05, 0x07, 0x03, 0x09, 0x00, 0x0A, 
                0x02, 0x01, 0x0C, 0x0B, 0x0D, 0x0E, 0x01, 0x00, 0x01, 0x04, 0x02, 0x03, 0x01, 0x01, 0x01, 0x01,
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x04, 0x08, 0x05, 0x07, 0x03, 0x09, 0x00, 0x0A, 
                0x02, 0x01, 0x0C, 0x0B, 0x0D, 0x0E 
            };

        #region Public Methods and Operators

        [TestMethod]
        public void Mark()
        {
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new HuffmanTable(reader);
                Assert.AreEqual(0xFF, huffmanTable.Mark);
            }
        }

        [TestMethod]
        public void Tag()
        {
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new HuffmanTable(reader);
                Assert.AreEqual(0xC4, huffmanTable.Tag);
            }
        }
        
        [TestMethod]
        public void Length()
        {
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new HuffmanTable(reader);
                Assert.AreEqual(0x42, huffmanTable.Length);
            }
        }
        
        [TestMethod]
        public void Count()
        {
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new HuffmanTable(reader);
                Assert.AreEqual(2, huffmanTable.Tables.Count());
            }
        }
        
        [TestMethod]
        public void DataA1()
        {
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new HuffmanTable(reader);
                var expected = new byte[] { 0x00, 0x01, 0x04, 0x02, 0x03, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                CollectionAssert.AreEqual(expected, huffmanTable.Tables.First().Data1);
            }
        }

        [TestMethod]
        public void DataA2()
        {
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new HuffmanTable(reader);
                var expected = new byte[] { 0x06, 0x04, 0x08, 0x05, 0x07, 0x03, 0x09, 0x00, 0x0A, 0x02, 0x01, 0x0C, 0x0B, 0x0D, 0x0E };
                CollectionAssert.AreEqual(expected, huffmanTable.Tables.First().Data2);
            }
        }

        [TestMethod]
        public void DataB1()
        {
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new HuffmanTable(reader);
                var expected = new byte[] { 0x00, 0x01, 0x04, 0x02, 0x03, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                CollectionAssert.AreEqual(expected, huffmanTable.Tables.Skip(1).Single().Data1);
            }
        }

        [TestMethod]
        public void DataB2()
        {
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var huffmanTable = new HuffmanTable(reader);
                var expected = new byte[] { 0x06, 0x04, 0x08, 0x05, 0x07, 0x03, 0x09, 0x00, 0x0A, 0x02, 0x01, 0x0C, 0x0B, 0x0D, 0x0E };
                CollectionAssert.AreEqual(expected, huffmanTable.Tables.Skip(1).Single().Data2);
            }
        }
        #endregion
    }
}