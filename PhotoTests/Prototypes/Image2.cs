using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class Image2
    {
        [TestMethod]
        public void DumpImage2Test()
        {
            const string fileName = @"D:\Users\Greg\Pictures\2016-02-21 Studio\Studio 015.CR2";
            DumpImage2(fileName);
        }

        private static void DumpImage2(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                // Image #2 is RGB, 16 bits per color, little endian.
                // Length = 3 * 16 bits * nb pixels

                var image = rawImage.Directories.Skip(2).First();

                var stripOffset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(1229532u, stripOffset);

                var imageWidth = image.Entries.Single(e => e.TagId == 0x0100 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(592u, imageWidth);

                var imageHeight = image.Entries.Single(e => e.TagId == 0x0101 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(395u, imageHeight);

                var samplesPerPixel = image.Entries.Single(e => e.TagId == 0x0115 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(3u, samplesPerPixel);

                var imageFileEntry0102 = image.Entries.Single(e => e.TagId == 0x0102 && e.TagType == 3);
                Assert.AreEqual(3u, imageFileEntry0102.NumberOfValue);
                // Assert.AreEqual(72014u, imageFileEntry0102.ValuePointer);
                var bitsPerSample = RawImage.ReadUInts16(binaryReader, imageFileEntry0102);
                CollectionAssert.AreEqual(new[] {(ushort) 16, (ushort) 16, (ushort) 16}, bitsPerSample);

                var outFile = Path.ChangeExtension(fileName, ".bmp");
                CreateBitmap(binaryReader, outFile, stripOffset, imageWidth, imageHeight);
            }
        }

        private static void CreateBitmap(BinaryReader binaryReader, string outFile, uint offset, uint width, uint height)
        {
            binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

            using (var bitmap = new Bitmap((int)width, (int)height)) // , PixelFormat.Format48bppRgb))
            {
                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                    {
                        var r = binaryReader.ReadUInt16();
                        var g = binaryReader.ReadUInt16();
                        var b = binaryReader.ReadUInt16();
                        // var color = Color.FromArgb(r, g, b);
                        var color = Color.FromArgb((byte)(r >> 5), (byte)(g >> 5), (byte)(b >> 5));
                        bitmap.SetPixel(x, y, color);
                    }

                bitmap.Save(outFile);
            }
        }
    }
}
