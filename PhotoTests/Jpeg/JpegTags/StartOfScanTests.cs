// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		StartOfScanTests.cs
// AUTHOR:		Greg Eakin

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg.JpegTags;

namespace PhotoTests.Jpeg.JpegTags
{
    [TestClass]
    public class StartOfScanTests
    {
        [TestMethod]
        public void CtorTest()
        {
            var data = new byte[]
            {
                0xFF, 0xDA, 0x00, 0x0A, 0x02, 0x01, 0x34, 0x02, 0x65,
                0xAA, 0xBB, 0xCC
            };

            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var startOfScan = new StartOfScan(reader);

                Assert.AreEqual(0x000A, startOfScan.Length);

                var components = startOfScan.Components;
                Assert.AreEqual(2, components.Length);
                Assert.AreEqual(01, components[0].Id);
                Assert.AreEqual(03, components[0].Dc);
                Assert.AreEqual(04, components[0].Ac);
                Assert.AreEqual(02, components[1].Id);
                Assert.AreEqual(06, components[1].Dc);
                Assert.AreEqual(05, components[1].Ac);

                Assert.AreEqual(0xAA, startOfScan.Bb1);
                Assert.AreEqual(0xBB, startOfScan.Bb2);
                Assert.AreEqual(0xCC, startOfScan.Bb3);
            }
        }
    }
}