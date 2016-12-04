// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		CR2HeaderTests.cs
// AUTHOR:		Greg Eakin

namespace PhotoTests.Tiff
{
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Tiff;

    [TestClass]
    public class CR2HeaderTests
    {
        #region Static Fields

        private static readonly byte[] Data = { 0x49, 0x49, 0x2A, 0x00, 0x10, 0x00, 0x00, 0x00, 0x43, 0x52, 0x02, 0x00, 0x46, 0xBF, 0x00, 0x00 };

        #endregion

        #region Public Methods and Operators

        [TestMethod]
        public void ByteOrder()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var cr2Header = new CR2Header(reader);
                CollectionAssert.AreEqual(new byte[] { 0x49, 0x49 }, cr2Header.ByteOrder);
            }
        }

        [TestMethod]
        public void CR2Magic()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var cr2Header = new CR2Header(reader);
                Assert.AreEqual(0x5243, cr2Header.CR2Magic); // "CR2\0"
            }
        }

        [TestMethod]
        public void CR2Version()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var cr2Header = new CR2Header(reader);
                CollectionAssert.AreEqual(new byte[] { 0x02, 0x00 }, cr2Header.CR2Version);
            }
        }

        [TestMethod]
        public void RawIfdOffset()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var cr2Header = new CR2Header(reader);
                Assert.AreEqual(0x0000BF46u, cr2Header.RawIfdOffset);
            }
        }

        [TestMethod]
        public void TiffMagic()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var cr2Header = new CR2Header(reader);
                Assert.AreEqual(0x002A, cr2Header.TiffMagic);
            }
        }

        [TestMethod]
        public void TiffOffset()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var cr2Header = new CR2Header(reader);
                Assert.AreEqual(0x00000010u, cr2Header.TiffOffset);
            }
        }

        #endregion
    }
}