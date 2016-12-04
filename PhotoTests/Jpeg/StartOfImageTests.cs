// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		StartOfImageTests.cs
// AUTHOR:		Greg Eakin

using PhotoLib.Jpeg.JpegTags;

namespace PhotoTests.Jpeg
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.IO;

    [TestClass]
    public class StartOfImageTests
    {
        #region Public Methods and Operators

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BadMark()
        {
            var data = new byte[] { 0x00, 0x00 };
            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var startOfImage = new StartOfImage(reader, 0x0000, (uint)data.Length);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BadTag()
        {
            var data = new byte[] { 0xFF, 0x00 };
            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var startOfImage = new StartOfImage(reader, 0x0000, (uint)data.Length);
            }
        }

        [TestMethod]
        public void Mark()
        {
            var data = new byte[] { 0xFF, 0xD8 };
            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var startOfImage = new StartOfImage(reader, 0x0000, (uint)data.Length);
                Assert.AreEqual(0xFF, startOfImage.Mark);
            }
        }

        [TestMethod]
        public void Tag()
        {
            var data = new byte[] { 0xFF, 0xD8 };
            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var startOfImage = new StartOfImage(reader, 0x0000, (uint)data.Length);
                Assert.AreEqual(0xD8, startOfImage.Tag);
            }
        }

        #endregion
    }
}