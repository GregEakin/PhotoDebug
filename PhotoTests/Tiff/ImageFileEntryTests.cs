// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		ImageFileEntryTests.cs
// AUTHOR:		Greg Eakin

namespace PhotoTests.Tiff
{
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Tiff;

    [TestClass]
    public class ImageFileEntryTests
    {
        #region Static Fields

        private static readonly byte[] Data = { 0x12, 0x00, 0x00, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, 0x40, 0x14 };

        #endregion

        #region Public Methods and Operators

        [TestMethod]
        public void NumberOfValue()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var imageFileEntry = new ImageFileEntry(reader);
                Assert.AreEqual(0x00010003u, imageFileEntry.NumberOfValue);
            }
        }

        [TestMethod]
        public void TagId()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var imageFileEntry = new ImageFileEntry(reader);
                Assert.AreEqual(0x0012, imageFileEntry.TagId);
            }
        }

        [TestMethod]
        public void TagType()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var imageFileEntry = new ImageFileEntry(reader);
                Assert.AreEqual(0x0100, imageFileEntry.TagType);
            }
        }

        [TestMethod]
        public void ValuePointer()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var imageFileEntry = new ImageFileEntry(reader);
                Assert.AreEqual(0x14400000u, imageFileEntry.ValuePointer);
            }
        }

        #endregion
    }
}