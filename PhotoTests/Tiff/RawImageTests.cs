// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		RawImageTests.cs
// AUTHOR:		Greg Eakin

namespace PhotoTests.Tiff
{
    using System;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Tiff;

    [TestClass]
    public class RawImageTests
    {
        [TestMethod]
        public void Header()
        {
            var data = new byte[]
                {
                    0x49, 0x49, 0x2A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x43, 0x52, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00
                };
            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var rawImage = new RawImage(reader);
                var cr2Header = rawImage.Header;
                Assert.AreEqual(0x5243, cr2Header.CR2Magic);
            }
        }

        [TestMethod]
        public void Directory()
        {
            var data = new byte[]
                {
                    0x49, 0x49, 0x2A, 0x00, 0x10, 0x00, 0x00, 0x00, 0x43, 0x52, 0x02, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00
                };
            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var rawImage = new RawImage(reader);
                var directory = rawImage.Directories.First();
                Assert.AreEqual(0, directory.Entries.Length);
                Assert.AreEqual(0x00000000u, directory.NextEntry);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DirectoryDuplicate()
        {
            var data = new byte[]
                {
                    0x49, 0x49, 0x2A, 0x00, 0x10, 0x00, 0x00, 0x00, 0x43, 0x52, 0x02, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x10, 0x00, 0x00, 0x00
                };
            using (var memory = new MemoryStream(data))
            using (var reader = new BinaryReader(memory))
            {
                var rawImage = new RawImage(reader);
                var directory = rawImage.Directories.First();
                Assert.AreEqual(0, directory.Entries.Length);
                Assert.AreEqual(0x00000000u, directory.NextEntry);
            }
        }
    }
}