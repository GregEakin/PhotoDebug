namespace PhotoLib
{
    using System.Drawing;
    using System.IO;
    using System.Linq;

    public class JpegImage
    {
        public static void TestImage()
        {
            const string FileName = @"C:\Users\Greg\Pictures\IMG_0511.CR2";
            // const string TestFile = @"C:\Users\Greg\Pictures\Oops.jpg";
            using (var fileStream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);

                var directory = rawImage.Directories.Last();
                var address = directory.Entries.First(e => e.TagId == 0x0111).ValuePointer;     // TIF_STRIP_OFFSETS For each strip, the byte offset of that strip.
                var length = directory.Entries.First(e => e.TagId == 0x0117).ValuePointer;      // TIF_STRIP_BYTE_COUNTS For each strip, the number of bytes in the strip after compression.

                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);

                // FF D8 Start of Image - w/o data segment
                var b1 = binaryReader.ReadUInt16();
                b1 = SwapBytes(b1);

                // FF C4 Define Huffman Table(s)
                var b2 = binaryReader.ReadUInt16();
                b2 = SwapBytes(b2);

                // 00 42 Length of DHT marker segment (-2)
                var b3 = binaryReader.ReadUInt16();
                b3 = SwapBytes(b3);

                var size = (b3 - 2) / 2 - 1;

                // Read 32 bytes, table 0 -- bits
                var b4 = binaryReader.ReadByte();
                for (var i = 0; i < size; i++)
                {
                    var b5 = binaryReader.ReadByte();
                }

                // Read 32 bytes, table 1 -- values
                var b6 = binaryReader.ReadByte();
                for (var i = 0; i < size; i++)
                {
                    var b7 = binaryReader.ReadByte();
                }

                // FF C3 Lossless (sequential)
                var b8 = binaryReader.ReadUInt16();
                b8 = SwapBytes(b8);

                var b9 = binaryReader.ReadUInt16();
                b9 = SwapBytes(b9);

                var precision = binaryReader.ReadByte();

                var height = binaryReader.ReadUInt16();
                height = SwapBytes(height);

                var samples = binaryReader.ReadUInt16();
                samples = SwapBytes(samples);

                var nf = binaryReader.ReadByte();

                var width = samples * nf;

                for (var i = 8; i < b9; i += 3)
                {
                    var b1A = binaryReader.ReadByte();
                    var b1B = binaryReader.ReadByte();
                    var b1C = binaryReader.ReadByte();
                }

                // FF DA Start of Scan
                var bA = binaryReader.ReadUInt16();
                bA = SwapBytes(bA);

                // Length
                var bB = binaryReader.ReadUInt16();
                bB = SwapBytes(bB);

                for (var i = 2; i < bB; i++)
                {
                    var b1A = binaryReader.ReadByte();
                }

                // FE D5 
                var bC = binaryReader.ReadUInt16();
                bC = SwapBytes(bC);

                var imageSize = width * height;

                var pos = binaryReader.BaseStream.Position - 2;
                var rawSize = address + length - pos;
                // var imageData = new ushort[rawSize];

                binaryReader.BaseStream.Seek(pos, SeekOrigin.Begin);
                var rawData = binaryReader.ReadBytes((int)rawSize);

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

        private static ushort SwapBytes(ushort data)
        {
            var upper = (data & (ushort)0x00FF) << 8;
            var lower = (data & (ushort)0xFF00) >> 8;
            return (ushort)(lower | upper);
        }
    }
}