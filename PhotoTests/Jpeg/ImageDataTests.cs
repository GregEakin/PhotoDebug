namespace PhotoTests.Jpeg
{
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Jpeg;

    [TestClass]
    public class ImageDataTests
    {
        [TestMethod]
        public void IndexBegin()
        {
            var data = new byte[] { 0xA5, 0x5A };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)data.Length);
                Assert.AreEqual(0, imageData.Index);
            }
        }

        [TestMethod]
        public void Index0()
        {
            var data = new byte[] { 0xA5, 0x5A };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)data.Length);
                imageData.GetNextBit();
                Assert.AreEqual(0, imageData.Index);
            }
        }

        [TestMethod]
        public void Index1()
        {
            var data = new byte[] { 0xA5, 0x5A };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)data.Length);
                imageData.GetSetOfBits(8);
                Assert.AreEqual(1, imageData.Index);
            }
        }

        [TestMethod]
        public void Index2()
        {
            var data = new byte[] { 0xA5, 0x5A };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)data.Length);
                imageData.GetSetOfBits(8);
                imageData.GetSetOfBits(8);
                Assert.AreEqual(2, imageData.Index);
            }
        }

        [TestMethod]
        public void EndOfFileBegin()
        {
            var data = new byte[] { 0xA5, 0x5A };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)data.Length);
                Assert.IsFalse(imageData.EndOfFile);
            }
        }

        [TestMethod]
        public void EndOfFile0()
        {
            var data = new byte[] { 0xA5, 0x5A };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)data.Length);
                imageData.GetNextBit();
                Assert.IsFalse(imageData.EndOfFile);
            }
        }

        [TestMethod]
        public void EndOfFile1()
        {
            var data = new byte[] { 0xA5, 0x5A };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)data.Length);
                imageData.GetSetOfBits(8);
                Assert.IsFalse(imageData.EndOfFile);
            }
        }

        [TestMethod]
        public void EndOfFile2()
        {
            var data = new byte[] { 0xA5, 0x5A };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)data.Length);
                imageData.GetSetOfBits(8);
                imageData.GetSetOfBits(8);
                Assert.IsTrue(imageData.EndOfFile);
            }
        }

        [TestMethod]
        public void RawData()
        {
            var data = new byte[] { 0xA5, 0x5A };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)data.Length);
                CollectionAssert.AreEqual(data, imageData.RawData);
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
                Assert.IsTrue(imageData.GetNextBit());
                Assert.IsFalse(imageData.GetNextBit());
                Assert.IsTrue(imageData.GetNextBit());
                Assert.IsFalse(imageData.GetNextBit());
                Assert.IsFalse(imageData.GetNextBit());
                Assert.IsTrue(imageData.GetNextBit());
                Assert.IsFalse(imageData.GetNextBit());
                Assert.IsTrue(imageData.GetNextBit());
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
                Assert.IsTrue(imageData.GetNextBit());
                Assert.IsFalse(imageData.GetNextBit());
                Assert.AreEqual(0x0009, imageData.GetSetOfBits(4));
                Assert.AreEqual(0x0001, imageData.GetSetOfBits(2));
                Assert.AreEqual(0x005A, imageData.GetSetOfBits(8));
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
                Assert.AreEqual(0x000A, imageData.GetSetOfBits(4));
                Assert.AreEqual(0x0055, imageData.GetSetOfBits(8));
                Assert.AreEqual(0x000A, imageData.GetSetOfBits(4));
            }
        }
    }
}