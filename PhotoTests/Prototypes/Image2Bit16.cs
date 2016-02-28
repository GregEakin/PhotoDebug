using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class Image2Bit16
    {
        [TestMethod]
        public void DumpImage2Test()
        {
            const string Folder = @"D:\Users\Greg\Pictures\2016-02-21 Studio\";
            DumpImage2(Folder, "Studio 015.CR2");
        }

        private static void DumpImage2(string folder, string file)
        {
            var fileName2 = folder + file;
            using (var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                // Image #2 is RGB, 16 bits per color, little endian.
                // Length = 3 * 16 bits * nb pixels
                {
                    var image = rawImage.Directories.Skip(2).First();
                    var width = image.Entries.Single(e => e.TagId == 0x0100 && e.TagType == 3).ValuePointer;
                    Assert.AreEqual(592u, width);
                    var height = image.Entries.Single(e => e.TagId == 0x0101 && e.TagType == 3).ValuePointer;
                    Assert.AreEqual(395u, height);
                    var offset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(1229532u, offset);
                    var samples = image.Entries.Single(e => e.TagId == 0x0115 && e.TagType == 3).ValuePointer;
                    Assert.AreEqual(3u, samples);
                    var rows = image.Entries.Single(e => e.TagId == 0x0116 && e.TagType == 3).ValuePointer;
                    Assert.AreEqual(395u, rows);
                    var count = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                    Assert.AreEqual(1403040u, count);

                    Assert.AreEqual(count, width * height * samples * 2);

                    DumpImage(binaryReader, folder + file + ".RGB", offset, width, height);
                }
            }
        }

        private static void DumpImage(BinaryReader binaryReader, string folder, uint offset, uint width, uint height)
        {
            binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

            using (var bitmap = new Bitmap((int)width, (int)height, PixelFormat.Format48bppRgb))
            {
                var size = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var data = bitmap.LockBits(size, ImageLockMode.ReadWrite, bitmap.PixelFormat);

                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                    {
                        var scan0 = data.Scan0 + data.Stride * y + x;

                        var c = binaryReader.ReadUInt16();
                        PixelSet(scan0, x, y, (short)c);
                    }

                bitmap.UnlockBits(data);
                bitmap.Save(folder + "0L2A8897-2.bmp");
            }
        }

        private static void PixelSet(IntPtr scan0, int row, int col, short value)
        {
            if (row % 2 == 0 && col % 2 == 0)
            {
                Marshal.WriteInt16(scan0, 3 * col + 0, 0);
                Marshal.WriteInt16(scan0, 3 * col + 1, 0);
                Marshal.WriteInt16(scan0, 3 * col + 2, value);
            }
            else if ((row % 2 == 1 && col % 2 == 0) || (row % 2 == 0 && col % 2 == 1))
            {
                Marshal.WriteInt16(scan0, 3 * col + 0, 0);
                Marshal.WriteInt16(scan0, 3 * col + 1, value);
                Marshal.WriteInt16(scan0, 3 * col + 2, 0);
            }
            else if (row % 2 == 1 && col % 2 == 1)
            {
                Marshal.WriteInt16(scan0, 3 * col + 0, value);
                Marshal.WriteInt16(scan0, 3 * col + 1, 0);
                Marshal.WriteInt16(scan0, 3 * col + 2, 0);
            }
        }
    }
}
