// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		RawDataTests.cs
// AUTHOR:		Greg Eakin

using System.IO;
using Xunit;
using PhotoLib.Tiff;

namespace PhotoTests.Tiff
{
    public class RawDataTests
    {
        [Fact]
        public void VerticalSlices()
        {
            var data = new byte[]
                {
                    0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
                    0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28,
                    0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38,
                    0x40, 0x41, 0x42
                };

            var expected = new byte[]
                {
                    0x10, 0x11, 0x12, 0x20, 0x21, 0x22, 0x30, 0x31, 0x32, 0x40,
                    0x13, 0x14, 0x15, 0x23, 0x24, 0x25, 0x33, 0x34, 0x35, 0x41,
                    0x16, 0x17, 0x18, 0x26, 0x27, 0x28, 0x36, 0x37, 0x38, 0x42
                };

            using var memoryStream = new MemoryStream(data);
            using var binaryReader = new BinaryReader(memoryStream);
            const int Height = 3;
            const int Width = 10;
            const int X = 3;
            const int Y = 3;
            const int Z = 1;
            Assert.Equal(Width, X * Y + Z);

            var rawData = new RawData(binaryReader, Height, X, Y, Z);

            Assert.Equal(expected, rawData.Data);
        }
    }
}