// Project Photo Library 0.1
// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoDebug
// FILE:		DumpImageSlices.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using PhotoLib.Jpeg.JpegTags;
using PhotoLib.Tiff;

namespace PhotoLib
{
    public class DumpImageSlices
    {
        private struct DataBuf
        {
            public ushort R;
            public ushort G;
            public ushort B;
        }

        private struct DiffBuf
        {
            public short Y1;
            public short Y2;
        }

        public static void DumpImage3Raw(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);

                // Image #3 is a raw image compressed in ITU-T81 lossless JPEG
                var image = rawImage.Directories.Skip(3).First();

                var offset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                var count = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                var slices = RawImage.ReadUInts16(binaryReader, image.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3));

                binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, offset, count);
                startOfImage.ImageData.Reset();

                var outFile = Path.ChangeExtension(fileName, ".png");
                CreateBitmap(binaryReader, startOfImage, outFile, offset, slices);
            }
        }

        private static void CreateBitmap(BinaryReader binaryReader, StartOfImage startOfImage, string outFile, uint offset, ushort[] slices)
        {
            //binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

            //var startOfFrame = startOfImage.StartOfFrame;
            //var height = startOfFrame.ScanLines;
            //var width = startOfFrame.Width;
            //using (var bitmap = new Bitmap(width, height, PixelFormat.Format48bppRgb))
            //{
            //    var size = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            //    var data = bitmap.LockBits(size, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            //    try
            //    {
            //        var pp = new[] { (ushort)0x2000, (ushort)0x2000 };

            //        for (var slice = 0; slice < slices[0]; slice++) // 0..5
            //            ProcessSlice(startOfImage, slice, slices[1], data, pp);
            //        ProcessSlice(startOfImage, slices[0], slices[2], data, pp);
            //    }
            //    finally
            //    {
            //        bitmap.UnlockBits(data);
            //    }

            //    bitmap.Save(outFile);
            //}
        }

        //private static void ProcessSlice(StartOfImage startOfImage, int slice, int width, BitmapData data, ushort[] pp)
        //{
        //    var startOfFrame = startOfImage.StartOfFrame;
        //    var scanLines = startOfFrame.ScanLines;
        //    var memory = new ushort[2];

        //    for (var line = 0; line < scanLines; line++)
        //    {
        //        // 6 bytes * 8 bits == 48 bits per pixel
        //        // 3 = 6 bytes * samplesPerLine / (slices[0] * slices[1] + slices[2]);
        //        var scan0 = data.Scan0 + data.Stride * line + slice * width * 6;

        //        // read two shorts, for two pixels
        //        for (var col = 0; col < width / 2; col++)
        //        {
        //            var diff = new DiffBuf
        //            {
        //                Y1 = startOfImage.ProcessColor(0x00),
        //                Y2 = startOfImage.ProcessColor(0x01),
        //            };

        //            if (line % 2 == 0 && col == 0)
        //            {
        //                pp[0] = (ushort)(pp[0] + diff.Y1);
        //                memory[0] = pp[0];

        //                pp[1] = (ushort)(pp[1] + diff.Y2);
        //                memory[1] = pp[1];
        //            }
        //            else
        //            {
        //                memory[0] = (ushort)(memory[0] + diff.Y1);

        //                memory[1] = (ushort)(memory[1] + diff.Y2);
        //            }

        //            if (line % 2 == 0)
        //            {
        //                var pixel0 = new DataBuf
        //                {
        //                    R = memory[0]
        //                };

        //                var pixel1 = new DataBuf
        //                {
        //                    G = memory[1]
        //                };

        //                PokePixels(scan0, col, pixel0, pixel1);
        //            }
        //            else
        //            {
        //                var pixel0 = new DataBuf
        //                {
        //                    G = memory[0]
        //                };

        //                var pixel1 = new DataBuf
        //                {
        //                    B = memory[1]
        //                };

        //                PokePixels(scan0, col, pixel0, pixel1);
        //            }
        //        }
        //    }
        //}

        private static void PokePixels(IntPtr scan0, int col, DataBuf pixel0, DataBuf pixel1)
        {
            {
                var red = pixel0.R;
                var green = pixel0.G;
                var blue = pixel0.B;

                Marshal.WriteInt16(scan0, 12 * col + 4, (short)red);
                Marshal.WriteInt16(scan0, 12 * col + 2, (short)green);
                Marshal.WriteInt16(scan0, 12 * col + 0, (short)blue);
            }

            {
                var red = pixel1.R;
                var green = pixel1.G;
                var blue = pixel1.B;

                Marshal.WriteInt16(scan0, 12 * col + 10, (short)red);
                Marshal.WriteInt16(scan0, 12 * col + 8, (short)green);
                Marshal.WriteInt16(scan0, 12 * col + 6, (short)blue);
            }
        }
    }
}