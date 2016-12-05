// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		CommentTests.cs
// AUTHOR:		Greg Eakin

using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg.JpegTags;
using PhotoLib.Tiff;

namespace PhotoTests.Jpeg.JpegTags
{
    [TestClass]
    public class CommentTests
    {
        [TestMethod]
        public void CtorTest()
        {
            var data = new byte[]
            {
                0xFF, 0xFE, 0x00, 0x08, 0x01, 0x02, 0x03, 0x04
            };

            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var comment = new Comment(reader);
                Assert.AreEqual(0x08, comment.Length);
                CollectionAssert.AreEqual(new byte[] { 0x01, 0x02, 0x03, 0x04 }, comment.Data);
            }
        }
    }
}