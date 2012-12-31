namespace PhotoTests.Jpeg
{
    using System;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Jpeg;

    [TestClass]
    public class ImageDataTests
    {
        private static readonly byte[] Data = { 0xFE, 0xD5, 0x5F, 0xBD };

        [TestMethod]
        public void RawData()
        {
            using (var memory = new MemoryStream(Data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)Data.Length);
                CollectionAssert.AreEqual(Data, imageData.RawData);
            }
        }

        [TestMethod]
        public void GetBit()
        {
            var data = new byte[] { 0xA5 };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)data.Length);
                Assert.IsTrue(imageData.GetBit());
                Assert.IsFalse(imageData.GetBit());
                Assert.IsTrue(imageData.GetBit());
                Assert.IsFalse(imageData.GetBit());
                Assert.IsFalse(imageData.GetBit());
                Assert.IsTrue(imageData.GetBit());
                Assert.IsFalse(imageData.GetBit());
                Assert.IsTrue(imageData.GetBit());
            }
        }

        [TestMethod]
        public void GetBits1()
        {
            var data = new byte[] { 0xA5, 0x5A };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)data.Length);
                Assert.IsTrue(imageData.GetBit());
                Assert.IsFalse(imageData.GetBit());
                Assert.AreEqual(0x0009, imageData.GetNextBits(4));
            }
        }

        [TestMethod]
        public void GetBits2()
        {
            var data = new byte[] { 0xA5, 0x5A };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)data.Length);
                Assert.IsTrue(imageData.GetBit());
                Assert.IsFalse(imageData.GetBit());
                Assert.AreEqual(0x0009, imageData.GetNextBits(4));
                Assert.AreEqual(0x0001, imageData.GetNextBits(2));
                Assert.AreEqual(0x005A, imageData.GetNextBits(8));
            }
        }

        [TestMethod]
        public void GetBits3()
        {
            var data = new byte[] { 0xA5, 0x5A };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)data.Length);
                Assert.AreEqual(0x000A, imageData.GetNextBits(4));
                Assert.AreEqual(0x0055, imageData.GetNextBits(8));
                Assert.AreEqual(0x000A, imageData.GetNextBits(4));
            }
        }
    }
}