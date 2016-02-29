using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Jpeg;
using PhotoLib.Tiff;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class Image3MRaw
    {
        [TestMethod]
        public void DumpImage3MRawTest()
        {
            // 2592 x 1728, Canon EOS 7D, 1/160 sec. f/1.8 85mm, SRAW   
            // const string Folder = @"D:\Users\Greg\Pictures\2013_10_14\";
            // DumpImage3SRaw(Folder, "IMG_4194.CR2");
            const string Folder = @"D:\Users\Greg\Pictures\2016-02-28 MRaw\";
            DumpImage3SRaw(Folder, "MRaw 002.CR2");
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
                    var compression = image.Entries.Single(e => e.TagId == 0x0103 && e.TagType == 3).ValuePointer;
                    Assert.AreEqual(6u, compression);
                    var offset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(0x2D42DCu, offset);
                    var count = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                    // Assert.AreEqual(0x1501476u, count);

                    // 0xC640 UShort 16-bit: [0x000119BE] (3): 1, 2960, 2960, 
                    var imageFileEntry = image.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3);
                    var slices = imageFileEntry.ValuePointer;
                    // Assert.AreEqual(0x000119BEu, slices);
                    var number = imageFileEntry.NumberOfValue;
                    Assert.AreEqual(3u, number);
                    var sizes = RawImage.ReadUInts16(binaryReader, imageFileEntry);
                    CollectionAssert.AreEqual(new[] { (ushort)8, (ushort)1296, (ushort)1296 }, sizes);

                    binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
                    var startOfImage = new StartOfImage(binaryReader, offset, count); // { ImageData = new ImageData(binaryReader, count) };

                    var startOfFrame = startOfImage.StartOfFrame;
                    Assert.AreEqual(2592u, startOfFrame.ScanLines);
                    Assert.AreEqual(3888u, startOfFrame.SamplesPerLine);
                    Assert.AreEqual(11664, startOfFrame.Width);

                    // var rowBuf0 = new short[startOfFrame.SamplesPerLine * startOfFrame.Components.Length];
                    // var rowBuf1 = new short[startOfFrame.SamplesPerLine * startOfFrame.Components.Length];
                    // var predictor = new[] { (short)(1 << (startOfFrame.Precision - 1)), (short)(1 << (startOfFrame.Precision - 1)) };
                    Assert.AreEqual(2, startOfImage.HuffmanTable.Tables.Count);
                    var table0 = startOfImage.HuffmanTable.Tables[0x00];
                    var table1 = startOfImage.HuffmanTable.Tables[0x01];

                    // Console.WriteLine(table0.ToString());
                    // Console.WriteLine(table1.ToString());

                    Assert.AreEqual(15, startOfFrame.Precision); // sraw/sraw2

                    // chrominance subsampling factors
                    Assert.AreEqual(3, startOfFrame.Components.Length); // sraw/sraw2

                    // J:a:b = 4:2:2, h/v = 2/1
                    Assert.AreEqual(1, startOfFrame.Components[0].ComponentId);
                    Assert.AreEqual(2, startOfFrame.Components[0].HFactor);
                    Assert.AreEqual(2, startOfFrame.Components[0].VFactor);
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

                    //var memory = new Image3SRawII.DataBuf[startOfFrame.ScanLines][];          // [1728][]
                    //for (var line = 0; line < startOfFrame.ScanLines; line++)   // 0 .. 1728
                    //{
                    //    var pp = (line == 0)
                    //            ? new Image3SRawII.DataBuf { Y = 0x8000 - 3720, Cb = 0, Cr = 0 }
                    //            : new Image3SRawII.DataBuf { Y = 0x4000 - 3720, Cb = 0, Cr = 0 };
                    //    memory[line] = ProcessLine15321B(line, startOfFrame.SamplesPerLine, startOfImage, pp, table0, table1);  //2592
                    //}
                    //Assert.AreEqual(8957952, cc);
                    //Assert.AreEqual(3, startOfImage.ImageData.DistFromEnd);
                    //MakeBitmap(memory, folder, sizes);
                }
            }
        }
    }
}
