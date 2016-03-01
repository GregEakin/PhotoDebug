using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class ExifData
    {
        [TestMethod]
        public void DumpExifData()
        {
            const string fileName = @"D:\Users\Greg\Pictures\2016-02-21 Studio\Studio 015.CR2";
            DumpExifInfo(fileName);
        }

        private static void DumpExifInfo(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var imageFileEntry = image.Entries.Single(e => e.TagId == 0x8769 && e.TagType == 4);
                var exif = imageFileEntry.ValuePointer;
                // Assert.AreEqual(446u, exif);

                DumpExifInfo(binaryReader, exif);
            }
        }

        private static void DumpExifInfo(BinaryReader binaryReader, uint offset)
        {
            binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

            var tags = new ImageFileDirectory(binaryReader);
            tags.DumpDirectory(binaryReader);
        }
    }
}
