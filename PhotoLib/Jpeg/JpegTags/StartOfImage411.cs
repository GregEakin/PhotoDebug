using System.IO;

namespace PhotoLib.Jpeg.JpegTags
{
    public class StartOfImage411 : StartOfImage
    {
        public struct DataBuf
        {
            public ushort Y;
            public short Cb;
            public short Cr;
        }

        public struct DiffBuf
        {
            public short Y1;
            public short Y2;
            public short Y3;
            public short Y4;
            public short Cb;
            public short Cr;
        }

        public StartOfImage411(BinaryReader binaryReader, uint address, uint length)
            : base(binaryReader, address, length)
        {
        }

        public DataBuf[][] ReadImage()
        {
            var memory = new DataBuf[StartOfFrame.ScanLines][];          // [2592][]
            for (var line = 0; line < StartOfFrame.ScanLines; line++)   // 0 .. 2592
            {
                var diff = ReadDiffRow();
                // VerifyDiff(diff, line);
                var prev = new DataBuf { Y = 0x4000, Cb = 0, Cr = 0 };
                var memory1 = ProcessDiff(diff, prev);
                memory[line] = memory1;
            }

            return memory;
        }

        public DiffBuf[] ReadDiffRow()
        {
            int samplesPerLine = StartOfFrame.SamplesPerLine;
            var table0 = HuffmanTable.Tables[0x00];
            var table1 = HuffmanTable.Tables[0x01];

            var diff = new DiffBuf[samplesPerLine / 4];         // 648
            for (var x = 0; x < samplesPerLine / 4; x++)        // 0..648
            {
                diff[x].Y1 = ProcessColor(table0);
                diff[x].Y2 = ProcessColor(table0);
                diff[x].Y3 = ProcessColor(table0);
                diff[x].Y4 = ProcessColor(table0);
                diff[x].Cb = ProcessColor(table1);
                diff[x].Cr = ProcessColor(table1);
            }

            return diff;
        }

        private static DataBuf[] ProcessDiff(DiffBuf[] diff, DataBuf prev)
        {
            var samplesPerLine = diff.Length * 4;
            var memory = new DataBuf[samplesPerLine];       // 2592
            for (var x = 0; x < samplesPerLine / 4; x++)    // 2592
            {
                var y1 = (ushort)(prev.Y + diff[x].Y1);
                var y2 = (ushort)(prev.Y + diff[x].Y1 + diff[x].Y2);
                var y3 = (ushort)(prev.Y + diff[x].Y1 + diff[x].Y2 + diff[x].Y3);
                var y4 = (ushort)(prev.Y + diff[x].Y1 + diff[x].Y2 + diff[x].Y3 + diff[x].Y4);
                var cb = (short)(prev.Cb + diff[x].Cb);
                var cr = (short)(prev.Cr + diff[x].Cr);

                prev.Y = y2;
                prev.Cb = cb;
                prev.Cr = cr;

                memory[4 * x].Y = y1;
                memory[4 * x].Cb = cb;
                memory[4 * x].Cr = cr;

                memory[4 * x + 1].Y = y2;
                memory[4 * x + 1].Cb = cb;
                memory[4 * x + 1].Cr = cr;
            }

            return memory;
        }
    }
}