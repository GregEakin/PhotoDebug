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

        // ...F ...F ...0 ...0 ...E ...0 ...5 ...4 ...1 ...F ...F ...3 ...5 ...F ...A ...6 ...F ...4 ...F ...1 ...4 ...E ...D ...2 ...5 ...1 ...0 ...E ...2 ...9 ...D ...B ...F ...1 ...E ...A ...E ...C ...C ...7
        // 1111 1111 0000 0000 1110 0000 0101 0100 0001 1111 1111 0011 0101 1111 1010 0110 1111 0100 1111 0001 0100 1110 1101 0010 0101 0001 0000 1110 0010 1001 1101 1011 1111 0001 1110 1010 1110 1100 1100 0111
        // 

        static DataBuf[] Prev;
        static double minY = double.MaxValue; static double maxY = double.MinValue;
        static double minCb = double.MaxValue; static double maxCb = double.MinValue;
        static double minCr = double.MaxValue; static double maxCr = double.MinValue;

        static int cc = 0;





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

        private static void DumpImage(BinaryReader binaryReader, string folder, uint offset, uint width, uint height)
        {
            using (var image1 = new Bitmap((int)width, (int)height)) // , PixelFormat.Format48bppRgb))
            {
                binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                    {
                        var r = binaryReader.ReadUInt16();
                        var g = binaryReader.ReadUInt16();
                        var b = binaryReader.ReadUInt16();
                        // var color = Color.FromArgb(r, g, b);
                        var color = Color.FromArgb((byte)(r >> 5), (byte)(g >> 5), (byte)(b >> 5));
                        image1.SetPixel(x, y, color);
                    }

                image1.Save(folder + "0L2A8897-2.bmp");
            }
        }

        private static void DumpImage(BinaryReader binaryReader, string filename, uint offset, uint length)
        {
            using (var fout = File.Open(filename, FileMode.Create, FileAccess.Write))
            {
                binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);

                //Create a byte array to act as a buffer
                var buffer = new byte[32];
                for (var i = 0; i < length;)
                {
                    //Read from the source file
                    //The Read method returns the number of bytes read
                    int n = binaryReader.Read(buffer, 0, buffer.Length);

                    //Write the contents of the buffer to the destination file
                    fout.Write(buffer, 0, n);

                    i += n;
                }

                //Flush the contents of the buffer to the file
                fout.Flush();
            }
        }

        public void FastStuff(string filename)
        {
            byte[] data;
            data = File.ReadAllBytes(filename);
        }
    }
}
