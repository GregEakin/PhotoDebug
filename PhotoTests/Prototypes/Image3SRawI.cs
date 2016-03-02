using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg;
using PhotoLib.Jpeg.JpegTags;
using PhotoLib.Tiff;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class Image3SRawI
    {
        //static double minY = double.MaxValue; static double maxY = double.MinValue;
        //static double minCb = double.MaxValue; static double maxCb = double.MinValue;
        //static double minCr = double.MaxValue; static double maxCr = double.MinValue;
        static int cc = 0;

        struct DataBuf
        {
            public ushort Y;
            public short Cb;
            public short Cr;
        }

        struct DiffBuf
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
            const string folder = @"D:\Users\Greg\Pictures\2016-02-26\";
            DumpImage3SRaw(folder, "007.CR2");
        }

        private static void DumpImage3SRaw(string folder, string file)
        {
            var fileName2 = folder + file;
            using (var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                // Image #3 is a raw image compressed in ITU-T81 lossless JPEG
                {
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
                    Assert.AreEqual(3, startOfFrame.Components.Length); // sraw/sraw2

                    // J:a:b = 4:2:2, h/v = 2/1
                    Assert.AreEqual(1, startOfFrame.Components[0].ComponentId);
                    Assert.AreEqual(2, startOfFrame.Components[0].HFactor);
                    Assert.AreEqual(1, startOfFrame.Components[0].VFactor);
                    Assert.AreEqual(0, startOfFrame.Components[0].TableId);

                    Assert.AreEqual(2, startOfFrame.Components[1].ComponentId);
                    Assert.AreEqual(1, startOfFrame.Components[1].HFactor);
                    Assert.AreEqual(1, startOfFrame.Components[1].VFactor);
                    Assert.AreEqual(0, startOfFrame.Components[1].TableId);

                    Assert.AreEqual(3, startOfFrame.Components[2].ComponentId);
                    Assert.AreEqual(1, startOfFrame.Components[2].HFactor);
                    Assert.AreEqual(1, startOfFrame.Components[2].VFactor);
                    Assert.AreEqual(0, startOfFrame.Components[2].TableId);

                    // sraw/sraw2
                    // Y1 Y2 Cb Cr ...
                    // Y1 Cb Cr Y2 x x
                    // Y1 Cb Cr Y2 x x

                    var startOfScan = startOfImage.StartOfScan;
                    // DumpStartOfScan(startOfScan);

                    Assert.AreEqual(1, startOfScan.Bb1);    // Start of spectral or predictor selection
                    Assert.AreEqual(0, startOfScan.Bb2);    // end of spectral selection
                    Assert.AreEqual(0, startOfScan.Bb3);    // successive approximation bit positions
                    Assert.AreEqual(3, startOfScan.Components.Length);   // sraw/sraw2

                    Assert.AreEqual(1, startOfScan.Components[0].Id);
                    Assert.AreEqual(0, startOfScan.Components[0].Dc);
                    Assert.AreEqual(0, startOfScan.Components[0].Ac);

                    Assert.AreEqual(2, startOfScan.Components[1].Id);
                    Assert.AreEqual(1, startOfScan.Components[1].Dc);
                    Assert.AreEqual(0, startOfScan.Components[1].Ac);

                    Assert.AreEqual(3, startOfScan.Components[2].Id);
                    Assert.AreEqual(1, startOfScan.Components[2].Dc);
                    Assert.AreEqual(0, startOfScan.Components[2].Ac);

                    // DumpCompressedData(startOfImage);

                    // horz sampling == 1
                    startOfImage.ImageData.Reset();

                    var prev = new DataBuf[startOfFrame.ScanLines / 6];
                    for (var line = 0; line < prev.Length; line++)
                        prev[line] = line == 0
                            ? new DataBuf { Y = 0x8000 - 3720, Cb = 0, Cr = 0 }
                            : new DataBuf { Y = 0x4000 - 3720, Cb = 0, Cr = 0 };

                    var memory = new DataBuf[startOfFrame.ScanLines, startOfFrame.Width / 3];   // 1728 x 7776 / 3 (= 2592)
                    for (var slice = 0; slice < slices[0]; slice++)                              // 0..5
                    {
                        for (var line = 0; line < startOfFrame.ScanLines; line++)                 // 0..1728
                            ProcessLine15321(slice, line, slices[1], startOfImage, memory, prev);  // 864
                    }
                    {
                        for (var line = 0; line < startOfFrame.ScanLines; line++)                 // 0..1728
                            ProcessLine15321(slices[0], line, slices[2], startOfImage, memory, prev);  // 864   
                    }

                    Assert.AreEqual(8957952, cc);
                    Assert.AreEqual(3, startOfImage.ImageData.DistFromEnd);
                    
                    //for (var slice = 0; slice < 2; slice++)                              // 0..5
                    //    for (var line = 0; line < startOfFrame.ScanLines; line++)
                    //        ProcessLine15321(slice, line, slices[1], startOfImage, memory, prev);  // 864

                    MakeBitmap(memory, folder);
                }
            }
        }

        private static void ProcessLine15321(int slice, int line, int samplesPerLine, StartOfImage startOfImage, DataBuf[,] memory, DataBuf[] prev)
        {
            var diff = ReadDiffBufs(samplesPerLine, startOfImage);

            //if (500 < line || line > 1000) return;
            //if (slice == 1 && line % 2 == 0) return;

            for (var x = 0; x < diff.Length; x++)        // 216
            {
                var y1 = (ushort)(prev[line / 6].Y + diff[x].Y1);
                var y2 = (ushort)(prev[line / 6].Y + diff[x].Y1 + diff[x].Y2);
                var cb = (short)(prev[line / 6].Cb + diff[x].Cb);
                var cr = (short)(prev[line / 6].Cr + diff[x].Cr);

                prev[line / 6].Y = y2;
                prev[line / 6].Cb = cb;
                prev[line / 6].Cr = cr;

                var col = 2 * slice * diff.Length + 2 * x;
                memory[line, col].Y = y1;
                memory[line, col].Cb = cb;
                memory[line, col].Cr = cr;

                memory[line, col + 1].Y = y2;
                memory[line, col + 1].Cb = cb;
                memory[line, col + 1].Cr = cr;

                // debug: check the bounds of the running sum
                //if (minY > prev[line].Y) minY = prev[line].Y;
                //if (maxY < prev[line].Y) maxY = prev[line].Y;

                //if (minCb > prev[line].Cb) minCb = prev[line].Cb;
                //if (maxCb < prev[line].Cb) maxCb = prev[line].Cb;

                //if (minCr > prev[line].Cr) minCr = prev[line].Cr;
                //if (maxCr < prev[line].Cr) maxCr = prev[line].Cr;
            }
        }

        private static DiffBuf[] ReadDiffBufs(int samplesPerLine, StartOfImage startOfImage)
        {
            var table0 = startOfImage.HuffmanTable.Tables[0x00];
            var table1 = startOfImage.HuffmanTable.Tables[0x01];

            var diff = new DiffBuf[samplesPerLine / 4]; // 864 / 4 == 216
            for (var x = 0; x < diff.Length; x++)
            {
                diff[x].Y1 = ProcessColor(startOfImage, table0);
                diff[x].Y2 = ProcessColor(startOfImage, table0);
                diff[x].Cb = ProcessColor(startOfImage, table1);
                diff[x].Cr = ProcessColor(startOfImage, table1);
                cc += 4;
            }

            return diff;
        }

        private static short ProcessColor(StartOfImage startOfImage, HuffmanTable table)
        {
            var hufBits = startOfImage.ImageData.GetValue(table);
            var difCode = startOfImage.ImageData.GetValue(hufBits);
            var difValue = HuffmanTable.DecodeDifBits(hufBits, difCode);
            return difValue;
        }

        private static void MakeBitmap(DataBuf[,] memory, string folder)
        {
            var y = memory.GetLength(0);
            var x = memory.GetLength(1);
            using (var bitmap = new Bitmap(x, y))
            {
                for (var row = 0; row < y; row++)
                    for (var col = 0; col < x; col++)
                    {
                        //var r = memory[row, col].Y + 1.40200 * memory[row, col].Cr;
                        //var g = memory[row, col].Y - 0.34414 * memory[row, col].Cb - 0.71414 * memory[col, row].Cr;
                        //var b = memory[row, col].Y + 1.77200 * memory[row, col].Cb;
                        var c = (memory[row, col].Y) >> 7;
                        var color = Color.FromArgb((byte)c, (byte)c, (byte)c);
                        bitmap.SetPixel(col, row, color);
                    }

                bitmap.Save(folder + "0L2A8897-3.bmp");
            }
        }

        private static void PixelSet(Bitmap bitmap, int row, int col, ushort val)
        {
            if (row % 2 == 0 && col % 2 == 0)
            {
                var r = (byte)Math.Min((val >> 4), 255);
                var color = Color.FromArgb(r, 0, 0);
                bitmap.SetPixel(col, row, color);
            }
            else if ((row % 2 == 1 && col % 2 == 0) || (row % 2 == 0 && col % 2 == 1))
            {
                var g = (byte)Math.Min((val >> 5), 255);
                var color = Color.FromArgb(0, g, 0);
                bitmap.SetPixel(col, row, color);
            }
            else if (row % 2 == 1 && col % 2 == 1)
            {
                var b = (byte)Math.Min((val >> 4), 255);
                var color = Color.FromArgb(0, 0, b);
                bitmap.SetPixel(col, row, color);
            }
        }
    }
}
