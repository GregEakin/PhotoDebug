// Project Console Application 0.1
// Copyright © 2013-2013. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoDebug
// FILE:		DualReaderTests.cs
// AUTHOR:		Greg Eakin
namespace PhotoTests
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using PhotoLib.Jpeg;
    using PhotoLib.Tiff;

    public class DualReaderTests
    {
        [TestMethod]
        public void TestMethodC5M3()
        {
            const string Folder = @"C:\Users\Greg\Pictures\2013-10-06 001\";
            const string FileName2 = Folder + "0L2A8892.CR2";
            const string Bitmap = Folder + "0L2A8892 C.BMP";

            DumpBitmap(FileName2, Bitmap);
        }

        private static void DumpBitmap(string fileName2, string bitmap)
        {
            using (var fileStream = File.Open(fileName2, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Last();

                var strips = imageFileDirectory.Entries.First(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE
                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x = binaryReader.ReadUInt16();
                var y = binaryReader.ReadUInt16();
                var z = binaryReader.ReadUInt16();
                Console.WriteLine("x {0}, y {1}, z {2}", x, y, z);
                Assert.AreEqual(1, x);
                Assert.AreEqual(2960, y);
                Assert.AreEqual(2960, z);

                var address = imageFileDirectory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length) { ImageData = new ImageData(binaryReader, length) };
                var lossless = startOfImage.Lossless;
                Console.WriteLine("lines {0}, samples per line {1} * {2} = {3}", lossless.ScanLines, lossless.SamplesPerLine, lossless.Components.Length, lossless.Width);
                Assert.AreEqual(x * y + z, lossless.Width); // Sensor width (bits)
                Assert.AreEqual(x * y + z, lossless.SamplesPerLine * lossless.Components.Length);

                var rowBuf = new short[4, lossless.SamplesPerLine];

                var predictor = new[] { (short)(1 << (lossless.Precision - 1)), (short)(1 << (lossless.Precision - 1)) };
                var table0 = startOfImage.HuffmanTable.Tables[0x00];
                // var table1 = startOfImage.HuffmanTable.Tables[0x01];

                using (var image1 = new Bitmap(500, 500))
                {
                    for (var k = 0; k < x; k++)
                    {
                        ParseRow(lossless, k, y, startOfImage, table0, rowBuf, predictor, image1);
                    }

                    ParseRow(lossless, x, z, startOfImage, table0, rowBuf, predictor, image1);

                    image1.Save(bitmap);

                    Console.WriteLine("EOF {0}", startOfImage.ImageData.RawData.Length - startOfImage.ImageData.Index);
                }
            }
        }

        private static void ParseRow(
            StartOfFrame lossless, int x, ushort y, StartOfImage startOfImage, HuffmanTable table0, short[,] rowBuf, short[] predictor, Bitmap image1)
        {
            var i1 = 4 / lossless.Components.Length;

            for (var j = 0; j < lossless.ScanLines / i1; j++)
            {
                for (var g = 0; g < i1; g++)
                {
                    for (var i = 0; i < y / lossless.Components.Length; i++)
                    {
                        for (var h = 0; h < lossless.Components.Length; h++)
                        {
                            var hufCode = UnitTests.GetValue(startOfImage.ImageData, table0);
                            var difCode = startOfImage.ImageData.GetSetOfBits(hufCode);
                            var dif = UnitTests.DecodeDifBits(hufCode, difCode);

                            if (i == 0)
                            {
                                rowBuf[g * i1 + h, i] = predictor[h] += dif;
                            }
                            else
                            {
                                rowBuf[g * i1 + h, i] = (short)(rowBuf[g * i1 + h, i - 1] + dif);
                            }
                        }
                    }
                }

                UnitTests.DumpPixel(x * y, j, y, rowBuf, image1);
            }

        }
    }
}