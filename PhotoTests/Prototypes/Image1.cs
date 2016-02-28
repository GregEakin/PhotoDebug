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
            const string Folder = @"D:\Users\Greg\Pictures\2016-02-21 Studio\";
            DumpImage1(Folder, "Studio 015.CR2");
        }

        private static void DumpImage1(string folder, string file)
        {
            var fileName2 = folder + file;
            using (var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                // Images #0 and #1 are compressed in lossy (classic) JPEG
                {
                    var image = rawImage.Directories.Skip(1).First();
                    var offset = image.Entries.Single(e => e.TagId == 0x0201 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(80324u, offset);
                    var length = image.Entries.Single(e => e.TagId == 0x0202 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(10334u, length);

                    DumpImage(binaryReader, folder + "0L2A8897-1.JPG", offset, length);
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
