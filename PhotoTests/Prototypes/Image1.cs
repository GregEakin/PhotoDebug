// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		Image1.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class Image1
    {
        [TestMethod]
        public void DumpImage1Test()
        {
            const string Folder = @"C:\Users\Greg\Source\Repos\PhotoDebug\Samples\";
            DumpImage1(Folder, "311A6647.CR2");
        }

        private static void DumpImage1(string folder, string file)
        {
            var fileName2 = folder + file;
            using (var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // Images #0 and #1 are compressed in lossy (classic) JPEG

                var image = rawImage.Directories.Skip(1).First();
                Assert.AreEqual(2, image.Entries.Length);

                var offset = image.Entries.Single(e => e.TagId == 0x0201 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(80324u, offset);

                var length = image.Entries.Single(e => e.TagId == 0x0202 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(10334u, length);

                binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
                DumpImage2(folder + "0L2A8897-1.JPG", binaryReader, length);
            }
        }

        private static void DumpImage2(string output, BinaryReader binaryReader, uint length)
        {
            using (var outFile = File.Create(output))
            {
                var bytes = (int)length;
                var buffer = new byte[32768];
                int read;
                while (bytes > 0 && (read = binaryReader.BaseStream.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
                {
                    outFile.Write(buffer, 0, read);
                    bytes -= read;
                }
            }
        }
    }
}
