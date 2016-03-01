using PhotoLib.Jpeg.JpegTags;

namespace PhotoTests.Jpeg
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.IO;

    [TestClass]
    public class StartOfScanTests
    {
        #region Public Methods and Operators

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BadMark()
        {
            var data = new byte[] { 0x00, 0x00 };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var startOfScan = new StartOfScan(reader);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BadTag()
        {
            var data = new byte[] { 0xFF, 0x00 };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var startOfScan = new StartOfScan(reader);
            }
        }

        [TestMethod]
        public void Mark()
        {
            var data = new byte[] { 0xFF, 0xDA, 0x00, 0x06, 0x00, 0x00, 0x00, 0x00 };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var startOfScan = new StartOfScan(reader);
                Assert.AreEqual(0xFF, startOfScan.Mark);
            }
        }

        [TestMethod]
        public void Tag()
        {
            var data = new byte[] { 0xFF, 0xDA, 0x00, 0x06, 0x00, 0x00, 0x00, 0x00 };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var startOfScan = new StartOfScan(reader);
                Assert.AreEqual(0xDA, startOfScan.Tag);
            }
        }

        #endregion
    }
}