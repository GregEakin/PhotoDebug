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
            var sizes = new[] { 2, 5, 7 };
            const int y = 6;
            const int x = 17;

            Assert.AreEqual(x, sizes[0] * sizes[1] + sizes[2]);

            for (var mrow = 0; mrow < y; mrow++)
            {
                for (var mcol = 0; mcol < x; mcol++)
                {
                    var index = mrow * x + mcol;
                    var slice = index / (sizes[1] * y);
                    if (slice >= sizes[0])
                        slice = sizes[0];
                    index -= slice * (sizes[1] * y);
                    var brow = index / sizes[slice < sizes[0] ? 1 : 2];
                    var bcol = index % sizes[slice < sizes[0] ? 1 : 2] + slice * sizes[1];
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
            byte[] data;
            data = File.ReadAllBytes(filename);
        }
    }
}
