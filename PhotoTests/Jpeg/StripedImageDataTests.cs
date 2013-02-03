namespace PhotoTests.Jpeg
{
    using System;
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Jpeg;

    [TestClass]
    public class StripedImageDataTests
    {
        #region Constants

        private const int Height = 3;

        private const int Width = 11;  // one byte for each color

        private const int X = 3;

        private const int Y = 3;

        private const int Z = 2;

        #endregion

        #region Static Fields

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

        #endregion

        #region Public Methods and Operators

        [TestMethod]
        public void GetBit()
        {
            var data = new byte[] { 0xA5 };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new StripedImageData(reader, (uint)data.Length, Width, Height, 0, 0, 0);
                Assert.IsTrue(imageData.GetNextBit());
                Assert.IsFalse(imageData.GetNextBit());
                Assert.IsTrue(imageData.GetNextBit());
                Assert.IsFalse(imageData.GetNextBit());
                Assert.IsFalse(imageData.GetNextBit());
                Assert.IsTrue(imageData.GetNextBit());
                Assert.IsFalse(imageData.GetNextBit());
                Assert.IsTrue(imageData.GetNextBit());
            }
        }

        [TestMethod]
        public void GetBits1()
        {
            var data = new byte[] { 0xA5, 0x5A };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new StripedImageData(reader, (uint)data.Length, Width, Height, 0, 0, 0);
                Assert.IsTrue(imageData.GetNextBit());
                Assert.IsFalse(imageData.GetNextBit());
                Assert.AreEqual(0x0009, imageData.GetSetOfBits(4));
            }
        }

        [TestMethod]
        public void GetBits2()
        {
            var data = new byte[] { 0xA5, 0x5A };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new StripedImageData(reader, (uint)data.Length, Width, Height, 0, 0, 0);
                Assert.IsTrue(imageData.GetNextBit());
                Assert.IsFalse(imageData.GetNextBit());
                Assert.AreEqual(0x0009, imageData.GetSetOfBits(4));
                Assert.AreEqual(0x0001, imageData.GetSetOfBits(2));
                Assert.AreEqual(0x005A, imageData.GetSetOfBits(8));
            }
        }

        [TestMethod]
        public void GetBits3()
        {
            var data = new byte[] { 0xA5, 0x5A };
            using (var memory = new MemoryStream(data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new StripedImageData(reader, (uint)data.Length, Width, Height, 0, 0, 0);
                Assert.AreEqual(0x000A, imageData.GetSetOfBits(4));
                Assert.AreEqual(0x0055, imageData.GetSetOfBits(8));
                Assert.AreEqual(0x000A, imageData.GetSetOfBits(4));
            }
        }

        // [TestMethod]
        public void RawData()
        {
            using (var memory = new MemoryStream(Data))
            {
                var reader = new BinaryReader(memory);
                var imageData = new StripedImageData(reader, (uint)Data.Length, Width, Height, 0, 0, 0);
                for (var i = 0; i < Data.Length; i++)
                {
                    var bits = imageData.GetNextByte();
                    Assert.AreEqual(Expected[i], bits);
                }
            }
        }

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
                    Console.WriteLine("Index = {0}, jidx={1}, i={2}, j={3}, r={4}, c={5}", row*Wide+col, jidx, i, j, row, col);
                }
            }

            CollectionAssert.AreEqual(Expected, buffer);
        }

        [TestMethod]
        public void TestMethodB8()
        {
            const int RawSize = Width * Height;
            const int Wide = Width * 1;

            var buffer = new byte[RawSize];

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
        #endregion
    }
}