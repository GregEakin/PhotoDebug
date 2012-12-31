namespace PhotoTests.Jpeg
{
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Jpeg;

    [TestClass]
    public class StripedImageDataTests
    {
        #region Static Fields

        const int Height = 3;

        const int Width = 10;

        private static readonly byte[] Data = new byte[]
                {
                    0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
                    0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28,
                    0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38,
                    0x40, 0x41, 0x42
                };

        private static readonly byte[] Expected = new byte[]
                {
                    0x10, 0x11, 0x12, 0x20, 0x21, 0x22, 0x30, 0x31, 0x32, 0x40,
                    0x13, 0x14, 0x15, 0x23, 0x24, 0x25, 0x33, 0x34, 0x35, 0x41,
                    0x16, 0x17, 0x18, 0x26, 0x27, 0x28, 0x36, 0x37, 0x38, 0x42
                };

        #endregion

        #region Public Methods and Operators

        [TestMethod]
        public void GetBit()
        {
            var data = new byte[] { 0xA5 };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new StripedImageData(reader, (uint)data.Length, Width, Height);
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
        public void GetBits1()
        {
            var data = new byte[] { 0xA5, 0x5A };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new StripedImageData(reader, (uint)data.Length, Width, Height);
                Assert.IsTrue(imageData.GetNextBit());
                Assert.IsFalse(imageData.GetNextBit());
                Assert.AreEqual(0x0009, imageData.GetSetOfBits(4));
            }
        }

        [TestMethod]
        public void GetBits2()
        {
            var data = new byte[] { 0xA5, 0x5A };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new StripedImageData(reader, (uint)data.Length, Width, Height);
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
                var imageData = new StripedImageData(reader, (uint)data.Length, Width, Height);
                Assert.AreEqual(0x000A, imageData.GetSetOfBits(4));
                Assert.AreEqual(0x0055, imageData.GetSetOfBits(8));
                Assert.AreEqual(0x000A, imageData.GetSetOfBits(4));
            }
        }

        // [TestMethod]
        public void RawData()
        {
            using (var memory = new MemoryStream(Data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new StripedImageData(reader, (uint)Data.Length, Width, Height);
                for (var i = 0; i < Data.Length; i++)
                {
                    var bits = imageData.GetNextByte();
                    Assert.AreEqual(Expected[i], bits);
                }
            }
        }

        #endregion
    }
}