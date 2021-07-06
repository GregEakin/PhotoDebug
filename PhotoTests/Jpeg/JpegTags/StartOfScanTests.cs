// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		StartOfScanTests.cs
// AUTHOR:		Greg Eakin

using PhotoLib.Jpeg.JpegTags;
using System;
using System.IO;
using Xunit;

namespace PhotoTests.Jpeg.JpegTags
{
    public class StartOfScanTests
    {
        [Fact]
        public void BadMarkTest()
        {
            var data = new byte[] { 0x00, 0x00 };
            using var memory = new MemoryStream(data);
            using var reader = new BinaryReader(memory);
            Assert.Throws<ArgumentException>(() => new StartOfScan(reader));
        }

        [Fact]
        public void BadTagTest()
        {
            var data = new byte[] { 0xFF, 0x00 };
            using var memory = new MemoryStream(data);
            using var reader = new BinaryReader(memory);
            Assert.Throws<ArgumentException>(() => new StartOfScan(reader));
        }

        [Fact]
        public void MarkTest()
        {
            var data = new byte[] { 0xFF, 0xDA, 0x00, 0x06, 0x00, 0x00, 0x00, 0x00 };
            using var memory = new MemoryStream(data);
            using var reader = new BinaryReader(memory);
            var startOfScan = new StartOfScan(reader);
            Assert.Equal(0xFF, startOfScan.Mark);
        }

        [Fact]
        public void TagTest()
        {
            var data = new byte[] { 0xFF, 0xDA, 0x00, 0x06, 0x00, 0x00, 0x00, 0x00 };
            using var memory = new MemoryStream(data);
            using var reader = new BinaryReader(memory);
            var startOfScan = new StartOfScan(reader);
            Assert.Equal(0xDA, startOfScan.Tag);
        }

        [Fact]
        public void ComponentTest()
        {
            var data = new byte[]
            {
                0xFF, 0xDA, 0x00, 0x0A, 0x02, 0x01, 0x34, 0x02, 0x65,
                0xAA, 0xBB, 0xCC
            };

            using var memory = new MemoryStream(data);
            using var reader = new BinaryReader(memory);
            var startOfScan = new StartOfScan(reader);

            Assert.Equal(0x000A, startOfScan.Length);

            var components = startOfScan.Components;
            Assert.Equal(2, components.Length);
            Assert.Equal(01, components[0].Id);
            Assert.Equal(03, components[0].Dc);
            Assert.Equal(04, components[0].Ac);
            Assert.Equal(02, components[1].Id);
            Assert.Equal(06, components[1].Dc);
            Assert.Equal(05, components[1].Ac);

            Assert.Equal(0xAA, startOfScan.Bb1);
            Assert.Equal(0xBB, startOfScan.Bb2);
            Assert.Equal(0xCC, startOfScan.Bb3);
        }
    }
}