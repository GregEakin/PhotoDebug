// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		Ifid3Tests.cs
// AUTHOR:		Greg Eakin

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;
using System.IO;
using System.Linq;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class Ifid3Tests
    {
        [TestMethod]
        public void DumpImage3Test()
        {
            const string fileName = @"C:..\..\Photos\5DIIIhigh.CR2";
            DumpImage3(fileName);
        }

        private static void DumpImage3(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                var image = rawImage.Directories.Skip(3).First();
                Assert.AreEqual(7, image.Entries.Length);

                CollectionAssert.AreEqual(
                    new ushort[]
                    {
                        0x0103, 0x0111, 0x0117, 0xC5D8, 0xC5E0, 0xC640, 0xC6C5
                    },
                    image.Entries.Select(e => e.TagId).ToArray());

                var compression = image.Entries.Single(e => e.TagId == 0x0103 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(6u, compression);

                var offset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(0x2D42DCu, offset);

                var count = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(0x1501476u, count);

                var item3 = image.Entries.Single(e => e.TagId == 0xC5D8 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(0x01u, item3);

                var item4 = image.Entries.Single(e => e.TagId == 0xC5E0 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(0x01u, item4);

                var imageFileEntry = image.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3);
                // Assert.AreEqual(3u, imageFileEntry.NumberOfValue);
                // Assert.AreEqual(0x000119BEu, imageFileEntry.ValuePointer);
                var slices = RawImage.ReadUInts16(binaryReader, imageFileEntry);
                CollectionAssert.AreEqual(new ushort[] {1, 2960, 2960}, slices);

                var item6 = image.Entries.Single(e => e.TagId == 0xC6C5 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(0x01u, item6);
            }
        }
    }
}
