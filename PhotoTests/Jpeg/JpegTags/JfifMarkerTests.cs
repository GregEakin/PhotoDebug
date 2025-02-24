﻿// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		JfifMarkerTests.cs
// AUTHOR:		Greg Eakin

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg.JpegTags;
using System.IO;

namespace PhotoTests.Jpeg.JpegTags
{
    [TestClass]
    public class JfifMarkerTests
    {
        [TestMethod]
        public void CtorTest()
        {
            var data = new byte[]
            {
                0xFF, 0xE0, 0x00, 0x16, 0x4A, 0x46, 0x49, 0x46, 0x00,
                0x12, 0x34, 0x03, 0x00, 0x05, 0x00, 0x06, 0x01, 0x02,
                0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            };

            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var jfifMarker = new JfifMarker(reader);
                Assert.AreEqual(0x0016, jfifMarker.Length);

                Assert.AreEqual(0x1234, jfifMarker.Version);
                Assert.AreEqual(0x03, jfifMarker.Units);
                Assert.AreEqual(0x0005, jfifMarker.DensityX);
                Assert.AreEqual(0x0006, jfifMarker.DensityY);
                Assert.AreEqual(0x01, jfifMarker.ThumbX);
                Assert.AreEqual(0x02, jfifMarker.ThumbY);
                CollectionAssert.AreEqual(new byte[] { 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F }, jfifMarker.Thumb);
            }
        }
    }
}