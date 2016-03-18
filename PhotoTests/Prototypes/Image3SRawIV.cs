using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg;
using PhotoLib.Jpeg.JpegTags;
using PhotoLib.Tiff;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class Image3SRawIV
    {
        private static int cc;

        private struct DataBuf
        {
            public ushort Y;
            public short Cb;
            public short Cr;
        }

        private struct DiffBuf
        {
            public short Y1;
            public short Y2;
            public short Cb;
            public short Cr;
        }

        [TestMethod]
        public void DumpImage3SRawTest()
        {
            // 2592 x 1728, Canon EOS 7D, 1/160 sec. f/1.8 85mm, SRAW   
            // const string Folder = @"D:\Users\Greg\Pictures\2013_10_14\";
            // DumpImage3SRaw(Folder, "IMG_4194.CR2");
            const string fileName = @"D:\Users\Greg\Pictures\2016-02-26\003.CR2";
            DumpImage3SRaw(fileName);
        }

        private static void DumpImage3SRaw(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                // Image #3 is a raw image compressed in ITU-T81 lossless JPEG

                var image = rawImage.Directories.Skip(3).First();

                var offset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(0x2D42DCu, offset);

                var count = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                // Assert.AreEqual(0x1501476u, count);

                var imageFileEntry = image.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3);
                var slices = RawImage.ReadUInts16(binaryReader, imageFileEntry);
                CollectionAssert.AreEqual(new[] { (ushort)5, (ushort)864, (ushort)864 }, slices);

                binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, offset, count);

                var startOfFrame = startOfImage.StartOfFrame;
                Assert.AreEqual(1728u, startOfFrame.ScanLines);
                Assert.AreEqual(2592u, startOfFrame.SamplesPerLine);
                Assert.AreEqual(7776, startOfFrame.Width);

                Assert.AreEqual(15, startOfFrame.Precision); // sraw/sraw2

                // chrominance subsampling factors
                Assert.AreEqual(3, startOfFrame.Components.Length); // mraw/sraw1

                Assert.AreEqual(1, startOfFrame.Components[0].ComponentId);
                Assert.AreEqual(2, startOfFrame.Components[0].HFactor);
                Assert.AreEqual(1, startOfFrame.Components[0].VFactor); // SRAW
                Assert.AreEqual(0, startOfFrame.Components[0].TableId);

                Assert.AreEqual(2, startOfFrame.Components[1].ComponentId);
                Assert.AreEqual(1, startOfFrame.Components[1].HFactor);
                Assert.AreEqual(1, startOfFrame.Components[1].VFactor);
                Assert.AreEqual(0, startOfFrame.Components[1].TableId);

                Assert.AreEqual(3, startOfFrame.Components[2].ComponentId);
                Assert.AreEqual(1, startOfFrame.Components[2].HFactor);
                Assert.AreEqual(1, startOfFrame.Components[2].VFactor);
                Assert.AreEqual(0, startOfFrame.Components[2].TableId);

                Assert.AreEqual(4, startOfFrame.Components.Sum(component => component.HFactor * component.VFactor));

                var startOfScan = startOfImage.StartOfScan;
                // DumpStartOfScan(startOfScan);

                Assert.AreEqual(1, startOfScan.Bb1); // Start of spectral or predictor selection
                Assert.AreEqual(0, startOfScan.Bb2); // end of spectral selection
                Assert.AreEqual(0, startOfScan.Bb3); // successive approximation bit positions
                Assert.AreEqual(3, startOfScan.Components.Length); // sraw/sraw2

                Assert.AreEqual(1, startOfScan.Components[0].Id);
                Assert.AreEqual(0, startOfScan.Components[0].Dc);
                Assert.AreEqual(0, startOfScan.Components[0].Ac);

                Assert.AreEqual(2, startOfScan.Components[1].Id);
                Assert.AreEqual(1, startOfScan.Components[1].Dc);
                Assert.AreEqual(0, startOfScan.Components[1].Ac);

                Assert.AreEqual(3, startOfScan.Components[2].Id);
                Assert.AreEqual(1, startOfScan.Components[2].Dc);
                Assert.AreEqual(0, startOfScan.Components[2].Ac);

                startOfImage.ImageData.Reset();

                var outFile = Path.ChangeExtension(fileName, ".png");
                CreateBitmap(binaryReader, startOfImage, outFile, offset, slices);

                Assert.AreEqual(8957952, cc);
                Assert.AreEqual(3, startOfImage.ImageData.DistFromEnd);
            }
        }

        private static void CreateBitmap(BinaryReader binaryReader, StartOfImage startOfImage, string outFile, uint offset, ushort[] slices)
        {
            binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

            var startOfFrame = startOfImage.StartOfFrame;
            var height = startOfFrame.ScanLines;
            Assert.AreEqual(1728, height);              // image height

            var samplesPerLine = startOfFrame.SamplesPerLine;
            Assert.AreEqual(2592, samplesPerLine);      // image width

            var width = startOfFrame.Width;
            Assert.AreEqual(7776, startOfFrame.Width);
            Assert.AreEqual(samplesPerLine * 2, slices[0] * slices[1] + slices[2]);
            Assert.AreEqual(width, samplesPerLine * 3);
            Assert.AreEqual(3, 6 * samplesPerLine / (slices[0] * slices[1] + slices[2]));

            using (var bitmap = new Bitmap(samplesPerLine, height, PixelFormat.Format48bppRgb))
            {
                var size = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var data = bitmap.LockBits(size, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                try
                {
                    Assert.AreEqual(6 * samplesPerLine, data.Stride);  // 6 bytes * 8 bits == 48 bits per pixel

                    for (var slice = 0; slice < slices[0]; slice++) // 0..5
                        ProcessSlice(startOfImage, slice, slices[1], data);
                    ProcessSlice(startOfImage, slices[0], slices[2], data);
                }
                finally
                {
                    bitmap.UnlockBits(data);
                }

                bitmap.Save(outFile);
            }
        }

        private static readonly DataBuf[] lastCol = new DataBuf[1728];

        private static void ProcessSlice(StartOfImage startOfImage, int slice, int samples, BitmapData data)
        {
            var startOfFrame = startOfImage.StartOfFrame;
            var table0 = startOfImage.HuffmanTable.Tables[0x00];
            var table1 = startOfImage.HuffmanTable.Tables[0x01];

            var lastRow = new DataBuf { Y = 0x0000 };       // 0x4000

            for (var line = 0; line < startOfFrame.ScanLines; line++) // 0..1728
            {
                // 6 bytes * 8 bits == 48 bits per pixel
                // 3 = 6 bytes * samplesPerLine / (slices[0] * slices[1] + slices[2]);
                var scan0 = data.Scan0 + data.Stride * line + slice * samples * 3;

                // read four shorts, for two pixels
                for (var col = 0; col < samples / 4; col++)       // 0..216  ==> 0/6 .. 2592/6  --> 0 .. 432
                {
                    var diff = new DiffBuf
                    {
                        Y1 = ProcessColor(startOfImage, table0),
                        Y2 = ProcessColor(startOfImage, table0),
                        Cb = ProcessColor(startOfImage, table1),
                        Cr = ProcessColor(startOfImage, table1)
                    };

                    if (slice == 0 && col == 0)
                    {
                        var pixel0 = new DataBuf
                        {
                            Y = (ushort)(lastRow.Y + diff.Y1),
                            Cb = (short)(lastRow.Cb + diff.Cb),
                            Cr = (short)(lastRow.Cr + diff.Cr)
                        };

                        var pixel1 = new DataBuf
                        {
                            Y = (ushort)(lastRow.Y + diff.Y1 + diff.Y2),
                            Cb = (short)(lastRow.Cb + diff.Cb),
                            Cr = (short)(lastRow.Cr + diff.Cr)
                        };

                        PokePixels(scan0, col, pixel0, pixel1);

                        //lastRow.Y = pixel0.Y;
                        //lastRow.Cb = pixel0.Cb;
                        //lastRow.Cr = pixel0.Cr;

                        //lastCol[line].Y = pixel1.Y;
                        //lastCol[line].Cb = pixel1.Cb;
                        //lastCol[line].Cr = pixel1.Cr;
                    }
                    else
                    {
                        var pixel0 = new DataBuf
                        {
                            Y = (ushort)(lastCol[line].Y + diff.Y1),
                            Cb = (short)(lastCol[line].Cb + diff.Cb),
                            Cr = (short)(lastCol[line].Cr + diff.Cr)
                        };

                        var pixel1 = new DataBuf
                        {
                            Y = (ushort)(lastCol[line].Y + diff.Y1 + diff.Y2),
                            Cb = (short)(lastCol[line].Cb + diff.Cb),
                            Cr = (short)(lastCol[line].Cr + diff.Cr)
                        };

                        PokePixels(scan0, col, pixel0, pixel1);

                        //lastCol[line].Y = pixel1.Y;
                        //lastCol[line].Cb = pixel1.Cb;
                        //lastCol[line].Cr = pixel1.Cr;
                    }
                }
            }
        }

        private static void PokePixels(IntPtr scan0, int col, DataBuf pixel0, DataBuf pixel1)
        {
            {
                var red = pixel0.Y + 1.40200 * pixel0.Cr;
                var green = pixel0.Y - 0.34414 * pixel0.Cb - 0.71414 * pixel0.Cr;
                var blue = pixel0.Y + 1.77200 * pixel0.Cb;

                Marshal.WriteInt16(scan0, 12 * col + 4, (short)red);
                Marshal.WriteInt16(scan0, 12 * col + 2, (short)green);
                Marshal.WriteInt16(scan0, 12 * col + 0, (short)blue);
            }

            {
                var red = pixel1.Y + 1.40200 * pixel1.Cr;
                var green = pixel1.Y - 0.34414 * pixel1.Cb - 0.71414 * pixel1.Cr;
                var blue = pixel1.Y + 1.77200 * pixel1.Cb;

                Marshal.WriteInt16(scan0, 12 * col + 10, (short)red);
                Marshal.WriteInt16(scan0, 12 * col + 8, (short)green);
                Marshal.WriteInt16(scan0, 12 * col + 6, (short)blue);
            }
        }

        private static short ProcessColor(StartOfImage startOfImage, HuffmanTable table)
        {
            cc++;
            var hufBits = startOfImage.ImageData.GetValue(table);
            var difCode = startOfImage.ImageData.GetValue(hufBits);
            var difValue = HuffmanTable.DecodeDifBits(hufBits, difCode);
            return difValue;
        }
    }
}
