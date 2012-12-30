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
        [ExpectedException(typeof(ArgumentException))]
        public void BadMark()
        {
            var badData = new byte[] { 0x00, 0x00 };
            using (var memory = new MemoryStream(badData))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)Data.Length - 2u);
            }
        }

        [TestMethod]
        public void Mark()
        {
            using (var memory = new MemoryStream(Data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)Data.Length - 2u);
                Assert.AreEqual(0xFE, imageData.Mark);
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
                var imageData = new ImageData(reader, (uint)Data.Length - 2u);
            }
        }

        [TestMethod]
        public void Tag()
        {
            using (var memory = new MemoryStream(Data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)Data.Length - 2u);
                Assert.AreEqual(0xD5, imageData.Tag);
            }
        }

        [TestMethod]
        public void RawData()
        {
            using (var memory = new MemoryStream(Data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new ImageData(reader, (uint)Data.Length - 2u);
                CollectionAssert.AreEqual(Data.Skip(2).ToArray(), imageData.RawData);
            }
        }
    }
}