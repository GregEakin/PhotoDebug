// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		StartOfImageTests.cs
// AUTHOR:		Greg Eakin

using PhotoLib.Jpeg.JpegTags;

namespace PhotoTests.Jpeg.JpegTags
{
    using Xunit;
    using System;
    using System.IO;

    
    public class StartOfImageTests
    {
        [Fact]
        [ExpectedException(typeof(ArgumentException))]
        public void BadMarkTest()
        {
            var data = new byte[] { 0x00, 0x00 };
            using var memory = new MemoryStream(data);
            using var reader = new BinaryReader(memory);
            var startOfImage = new StartOfImage(reader, 0x0000, (uint)data.Length);
        }

        [Fact]
        [ExpectedException(typeof(ArgumentException))]
        public void BadTagTest()
        {
            var data = new byte[] { 0xFF, 0x00 };
            using var memory = new MemoryStream(data);
            using var reader = new BinaryReader(memory);
            var startOfImage = new StartOfImage(reader, 0x0000, (uint)data.Length);
        }

        [Fact]
        public void MarkTest()
        {
            var data = new byte[] { 0xFF, 0xD8 };
            using var memory = new MemoryStream(data);
            using var reader = new BinaryReader(memory);
            var startOfImage = new StartOfImage(reader, 0x0000, (uint)data.Length);
            Assert.Equal(0xFF, startOfImage.Mark);
        }

        [Fact]
        public void TagTest()
        {
            var data = new byte[] { 0xFF, 0xD8 };
            using var memory = new MemoryStream(data);
            using var reader = new BinaryReader(memory);
            var startOfImage = new StartOfImage(reader, 0x0000, (uint)data.Length);
            Assert.Equal(0xD8, startOfImage.Tag);
        }

        [Fact]
        public void EndOfImageTest()
        {
            var data = new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 };
            using var memory = new MemoryStream(data);
            using var reader = new BinaryReader(memory);
            var startOfImage = new StartOfImage(reader, 0x0000, (uint)data.Length);
        }

        [Fact]
        public void App1Test()
        {
            var data = new byte[] { 0xFF, 0xD8, 0xFF, 0xE1, 0x00, 0x04, 0x00, 0x00 };
            using var memory = new MemoryStream(data);
            using var reader = new BinaryReader(memory);
            var startOfImage = new StartOfImage(reader, 0x0000, (uint)data.Length);
        }
    }
}