// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		Image3SRawIII.cs
// AUTHOR:		Greg Eakin

using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg.JpegTags;
using PhotoLib.Tiff;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class Image3SRawIII
    {
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

                    startOfImage.ImageData.Reset();

                    canon_sraw_load_raw(startOfImage, slices);

                    //Assert.AreEqual(3, startOfImage.ImageData.DistFromEnd);

                    // MakeBitmap(memory, folder);
                }
            }
        }

        private static void canon_sraw_load_raw(StartOfImage startOfImage, ushort[] slices)
        {
            var startOfFrame = startOfImage.StartOfFrame;
            var sraw = startOfFrame.Components[0].HFactor * startOfFrame.Components[0].VFactor - 1;   // 1
            Assert.AreEqual(1, sraw);
            var clrs = startOfFrame.Components.Length + sraw;                                         // 4
            Assert.AreEqual(4, clrs);

            var height = startOfFrame.ScanLines;
            Assert.AreEqual(1728, height);
            var jrow = 0;

            var width = slices[1];                          // image widht
            Assert.AreEqual(864, width);
            var rawWidth = startOfFrame.SamplesPerLine;     // 0x1031
            var jhwide = startOfFrame.SamplesPerLine;       // 0xffc0, data[3] << 8 | data[4]
            Assert.AreEqual(2592u, startOfFrame.SamplesPerLine);

            var jwide = (jhwide >>= 1) * clrs;
            var ecol = 0;
            var jcol = 0;

            var rp = new int[0];
            for (var slice = 0; slice <= slices[0]; slice++)
            {
                var scol = ecol;
                ecol += slices[1] * 2 / clrs;
                if (slices[0] == 0 || ecol > rawWidth - 1)
                    ecol = rawWidth & -2;
                for (var row = 0; row < height; row += (clrs >> 1) - 1)
                {
                    var ip = new short[width, 4];  // image + row * width
                    for (var col = scol; col < ecol; col += 2, jcol += clrs)
                    {
                        if ((jcol %= jwide) == 0)
                            rp = new_ljpeg_row(jrow++, startOfImage);  // rp.length = 10368
                        if (col >= width)
                            continue;
                        for (var c = 0; c < clrs - 2; c++)
                            ip[col + (c >> 1) * width + (c & 1), 0] = (short)rp[jcol + c];
                        ip[col, 1] = (short)(rp[jcol + clrs - 2] - 0x4000);
                        ip[col, 2] = (short)(rp[jcol + clrs - 1] - 0x4000);
                    }
                }
            }
        }

        private static int[] new_ljpeg_row(int jrow, StartOfImage startOfImage)
        {
            var startOfFrame = startOfImage.StartOfFrame;

            var bits = startOfFrame.Precision;     // 15
            var vpred = new int[6];
            for (var c = 0; c < 6; c++)
                vpred[c] = 1 << (bits - 1);

            var memory = new int[4 * startOfFrame.SamplesPerLine];

            var spred = 0;
            for (var col = 0; col < startOfFrame.SamplesPerLine; col++)
                for (var c = 0; c < 4; c++)
                {
                    var diff = startOfImage.ProcessColor((byte)(c / 2));
                    int pred;
                    if (c <= 1 && (col > 0 || c > 0))
                        pred = spred;
                    else if (col > 0)
                        pred = memory[4 * col + c - 4];
                    else
                        pred = (vpred[c] += diff) - diff;

                    memory[4 * col + c] = pred + diff;
                    if (c <= 1)
                        spred = memory[4 * col + c];
                }

            return memory;
        }
    }
}
