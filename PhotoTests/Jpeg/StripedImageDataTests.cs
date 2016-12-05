// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		StripedImageDataTests.cs
// AUTHOR:		Greg Eakin

namespace PhotoTests.Jpeg
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StripedImageDataTests
    {
        private const int Height = 3;

        private const int Width = 11;  // one byte for each color

        private const int X = 3;

        private const int Y = 3;

        private const int Z = 2;

        private static readonly byte[] Data = new byte[]
            {
                0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 
                0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 
                0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 
                0x40, 0x41, 0x42, 0x43, 0x44, 0x45
            };

        private static readonly byte[] Expected = new byte[]
            {
                0x10, 0x11, 0x12, 0x20, 0x21, 0x22, 0x30, 0x31, 0x32, 0x40, 0x41,
                0x13, 0x14, 0x15, 0x23, 0x24, 0x25, 0x33, 0x34, 0x35, 0x42, 0x43,
                0x16, 0x17, 0x18, 0x26, 0x27, 0x28, 0x36, 0x37, 0x38, 0x44, 0x45
            };

        [TestMethod]
        public void TestMethodB7()
        {
            const int RawSize = Width * Height;
            const int Wide = Width * 1;

            var buffer = new byte[RawSize];

            for (var jrow = 0; jrow < Height; jrow++)
            {
                for (var jcol = 0; jcol < Wide; jcol++)    // Width *= colors
                {
                    var jidx = jrow * Wide + jcol;
                    var val = Data[jidx];

                    var i = jidx / (Y * Height);
                    var j = jidx % (Y * Height);
                    var row = j / (i < X ? X : Z);
                    var col = j % (i < X ? X : Z) + i * Y;

                    buffer[row * Wide + col] = val;
                    Console.WriteLine("Index = {0}, jidx={1}, i={2}, j={3}, r={4}, c={5}", row * Wide + col, jidx, i, j, row, col);
                }
            }

            CollectionAssert.AreEqual(Expected, buffer);
        }

        [TestMethod]
        public void TestMethodB8()
        {
            const int RawSize = Width * Height;
            const int Wide = Width * 1;

            // var buffer = new byte[RawSize];

            for (var index = 0; index < RawSize; index++)
            {
                var i = index % Wide;
                var j = index / Wide;

                var i1 = i % X;
                var j1 = i / X;
                var j2 = i1 + X * Height * j1 + j * X;

                // buffer[index] = val;

                Console.WriteLine("Index = {0}, i={1}, j={2}, i1={3}, j1={4}, j2={5}", index, i, j, i1, j1, j2.ToString("X2"));
            }

            // CollectionAssert.AreEqual(Expected, buffer);
        }

        [TestMethod]
        public void Test1()
        {
            var i = 0;

            for (var x = 0; x < X; x++)
            {
                for (var jrow = 0; jrow < Height; jrow++)
                {
                    for (var y = 0; y < Y; y++)
                    {
                        var index = x * Y + jrow * Width + y;
                        Assert.AreEqual(Expected[index], Data[i++]);
                    }
                }
            }
            for (var jrow = 0; jrow < Height; jrow++)
            {
                for (var z = 0; z < Z; z++)
                {
                    var index = X * Y + jrow * Width + z;
                    Assert.AreEqual(Expected[index], Data[i++]);
                }
            }
        }

        [TestMethod]
        public void Test2()
        {
            var i = 0;
            for (var x = 0; x < X; x++)
            {
                for (var jrow = 0; jrow < Height; jrow++)
                {
                    for (var y = 0; y < Y; y++)
                    {
                        var index = x * Y + jrow * Width + y;

                        var x2 = i / (Height * Y);
                        var jrow2 = (i - x2 * Height * Y) / Y;
                        var y2 = (i - x2 * Height * Y) % Y;
                        Assert.AreEqual(x, x2);
                        Assert.AreEqual(jrow, jrow2);
                        Assert.AreEqual(y, y2);

                        Assert.AreEqual(Expected[index], Data[i]);
                        i++;
                    }
                }
            }
            for (var jrow = 0; jrow < Height; jrow++)
            {
                for (var y = 0; y < Z; y++)
                {
                    var index = X * Y + jrow * Width + y;

                    var jrow2 = (i - X * Height * Y) / Z;
                    var y2 = (i - X * Height * Y) % Z;
                    Assert.AreEqual(jrow, jrow2);
                    Assert.AreEqual(y, y2);

                    Assert.AreEqual(Expected[index], Data[i]);
                    i++;
                }
            }
        }

        [TestMethod]
        public void Test3()
        {
            for (var i = 0; i < Width * Height; i++)
            {
                var x2 = i / (Height * Y);
                if (x2 < X)
                {
                    var jrow2 = (i - x2 * Height * Y) / Y;
                    var y2 = (i - x2 * Height * Y) % Y;

                    var index = x2 * Y + jrow2 * Width + y2;
                    Assert.AreEqual(Expected[index], Data[i]);
                }
                else
                {
                    var jrow2 = (i - x2 * Height * Y) / Z;
                    var y2 = (i - x2 * Height * Y) % Z;

                    var index = x2 * Y + jrow2 * Width + y2;
                    Assert.AreEqual(Expected[index], Data[i]);
                }
            }
        }
    }
}