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
                var x1 = binaryReader.ReadUInt16();
                var y1 = binaryReader.ReadUInt16();
                var z1 = binaryReader.ReadUInt16();

                var address = imageFileDirectory.Entries.First(e => e.TagId == 0x0111).ValuePointer; // TIF_STRIP_OFFSETS
                var length = imageFileDirectory.Entries.First(e => e.TagId == 0x0117).ValuePointer; // TIF_STRIP_BYTE_COUNTS
                binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);
                var startOfImage = new StartOfImage(binaryReader, address, length);
                var lossless = startOfImage.Lossless;

                var rawSize = address + length - binaryReader.BaseStream.Position;
                startOfImage.ImageData = new LinearImageData(binaryReader, (uint)rawSize);

                var colors = lossless.Components.Sum(comp => comp.HFactor * comp.VFactor);
                var table0 = startOfImage.HuffmanTable.Tables[0x00];

                Width = lossless.SamplesPerLine * colors;
                Height = lossless.ScanLines;
                Array = new ushort[Width * Height];

                for (var i = 0; i < Width * Height; i++)
                {
                    var x = i / (Height * y1);
                    if (x < x1)
                    {
                        var jrow = (i - x * Height * y1) / y1;
                        var y = (i - x * Height * y1) % y1;
                        var index = x * y1 + jrow * Width + y;
                        var val = GetValue(startOfImage.ImageData, table0);
                        var bits = startOfImage.ImageData.GetSetOfBits(val) * 128;
                        Array[index] = (ushort)bits;
                    }
                    else
                    {
                        var jrow = (i - x1 * Height * y1) / z1;
                        var y = (i - x1 * Height * y1) % z1;
                        var index = x1 * y1 + jrow * Width + y;
                        var val = GetValue(startOfImage.ImageData, table0);
                        var bits = startOfImage.ImageData.GetSetOfBits(val) * 128;
                        Array[index] = (ushort)bits;
                    }
                }

                //for (var x = 0; x < x1; x++)
                //{
                //    for (var jrow = 0; jrow < Height; jrow++)
                //    {
                //        for (var y = 0; y < y1; y++)
                //        {
                //            var index = x * y1 + jrow * Width + y;

                //            var val = GetValue(startOfImage.ImageData, table0);
                //            var bits = startOfImage.ImageData.GetSetOfBits(val) * 128;

                //            Array[index] = (ushort)bits;
                //        }
                //    }
                //}
                //for (var jrow = 0; jrow < Height; jrow++)
                //{
                //    for (var z = 0; z < z1; z++)
                //    {
                //        var index = x1 * y1 + jrow * Width + z;

                //        var val = GetValue(startOfImage.ImageData, table0);
                //        var bits = startOfImage.ImageData.GetSetOfBits(val) * 128;

                //        Array[index] = (ushort)bits;
                //    }
                //}
            }
        }

        public static ushort GetValue(IImageData imageData, HuffmanTable table)
        {
            var hufIndex = (ushort)0;
            var hufBits = (ushort)0;
            HuffmanTable.HCode hCode;
            do
            {
                hufIndex = imageData.GetNextBit(hufIndex);
                hufBits++;
            }
            while (!table.Dictionary.TryGetValue(hufIndex, out hCode) || hCode.Length != hufBits);

            return hCode.Code;
        }
    }
}