// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		StartOfFrameTests.cs
// AUTHOR:		Greg Eakin

namespace PhotoTests.Jpeg
{
    using System;
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Jpeg;

    [TestClass]
    public class StartOfFrameTests
    {
        private static readonly byte[] Data =
            {
                0xFF, 0xC3, 0x00, 0x14, 0x0E, 0x0D, 0xBC, 0x05, 0x3C, 0x04, 0x01, 0x11, 0x00, 0x02, 0x11, 0x00, 0x03, 0x11, 0x00, 0x04, 0x11, 0x00
            };

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BadMark()
        {
            var badData = new byte[] { 0x00, 0x00 };
            using (var memory = new MemoryStream(badData))
            using (var reader = new BinaryReader(memory))
            {
                var lossless = new StartOfFrame(reader);
            }
        }

        [TestMethod]
        public void Mark()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var lossless = new StartOfFrame(reader);
                Assert.AreEqual(0xFF, lossless.Mark);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BadTag()
        {
            var badData = new byte[] { 0xFF, 0x00 };
            using (var memory = new MemoryStream(badData))
            using (var reader = new BinaryReader(memory))
            {
                var lossless = new StartOfFrame(reader);
            }
        }

        [TestMethod]
        public void Tag()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var lossless = new StartOfFrame(reader);
                Assert.AreEqual(0xC3, lossless.Tag);
            }
        }

        [TestMethod]
        public void Length()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var lossless = new StartOfFrame(reader);
                Assert.AreEqual(0x0014, lossless.Length);
            }
        }

        [TestMethod]
        public void Precision()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var lossless = new StartOfFrame(reader);
                Assert.AreEqual(0x0E, lossless.Precision);
            }
        }

        [TestMethod]
        public void ScanLines()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var lossless = new StartOfFrame(reader);
                Assert.AreEqual(0x0DBC, lossless.ScanLines);
            }
        }

        [TestMethod]
        public void SamplesPerLine()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var lossless = new StartOfFrame(reader);
                Assert.AreEqual(0x053C, lossless.SamplesPerLine);
            }
        }

        [TestMethod]
        public void ComponentCount()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var lossless = new StartOfFrame(reader);
                Assert.AreEqual(0x04, lossless.Components.Length);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(EndOfStreamException))]
        public void ShortLength()
        {
            var badData = new byte[]
            {
                0xFF, 0xC3, 0x00, 0x07, 0x0E, 0x0D, 0xBC, 0x05, 0x3C
            };

            using (var memory = new MemoryStream(badData))
            using (var reader = new BinaryReader(memory))
            {
                var lossless = new StartOfFrame(reader);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LongLength()
        {
            var badData = new byte[]
            {
                0xFF, 0xC3, 0x00, 0x09, 0x0E, 0x0D, 0xBC, 0x05, 0x3C, 0x00
            };

            using (var memory = new MemoryStream(badData))
            using (var reader = new BinaryReader(memory))
            {
                var lossless = new StartOfFrame(reader);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ShortComponentCount()
        {
            var badData = new byte[]
            {
                0xFF, 0xC3, 0x00, 0x0C, 0x0E, 0x0D, 0xBC, 0x05, 0x3C, 0x01, 0x01, 0x11, 0x00, 0x02
            };

            using (var memory = new MemoryStream(badData))
            using (var reader = new BinaryReader(memory))
            {
                var lossless = new StartOfFrame(reader);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(EndOfStreamException))]
        public void LongComponentCount()
        {
            var badData = new byte[]
            {
                0xFF, 0xC3, 0x00, 0x0A, 0x0E, 0x0D, 0xBC, 0x05, 0x3C, 0x01, 0x01, 0x11
            };

            using (var memory = new MemoryStream(badData))
            using (var reader = new BinaryReader(memory))
            {
                var lossless = new StartOfFrame(reader);
            }
        }

        [TestMethod]
        public void ComponentId()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var lossless = new StartOfFrame(reader);
                Assert.AreEqual(0x01, lossless.Components[0].ComponentId);
            }
        }

        [TestMethod]
        public void TableId()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var lossless = new StartOfFrame(reader);
                Assert.AreEqual(0x00, lossless.Components[0].TableId);
            }
        }

        [TestMethod]
        public void HFactor()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var lossless = new StartOfFrame(reader);
                Assert.AreEqual(0x01, lossless.Components[0].HFactor);
            }
        }

        [TestMethod]
        public void VFactor()
        {
            using (var memory = new MemoryStream(Data))
            using (var reader = new BinaryReader(memory))
            {
                var lossless = new StartOfFrame(reader);
                Assert.AreEqual(0x01, lossless.Components[0].VFactor);
            }
        }
    }
}