using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class Image0
    {
        [TestMethod]
        public void DumpImage0Test()
        {
            const string Folder = @"D:\Users\Greg\Pictures\2016-02-21 Studio\";
            DumpImage0(Folder, "Studio 015.CR2");
        }

        private static void DumpImage0(string folder, string file)
        {
            var fileName2 = folder + file;
            using (var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                // Images #0 and #1 are compressed in lossy (classic) JPEG
                {
                    var image = rawImage.Directories.First();
                    Assert.AreEqual(18, image.Entries.Length);

                    var imageWidth = image.Entries.Single(e => e.TagId == 0x0100 && e.TagType == 3).ValuePointer;
                    Assert.AreEqual(5760u, imageWidth);
                
                    var imageLength = image.Entries.Single(e => e.TagId == 0x0101 && e.TagType == 3).ValuePointer;
                    Assert.AreEqual(3840u, imageLength);

                    var imageFileEntry0102 = image.Entries.Single(e => e.TagId == 0x0102 && e.TagType == 3);
                    Assert.AreEqual(238u, imageFileEntry0102.ValuePointer);
                    Assert.AreEqual(3u, imageFileEntry0102.NumberOfValue);
                    var bitsPerSample = RawImage.ReadUInts16(binaryReader, imageFileEntry0102);
                    CollectionAssert.AreEqual(new[] { (ushort)8, (ushort)8, (ushort)8 }, bitsPerSample);

                    var compression = image.Entries.Single(e => e.TagId == 0x0103 && e.TagType == 3).ValuePointer;
                    Assert.AreEqual(6u, compression);

                    var imageFileEntry010F = image.Entries.Single(e => e.TagId == 0x010F && e.TagType == 2);
                    var len0104 = imageFileEntry010F.NumberOfValue;
                    var xx0104 = imageFileEntry010F.ValuePointer;
                    //var identifer = binaryReader.ReadBytes(len0104);
                    //Assert.AreEqual("Canon", Encoding.ASCII.GetString(identifer));

                    var make = image.Entries.Single(e => e.TagId == 0x010f && e.TagType == 2).ValuePointer;
                    //Assert.AreEqual("Canon", make);

                    var imageFileEntry0110 = image.Entries.Single(e => e.TagId == 0x0110 && e.TagType == 2);
                    var model = image.Entries.Single(e => e.TagId == 0x0110 && e.TagType == 2).ValuePointer;
                    //Assert.AreEqual("Canon EOS 5D", model);

                    var stripOffset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(90660u, stripOffset);

                    var orientation = image.Entries.Single(e => e.TagId == 0x0112 && e.TagType == 3).ValuePointer;
                    // Assert.AreEqual(1u, orientation);

                    var stripByteCounts = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(1138871u, length);

                    var xResolution = image.Entries.Single(e => e.TagId == 0x011A && e.TagType == 5).ValuePointer;
                    Assert.AreEqual(282u, xResolution);

                    var yResolution = image.Entries.Single(e => e.TagId == 0x011B && e.TagType == 5).ValuePointer;
                    Assert.AreEqual(290u, yResolution);

                    var imageFileEntry0128 = image.Entries.Single(e => e.TagId == 0x0128 && e.TagType == 3);
                    // Assert.AreEqual(2u, imageFileEntry0128.ValuePointer);
                    // Assert.AreEqual(1u, imageFileEntry0128.NumberOfValue);
                    var resolutionUnit = RawImage.ReadUInts16(binaryReader, imageFileEntry0128);
                    CollectionAssert.AreEqual(new[] { (ushort)42 }, resolutionUnit);

                    //var dateTime = image.Entries.Single(e => e.TagId == 0x0132 && e.TagType == 2).ValuePointer;
                    //Assert.AreEqual("1234", dateTime);

                    var exipf = image.Entries.Single(e => e.TagId == 0x8769 && e.TagType == 4).ValuePointer;
                    Assert.AreEqual(446u, exipf);

                    var gps = image.Entries.Single(e => e.TagId == 0x8825 && e.TagType == 4).ValuePointer;
                    Assert.AreEqual(70028u, gps);

                    DumpImage(binaryReader, folder + "0L2A8897-0.JPG", stripOffset, stripByteCounts);
                }
            }
        }

        private static void DumpImage(BinaryReader binaryReader, string filename, uint offset, uint length)
        {
            using (var fout = File.Open(filename, FileMode.Create, FileAccess.Write))
            {
                binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

                //Create a byte array to act as a buffer
                var buffer = new byte[32];
                for (var i = 0; i < length;)
                {
                    //Read from the source file
                    //The Read method returns the number of bytes read
                    var n = binaryReader.Read(buffer, 0, buffer.Length);

                    //Write the contents of the buffer to the destination file
                    fout.Write(buffer, 0, n);

                    i += n;
                }

                //Flush the contents of the buffer to the file
                fout.Flush();
            }
        }
    }
}
