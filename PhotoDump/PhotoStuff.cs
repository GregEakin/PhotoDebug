namespace PhotoDump
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using PhotoLib.Jpeg;
    using PhotoLib.Tiff;

    public class PhotoStuff
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public ushort[] Array { get; private set; }

        public PhotoStuff(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                var binaryReader = new BinaryReader(fileStream);
                var rawImage = new RawImage(binaryReader);
                var imageFileDirectory = rawImage.Directories.Last();

                var strips = imageFileDirectory.Entries.First(e => e.TagId == 0xC640 && e.TagType == 3).ValuePointer; // TIF_CR2_SLICE
                binaryReader.BaseStream.Seek(strips, SeekOrigin.Begin);
                var blockCount = binaryReader.ReadUInt16();
                var blockWidth = binaryReader.ReadUInt16();
                var blockRest = binaryReader.ReadUInt16();

                var address = imageFileDirectory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var lossless = startOfImage.Lossless;

                var rawSize = address + length - binaryReader.BaseStream.Position - 2;
                startOfImage.ImageData = new ImageData(binaryReader, (uint)rawSize);

                var colors = lossless.Components.Sum(comp => comp.HFactor * comp.VFactor);
                var tables = startOfImage.HuffmanTable.Tables.Values.ToList();

                Width = lossless.SamplesPerLine * colors;
                Height = lossless.ScanLines;
                Array = new ushort[Width * Height];
                var pred = (ushort)(1 << (lossless.Precision - 1));
                var predictor = Enumerable.Repeat(pred, Height * colors).ToArray();

                for (var i = 0; i < Width * Height; i += 4)
                {
                    int jcol;
                    int jrow;
                    var block = i / (Height * blockWidth);
                    if (block < blockCount)
                    {
                        jrow = (i - block * Height * blockWidth) / blockWidth;
                        var x1 = (i - block * Height * blockWidth) % blockWidth;
                        jcol = block * blockWidth + x1;
                    }
                    else
                    {
                        jrow = (i - blockCount * Height * blockWidth) / blockRest;
                        var x1 = (i - blockCount * Height * blockWidth) % blockRest;
                        jcol = blockCount * blockWidth + x1;
                    }

                    var index = jrow * Width + jcol;
                    PokeValues(startOfImage, tables, jcol, jrow, index, predictor);
                }
            }
        }

        private void PokeValues(StartOfImage startOfImage, IList<HuffmanTable> tables, int x, int y, int index, IList<ushort> predictor)
        {
            const int Colors = 4;
            const int BlockWidth = 0x06c0;
            for (var i = 0; i < Colors; i++)
            {
                var hufCode = GetValue(startOfImage.ImageData, tables[0]);
                var difCode = startOfImage.ImageData.GetSetOfBits(hufCode);
                var dif = (ushort)DecodeDifBits(difCode, hufCode);

                int value;
                if (x < Colors)
                {
                    value = predictor[Colors * y + i] += dif;
                }
                else if (x % (BlockWidth * Height) == 0)
                {
                    var j = index + i - (Height - 1) * BlockWidth - 1;
                    value = this.Array[j] + dif;
                }
                else
                {
                    value = this.Array[index - Colors + i] + dif;
                }
                this.Array[index + i] = (ushort)(0x3fff & value);
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