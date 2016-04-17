using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg.JpegTags;
using PhotoLib.Tiff;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class Image3SRawI
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
            const string folder = @"D:\Users\Greg\Pictures\2016-02-26\";
            DumpImage3SRaw(folder, "003.CR2");
        }

        private static void DumpImage3SRaw(string folder, string file)
        {
            var fileName2 = folder + file;
            using (var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // Image #3 is a raw image compressed in ITU-T81 lossless JPEG

                var image = rawImage.Directories.Skip(3).First();
                var offset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                var count = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;

                var imageFileEntry = image.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3);
                var slices = RawImage.ReadUInts16(binaryReader, imageFileEntry);
                CollectionAssert.AreEqual(new[] {(ushort) 5, (ushort) 864, (ushort) 864}, slices);

                binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, offset, count);

                var startOfFrame = startOfImage.StartOfFrame;
                Assert.AreEqual(15, startOfFrame.Precision); // sraw/sraw2
                Assert.AreEqual(1728u, startOfFrame.ScanLines); // height
                Assert.AreEqual(2592u, startOfFrame.SamplesPerLine); // width
                Assert.AreEqual(7776, startOfFrame.Width);
                Assert.AreEqual(startOfFrame.SamplesPerLine*3, startOfFrame.Width);
                Assert.AreEqual(startOfFrame.SamplesPerLine*2, slices[0]*slices[1] + slices[2]);

                startOfImage.ImageData.Reset();

                // read one line of diff 2592 * 3 == samples per line
                // write it to six lines of one slice 

                var memory = new DataBuf[startOfFrame.ScanLines, startOfFrame.SamplesPerLine]; // 1728 x 2592
                for (var slice = 0; slice < slices[0]; slice++) // 0..5
                    ProcessSlice(startOfImage, slice, slices[1], memory);
                ProcessSlice(startOfImage, slices[0], slices[2], memory);

                Assert.AreEqual(8957952, cc);
                Assert.AreEqual(3, startOfImage.ImageData.DistFromEnd);

                MakeBitmap(memory, folder);
            }
        }

        private static void ProcessSlice(StartOfImage startOfImage, int slice, int width, DataBuf[,] memory)
        {
            var startOfFrame = startOfImage.StartOfFrame;
            const int step = 6;
            for (var line = 0; line < startOfFrame.ScanLines; line += step)              // 0..1728
            {
                var vpred = new DataBuf { Y = 0x4000 };
                var diff = ReadDiffBufs(step * width, startOfImage);
                ProcessSixLines(slice, line, memory, vpred, diff);  // 864
            }
        }

        // 4:2:2 chrominance subsampling pattern
        // 2x1 chroma subsampling
        private static void ProcessSixLines(int slice, int line, DataBuf[,] memory, DataBuf prev, DiffBuf[] diff)
        {
            var last = memory.GetLength(1);
            var spred = (ushort)0;

            for (var y = 0; y < 6; y++)
            {
                var row = line + y;
                if (slice == 0)
                {
                    prev.Y = (ushort)(prev.Y + diff[0].Y1);
                    spred = prev.Y;
                    memory[row, 0].Y = prev.Y;
                    memory[row, 0].Cb = prev.Cb += diff[0].Cb;
                    memory[row, 0].Cr = prev.Cr += diff[0].Cr;

                    spred = (ushort)(spred + diff[0].Y2);
                    memory[row, 1].Y = spred;
                    memory[row, 1].Cb = 0; //(short)(memory[row, 0].Cb + diff[0].Cb);
                    memory[row, 1].Cr = 0; //(short)(memory[row, 0].Cr + diff[0].Cr);
                }
                else
                {
                    spred = (ushort)(spred + diff[0].Y1);
                    memory[row, 0].Y = spred;
                    memory[row, 0].Cb = (short)(memory[row, last - 1].Cb + diff[0].Cb);
                    memory[row, 0].Cr = (short)(memory[row, last - 1].Cr + diff[0].Cr);

                    spred = (ushort)(spred + diff[0].Y2);
                    memory[row, 1].Y = spred;
                    memory[row, 1].Cb = 0; //(short)(memory[row, 0].Cb + diff[0].Cb);
                    memory[row, 1].Cr = 0; //(short)(memory[row, 0].Cr + diff[0].Cr);
                }

                for (var x = 1; x < diff.Length / 6; x++) // 216
                {
                    var col = 2 * (slice * diff.Length / 6 + x);

                    spred = (ushort)(spred + diff[x].Y1);
                    memory[row, col].Y = spred;
                    memory[row, col].Cb = (short)(memory[row, col - 1].Cb + diff[x].Cb);
                    memory[row, col].Cr = (short)(memory[row, col - 1].Cr + diff[x].Cr);

                    spred = (ushort)(spred + diff[x].Y2);
                    memory[row, col + 1].Y = spred;
                    memory[row, col + 1].Cb = (short)(memory[row, col].Cb + diff[x].Cb);
                    memory[row, col + 1].Cr = (short)(memory[row, col].Cr + diff[x].Cr);
                }
            }
        }

        // 4:2:2 chrominance subsampling pattern
        // 2x1 chroma subsampling
        private static void ProcessLine15321Old(int slice, int line, int samplesPerLine, StartOfImage startOfImage, DataBuf[,] memory, DataBuf prev)
        {
            var diff = ReadDiffBufs(samplesPerLine, startOfImage);
            for (var x = 0; x < diff.Length; x++)        // 216
            {
                var pp = prev;

                var y1 = (ushort)(pp.Y + diff[x].Y1);
                var y2 = (ushort)(pp.Y + diff[x].Y1 + diff[x].Y2);
                var cb = (short)(pp.Cb + diff[x].Cb);
                var cr = (short)(pp.Cr + diff[x].Cr);

                pp.Y = y2;
                pp.Cb = cb;
                pp.Cr = cr;

                var col = 2 * slice * diff.Length + 2 * x;
                memory[line, col].Y = y1;
                memory[line, col].Cb = cb;
                memory[line, col].Cr = cr;

                memory[line, col + 1].Y = y2;
                memory[line, col + 1].Cb = 0;
                memory[line, col + 1].Cr = 0;
            }
        }

        private static DiffBuf[] ReadDiffBufs(int samplesPerLine, StartOfImage startOfImage)
        {
            var table0 = startOfImage.HuffmanTable.Tables[0x00];
            var table1 = startOfImage.HuffmanTable.Tables[0x01];

            var diff = new DiffBuf[samplesPerLine / 4]; // 864 / 4 == 216
            for (var x = 0; x < diff.Length; x++)
            {
                diff[x].Y1 = startOfImage.ProcessColor(0x00);
                diff[x].Y2 = startOfImage.ProcessColor(0x00);
                diff[x].Cb = startOfImage.ProcessColor(0x01);
                diff[x].Cr = startOfImage.ProcessColor(0x01);
                cc += 4;
            }

            return diff;
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
                        var red = memory[row, col].Y + 1.40200 * memory[row, col].Cr;
                        red = red / 128.0 + 0.5;
                        red = CheckValue(red, row, col);

                        var green = memory[row, col].Y - 0.34414 * memory[row, col].Cb - 0.71414 * memory[row, col].Cr;
                        green = green / 128.0 + 0.5;
                        green = CheckValue(green, row, col);

                        var blue = memory[row, col].Y + 1.77200 * memory[row, col].Cb;
                        blue = blue / 128.0 + 0.5;
                        blue = CheckValue(blue, row, col);

                        var color = Color.FromArgb((int)red, (int)green, (int)blue);
                        bitmap.SetPixel(col, row, color);
                    }

                bitmap.Save(folder + "0L2A8897-3.bmp");
            }
        }

        private static double CheckValue(double value, double row, double col)
        {
            if (value < 0.0)
            {
                // Console.WriteLine("LB: {0} at {1}, {2}", value, row, col);
                return 0.0;
            }
            if (value > 255.0)
            {
                // Console.WriteLine("UB: {0} at {1}, {2}", value, row, col);
                return 255.0;
            }

            return value;
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
