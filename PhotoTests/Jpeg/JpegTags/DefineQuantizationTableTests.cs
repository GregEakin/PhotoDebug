// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		DefineQuantizationTableTests.cs
// AUTHOR:		Greg Eakin

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg.JpegTags;

namespace PhotoTests.Jpeg.JpegTags
{
    [TestClass]
    public class DefineQuantizationTableTests
    {
        [TestMethod]
        public void CtorTest()
        {
            var data = new byte[]
            {
                0xFF, 0xDB, 0x00, 0x02
            };

            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var defineQuantizationTable = new DefineQuantizationTable(reader);
                Assert.AreEqual(0x0002, defineQuantizationTable.Length);
            }
        }

        [TestMethod]
        public void CtorTest2()
        {
            var data = new byte[]
            {
                0xFF, 0xDB, 0x00, 0x43, 0x12,
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
                0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
                0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
                0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
                0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
                0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37,
                0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
            };

            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var defineQuantizationTable = new DefineQuantizationTable(reader);
                Assert.AreEqual(0x0043, defineQuantizationTable.Length);

                Assert.AreEqual(1, defineQuantizationTable.Dictionary.Count);
                var table = defineQuantizationTable.Dictionary[0x12];
                Assert.AreEqual(64, table.Length);
                for (var i = 0; i < table.Length; i++)
                    Assert.AreEqual(i, table[i]);
            }
        }
    }
}