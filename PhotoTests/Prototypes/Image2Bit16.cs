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

                var image = rawImage.Directories.Skip(2).First();
                Assert.AreEqual(13, image.Entries.Length);

                var imageWidth = image.Entries.Single(e => e.TagId == 0x0100 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(592u, imageWidth);

                var imageHeight = image.Entries.Single(e => e.TagId == 0x0101 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(395u, imageHeight);

                var imageFileEntry0102 = image.Entries.Single(e => e.TagId == 0x0102 && e.TagType == 3);
                // Assert.AreEqual(72014u, imageFileEntry0102.ValuePointer);
                // Assert.AreEqual(3u, imageFileEntry0102.NumberOfValue);
                var bitsPerSample = RawImage.ReadUInts16(binaryReader, imageFileEntry0102);
                CollectionAssert.AreEqual(new[] { (ushort)16, (ushort)16, (ushort)16 }, bitsPerSample);

                var compression = image.Entries.Single(e => e.TagId == 0x0103 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(1u, compression);               // 1 == uncompressed

                var photometricInterpretation =
                    image.Entries.Single(e => e.TagId == 0x0106 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(2u, photometricInterpretation); // 2 == RGB

                var stripOffset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(1229532u, stripOffset);

                var samplesPerPixel = image.Entries.Single(e => e.TagId == 0x0115 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(3u, samplesPerPixel);

                var rowsPerStrip = image.Entries.Single(e => e.TagId == 0x0116 && e.TagType == 3).ValuePointer;
                Assert.AreEqual(395u, rowsPerStrip);

                var stripByteCounts = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(1403040u, stripByteCounts);
                Assert.AreEqual(stripByteCounts, imageWidth * imageHeight * samplesPerPixel * 2);

                var planarConfiguration = image.Entries.Single(e => e.TagId == 0x011C && e.TagType == 3).ValuePointer;
                Assert.AreEqual(1u, planarConfiguration);       // 1 == chunky

                // unknown
                var table1 = image.Entries.Single(e => e.TagId == 0xC5D9 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(2u, table1);

                var table2 = image.Entries.Single(e => e.TagId == 0xC6C5 && e.TagType == 4).ValuePointer;
                Assert.AreEqual(3u, table2);

                var imageFileEntryC6DC = image.Entries.Single(e => e.TagId == 0xC6DC && e.TagType == 4);
                // Assert.AreEqual(72020u, imageFileEntry011C.ValuePointer);
                // Assert.AreEqual(4u, imageFileEntryC6DC.NumberOfValue);
                var stuff = RawImage.ReadUInts(binaryReader, imageFileEntryC6DC);
                CollectionAssert.AreEqual(new[] { 577u, 386u, 14u, 9u }, stuff);
                Assert.AreEqual(imageWidth, stuff[0] + stuff[2] + 1);
                Assert.AreEqual(imageHeight, stuff[1] + stuff[3]);

                var outFile = Path.ChangeExtension(fileName, ".png");
                CreateBitmap(binaryReader, outFile, stripOffset, imageWidth, imageHeight);
            }
        }

        private static void CreateBitmap(BinaryReader binaryReader, string outFile, uint offset, uint width, uint height)
        {
            binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

            using (var bitmap = new Bitmap((int)width, (int)height, PixelFormat.Format48bppRgb))
            {
                var size = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var data = bitmap.LockBits(size, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                try
                {
                    Assert.AreEqual((int)(6 * width), data.Stride);

                    for (var y = 0; y < height; y++)
                    {
                        var scan0 = data.Scan0 + data.Stride * y;
                        for (var x = 0; x < width; x++)
                        {
                            var r = CheckValue(binaryReader.ReadUInt16());
                            Marshal.WriteInt16(scan0, 6 * x + 4, (short)r);

                            var g = CheckValue(binaryReader.ReadUInt16());
                            Marshal.WriteInt16(scan0, 6 * x + 2, (short)g);

                            var b = CheckValue(binaryReader.ReadUInt16());
                            Marshal.WriteInt16(scan0, 6 * x + 0, (short)b);
                        }
                    }
                }
                finally
                {
                    bitmap.UnlockBits(data);
                }

                bitmap.Save(outFile);
            }

            Console.WriteLine("Min = 0x{0:X4}, Max = 0x{1:X4}", min, max);
        }

        private static ushort min = ushort.MaxValue;
        private static ushort max = ushort.MinValue;

        private static ushort CheckValue(ushort p0)
        {
            if (min > p0) min = p0;
            if (max < p0) max = p0;

            // Min = 0x07C4, Max = 0x38F6
            var d0 = ((double)p0 - 0x07C4) / (0x38F6 - 0x07c4) * 0xFFFF;

            return (ushort)d0;
        }
    }
}
