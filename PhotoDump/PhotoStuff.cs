using PhotoLib.Jpeg.NonStartOfFrame;
using PhotoLib.Tiff;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace PhotoDump
{
    public class PhotoStuff
    {
        public PhotoStuff(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.Last();

                // var compression = image.Entries.Single(e => e.TagId == 0x0103 && e.TagType == 3).ValuePointer;

                var offset = image.Entries.Single(e => e.TagId == 0x0111 && e.TagType == 4).ValuePointer;
                var count = image.Entries.Single(e => e.TagId == 0x0117 && e.TagType == 4).ValuePointer;
                var imageFileEntry = image.Entries.Single(e => e.TagId == 0xC640 && e.TagType == 3);
                var slices = RawImage.ReadUInts16(binaryReader, imageFileEntry);

                binaryReader.BaseStream.Seek(offset, SeekOrigin.Begin);
                var startOfImage = new StartOfImageRgb(binaryReader, offset, count);
                startOfImage.ImageData.Reset();
                var memory = startOfImage.ReadImage();

                var outFile = Path.ChangeExtension(fileName, ".bmp");
                MakeBitmap(memory, outFile, slices);
            }
        }

        private static void MakeBitmap(ushort[][] memory, string outFile, ushort[] slices)
        {
            var y = memory.GetLength(0);
            var x = memory[0].GetLength(0);

            using (var bitmap = new Bitmap(x, y))
            {
                for (var mrow = 0; mrow < y; mrow++)
                {
                    var rdata = memory[mrow];
                    for (var mcol = 0; mcol < x; mcol++)
                    {
                        var index = mrow * x + mcol;
                        var slice = index / (slices[1] * y);
                        if (slice > slices[0])
                            slice = slices[0];
                        var offset = index - slice * slices[1] * y;
                        var page = slice < slices[0] ? 1 : 2;
                        var brow = offset / slices[page];
                        var bcol = offset % slices[page] + slice * slices[1];

                        var val = rdata[mcol];
                        PixelSet(bitmap, brow, bcol, val);
                    }
                }

                bitmap.Save(outFile);
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
