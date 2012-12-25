namespace PhotoTests
{
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib;

    [TestClass]
    public class TawImageTests
    {
        #region Public Methods and Operators

        [TestMethod]
        public void ReadEntryTest()
        {
            var data = new byte[] { 0x55, 0xAA, 0xAA, 0x55, 0x01, 0x02, 0x03, 0x4, 0xFF, 0xFE, 0xFD, 0xFC };
            using (var memoryStream = new MemoryStream(data))
            {
                var binaryReader = new BinaryReader(memoryStream);
                var entry = new ImageFileEntry(binaryReader);
                Assert.AreEqual(0xAA55, entry.TagId);
                Assert.AreEqual(0x55AA, entry.TagType);
                Assert.AreEqual(0x04030201u, entry.NumberOfValue);
                Assert.AreEqual(0xFCFDFEFFu, entry.ValuePointer);
            }
        }

        [TestMethod]
        public void ReadHeaderTest()
        {
            var data = new byte[] { 0x55, 0xAA, 0x81, 0x7E, 0x01, 0x02, 0x03, 0x4, 0x11, 0x22, 0xAA, 0x55, 0xFF, 0xFE, 0x01, 0x02 };
            using (var memoryStream = new MemoryStream(data))
            {
                var binaryReader = new BinaryReader(memoryStream);
                var header = new CR2Header(binaryReader);
                CollectionAssert.AreEqual(new byte[] { 0x55, 0xAA }, header.ByteOrder);
                Assert.AreEqual(0x7E81, header.TiffMagic);
                Assert.AreEqual(0x04030201u, header.TiffOffset);
                Assert.AreEqual(0x2211, header.CR2Magic);
                CollectionAssert.AreEqual(new byte[] { 0xAA, 0x55 }, header.CR2Version);
                Assert.AreEqual(0x0201FEFFu, header.RawIfdOffset);
            }
        }

        #endregion
    }
}