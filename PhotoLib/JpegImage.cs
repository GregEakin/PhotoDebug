namespace PhotoLib
{
    using System.IO;
    using System.Linq;

    using PhotoLib.Jpeg;
    using PhotoLib.Tiff;

    public class JpegImage
    {
        public static void TestImage()
        {
            // const string FileName = @"C:\Users\Greg\Pictures\IMG_0511.CR2";
            const string FileName = @"C:\Users\Greg\Pictures\IMG_0516.CR2";
            // const string TestFile = @"C:\Users\Greg\Pictures\Oops.jpg";
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.First(e => e.TagId == 0x0111).ValuePointer;     // TIF_STRIP_OFFSETS For each strip, the byte offset of that strip.
                var length = directory.Entries.First(e => e.TagId == 0x0117).ValuePointer;      // TIF_STRIP_BYTE_COUNTS For each strip, the number of bytes in the strip after compression.

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);

                var startOfImage = new StartOfImage(binaryReader, address, length);

                // GetBits(rawData);
                // GetLosslessJpgRow(null, rawData, TB0, TL0, TB1, TL1, Prop);

                // for (var iRow = 0; iRow < height; iRow++)
                {
                    // var rowBuf = new ushort[width];
                    // GetLosslessJpgRow(rowBuf, rawData, TL0, TB0, TL1, TB1, Prop);
                    // PutUnscrambleRowSlice(rowBuf, imageData, iRow, Prop);
                }

                //using (var str = new MemoryStream(rawData))
                //{
                //    var image = Image.FromStream(str, false, false);
                //    image.Save("asdf.jpg");
                //}
            }
        }
    }
}