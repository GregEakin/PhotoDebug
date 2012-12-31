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
    }
}