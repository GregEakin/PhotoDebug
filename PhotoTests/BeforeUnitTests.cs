// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		BeforeUnitTests.cs
// AUTHOR:		Greg Eakin

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace PhotoTests
{
    [TestClass]
    public class BeforeUnitTests
    {
        [TestMethod]
        public void CheckSlices()
        {
            var sizes = new[] { 2, 3, 5 };
            const int y = 3;
            const int x = 11;

            Assert.AreEqual(x, sizes[0] * sizes[1] + sizes[2]);

            var s1 = 0;
            var r1 = 0;
            var c1 = 0;

            for (var mrow = 0; mrow < y; mrow++)
            {
                for (var mcol = 0; mcol < x; mcol++)
                {
                    var index = mrow * x + mcol;
                    var slice = index / (sizes[1] * y);
                    if (slice > sizes[0])
                        slice = sizes[0];
                    var offset = index - slice * (sizes[1] * y);
                    var page = slice < sizes[0] ? 1 : 2;
                    var brow = offset / sizes[page];
                    var bcol = offset % sizes[page] + slice * sizes[1];

                    Console.WriteLine("{0}, {1},  {2}, {3},  {4}, {5}, {6}, {7}", mrow, mcol, brow, bcol, index, s1, r1, c1);

                    Assert.AreEqual(r1, brow);
                    Assert.AreEqual(s1 * sizes[1] + c1, bcol);
                    c1++;

                    if ((s1 < sizes[0] && c1 < sizes[1]) || (s1 == sizes[0] && c1 < sizes[2])) continue;
                    c1 = 0;
                    r1++;

                    if (r1 < y) continue;
                    r1 = 0;
                    s1++;
                }
            }
        }

        internal static Image ImageFromArray(byte[] array)
        {
            var width = 1472;
            var height = array.Length / width / 2;
            using (var b = new Bitmap(width, height, PixelFormat.Format16bppGrayScale))
            {
                var size = new Rectangle(0, 0, width, height);
                var bmData = b.LockBits(size, ImageLockMode.ReadWrite, PixelFormat.Format16bppGrayScale);
                var stride = bmData.Stride;
                var scan0 = bmData.Scan0;
                Marshal.Copy(array, 0, scan0, array.Length);
                b.UnlockBits(bmData);
                b.RotateFlip(RotateFlipType.Rotate90FlipX);
                return b;
            }
        }

        internal static void Bitmaps(byte[] byteArray, int offsetInBytes, short shortValue)
        {
            var width = 10;
            var height = 10;
            using (var bitmap = new Bitmap(width, height, PixelFormat.Format16bppGrayScale))
            {
                var size = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

                // Lock the unmanaged bits for efficient writing.
                var data = bitmap.LockBits(size, ImageLockMode.ReadWrite, bitmap.PixelFormat);

                // Bulk copy pixel data from a byte array:
                Marshal.Copy(byteArray, 0, data.Scan0, byteArray.Length);

                // Or, for one pixel at a time:
                Marshal.WriteInt16(data.Scan0, offsetInBytes, shortValue);

                // When finished, unlock the unmanaged bits
                bitmap.UnlockBits(data);
            }
        }

        public void FastStuff(string filename)
        {
            var data = File.ReadAllBytes(filename);
        }
    }
}
