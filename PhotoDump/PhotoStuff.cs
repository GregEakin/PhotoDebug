namespace PhotoDump
{
    using System.IO;
    using System.Linq;

    using PhotoLib.Jpeg;
    using PhotoLib.Tiff;

    public class PhotoStuff
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public short[] Array { get; private set; }

        public PhotoStuff(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Last();

                var strips = imageFileDirectory.Entries.First(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE
                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var x1 = binaryReader.ReadUInt16();
                var y1 = binaryReader.ReadUInt16();
                var z1 = binaryReader.ReadUInt16();

                var address = imageFileDirectory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var lossless = startOfImage.Lossless;

                var rawSize = address + length - binaryReader.BaseStream.Position - 2;
                startOfImage.ImageData = new ImageData(binaryReader, (uint)rawSize);

                var colors = lossless.Components.Sum(comp => comp.HFactor * comp.VFactor);
                var table0 = startOfImage.HuffmanTable.Tables[0x00];
                var table1 = startOfImage.HuffmanTable.Tables[0x01];
                var predictor = new[] { (short)(1 << (lossless.Precision - 1)), (short)(1 << (lossless.Precision - 1)) };

                Width = lossless.SamplesPerLine * colors;
                Height = lossless.ScanLines;
                Array = new short[Width * Height];

                for (var i = 0; i < Width * Height; i += 2)
                {
                    int y;
                    int index;
                    var x = i / (Height * y1);
                    if (x < x1)
                    {
                        var jrow = (i - x * Height * y1) / y1;
                        y = (i - x * Height * y1) % y1;
                        index = x * y1 + jrow * Width + y;
                    }
                    else
                    {
                        var jrow = (i - x1 * Height * y1) / z1;
                        y = (i - x1 * Height * y1) % z1;
                        index = x1 * y1 + jrow * Width + y;
                    }
                    
                    PokeValues(startOfImage, table0, table1, y, index, predictor);
                }
            }
        }

        private void PokeValues(StartOfImage startOfImage, HuffmanTable table0, HuffmanTable table1, int y, int index, short[] predictor)
        {
            var hufCode0 = GetValue(startOfImage.ImageData, table0);
            var difCode0 = startOfImage.ImageData.GetSetOfBits(hufCode0);
            var dif0 = DecodeDifBits(difCode0, hufCode0);

            var hufCode1 = GetValue(startOfImage.ImageData, table1);
            var difCode1 = startOfImage.ImageData.GetSetOfBits(hufCode1);
            var dif1 = DecodeDifBits(difCode1, hufCode1);
            
            if (y == 0)
            {
                this.Array[index + 0] = (short)(predictor[0] + dif0);
                this.Array[index + 1] = (short)(predictor[1] + dif1);
            }
            else
            {
                this.Array[index + 0] = (short)(this.Array[index - 2] + dif0);
                this.Array[index + 1] = (short)(this.Array[index - 1] + dif1);
            }

            if (Array[index + 0] > 0x2FFF || Array[index + 1] > 0x2FFF)
            {
                var x = Array[index];
            }
        }

        private static short DecodeDifBits(ushort difCode, ushort difBits)
        {
            short dif0;
            if ((difCode & (0x01u << (difBits - 1))) != 0)
            {
                // msb is 1, thus decoded DifCode is positive
                dif0 = (short)difCode;
            }
            else
            {
                // msb is 0, thus DifCode is negative
                var mask = (1 << difBits) - 1;
                var m1 = difCode ^ mask;
                dif0 = (short)(0 - m1);
            }
            return dif0;
        }

        public static ushort GetValue(ImageData imageData, HuffmanTable table)
        {
            var hufIndex = (ushort)0;
            var hufBits = (ushort)0;
            HuffmanTable.HCode hCode;
            do
            {
                hufIndex = imageData.GetNextShort(hufIndex);
                hufBits++;
            }
            while (!table.Dictionary.TryGetValue(hufIndex, out hCode) || hCode.Length != hufBits);

            return hCode.Code;
        }
    }
}