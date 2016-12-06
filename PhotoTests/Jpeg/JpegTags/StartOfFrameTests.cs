// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		StartOfFrameTests.cs
// AUTHOR:		Greg Eakin

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg;

namespace PhotoTests.Jpeg.JpegTags
{
    [TestClass]
    public class StartOfFrameTests
    {
        [TestMethod]
        public void CtorTest()
        {
            var data = new byte[]
            {
                0xFF, 0xC0, 0x00, 0x0E, 0x12, 0x12, 0x34, 0x43, 0x21,
                0x02, 0x01, 0x24, 0x03, 0x02, 0x75, 0x06
            };

            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var startOfFrame = new StartOfFrame(reader);

                Assert.AreEqual(0x000E, startOfFrame.Length);

                Assert.AreEqual(0x12, startOfFrame.Precision);
                Assert.AreEqual(0x1234, startOfFrame.ScanLines);
                Assert.AreEqual(0x4321, startOfFrame.SamplesPerLine);

                Assert.AreEqual(2, startOfFrame.Components.Length);

                Assert.AreEqual(0x01, startOfFrame.Components[0].ComponentId);
                Assert.AreEqual(0x02, startOfFrame.Components[0].HFactor);
                Assert.AreEqual(0x04, startOfFrame.Components[0].VFactor);
                Assert.AreEqual(0x03, startOfFrame.Components[0].TableId);

                Assert.AreEqual(0x02, startOfFrame.Components[1].ComponentId);
                Assert.AreEqual(0x07, startOfFrame.Components[1].HFactor);
                Assert.AreEqual(0x05, startOfFrame.Components[1].VFactor);
                Assert.AreEqual(0x06, startOfFrame.Components[1].TableId);
            }
        }
    }
}