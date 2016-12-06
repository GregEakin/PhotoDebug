// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		StartOfImageTests.cs
// AUTHOR:		Greg Eakin

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg.JpegTags;

namespace PhotoTests.Jpeg.JpegTags
{
    [TestClass]
    public class StartOfImageTests
    {
        [TestMethod]
        public void CtorTest()
        {
            var data = new byte[]
            {
                0xFF, 0xD8
            };

            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var startOfImage = new StartOfImage(reader, 0, (uint)data.Length);
            }
        }

        [TestMethod]
        public void EndOfImageTest()
        {
            var data = new byte[]
            {
                0xFF, 0xD8, 0xFF, 0xD9
            };

            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var startOfImage = new StartOfImage(reader, 0, (uint)data.Length);
            }
        }

        [TestMethod]
        public void App1Test()
        {
            var data = new byte[]
            {
                0xFF, 0xD8, 0xFF, 0xE1, 0x00, 0x04, 0x00, 0x00
            };

            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var startOfImage = new StartOfImage(reader, 0, (uint)data.Length);
            }
        }
    }
}