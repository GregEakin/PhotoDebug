using System.IO;

namespace PhotoLib.Jpeg.JpegTags
{
    public class StartOfImageRgb : StartOfImage
    {
        public StartOfImageRgb(BinaryReader binaryReader, uint address, uint length)
            : base(binaryReader, address, length)
        {
        }

        public ushort[][] ReadImage()
        {
            var memory = new ushort[StartOfFrame.ScanLines][];          // 3950 x 5920
            var pp = new[] { (ushort)0x2000, (ushort)0x2000 };
            for (var line = 0; line < StartOfFrame.ScanLines; line++) // 0 .. 3950
            {
                var diff = ReadDiffRow();
                var memory1 = ProcessDiff(diff, pp);
                memory[line] = memory1;
            }

            return memory;
        }

        public short[] ReadDiffRow()
        {
            int samplesPerLine = StartOfFrame.SamplesPerLine;

            var diff = new short[2 * samplesPerLine];
            for (var x = 0; x < samplesPerLine; x++)
            {
                diff[2 * x + 0] = ProcessColor(0x00);
                diff[2 * x + 1] = ProcessColor(0x01);
            }

            return diff;
        }

        public static ushort[] ProcessDiff(short[] diff, ushort[] pp)
        {
            var memory = new ushort[diff.Length];
            var step = pp.Length;
            for (var x = 0; x < diff.Length; x++)   //  0..2960
            {
                if (x / step == 0)
                {
                    var pred = pp[x % step];
                    pp[x % step] += (ushort)diff[x];
                    memory[x] = (ushort)(pred + diff[x]);
                }
                else
                {
                    var pred = memory[x - step];
                    memory[x] = (ushort)(pred + diff[x]);
                }
            }

            return memory;
        }
    }
}

