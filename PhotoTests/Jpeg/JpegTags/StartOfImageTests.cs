// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		StartOfImageTests.cs
// AUTHOR:		Greg Eakin

using PhotoLib.Jpeg.JpegTags;

namespace PhotoTests.Jpeg.JpegTags
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.IO;

    [TestClass]
    public class StartOfImageTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BadMarkTest()
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
        public void BadTagTest()
        {
            var data = new byte[] { 0xFF, 0x00 };
            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var startOfImage = new StartOfImage(reader, 0x0000, (uint)data.Length);
            }
        }

        [TestMethod]
        public void MarkTest()
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
        public void TagTest()
        {
            var data = new byte[] { 0xFF, 0xD8 };
            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var startOfImage = new StartOfImage(reader, 0x0000, (uint)data.Length);
                Assert.AreEqual(0xD8, startOfImage.Tag);
            }
        }

        [TestMethod]
        public void EndOfImageTest()
        {
            var data = new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 };
            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var startOfImage = new StartOfImage(reader, 0x0000, (uint)data.Length);
            }
        }

        [TestMethod]
        public void App1Test()
        {
            var data = new byte[] { 0xFF, 0xD8, 0xFF, 0xE1, 0x00, 0x04, 0x00, 0x00 };
            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var startOfImage = new StartOfImage(reader, 0x0000, (uint)data.Length);
            }
        }
    }
}