// Project Photo Library 0.1
// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		StartOfImageYcc.cs
// AUTHOR:		Greg Eakin

using System.IO;

namespace PhotoLib.Jpeg.JpegTags
{
    public class StartOfImageYcc : StartOfImage
    {
        public struct DiffBuf
        {
            public short Y1;
            public short Y2;
            public short Cb;
            public short Cr;
        }

        public struct DataBuf
        {
            public ushort Y;
            public short Cb;
            public short Cr;
        }

        public StartOfImageYcc(BinaryReader binaryReader, uint address, uint length)
            : base(binaryReader, address, length)
        {
        }

        public DataBuf[][] ReadImage()
        {
            var memory = new DataBuf[StartOfFrame.ScanLines][];          // [1728][]
            var prev = new DataBuf { Y = 0x4000, Cb = 0, Cr = 0 };
            for (var line = 0; line < StartOfFrame.ScanLines; line++) // 0 .. 3950
            {
                var diff = ReadDiffRow();
                var memory1 = ProcessDiff(diff, prev);
                memory[line] = memory1;
            }

            return memory;
        }

        public DiffBuf[] ReadDiffRow()
        {
            int samplesPerLine = StartOfFrame.SamplesPerLine;

            var diff = new DiffBuf[samplesPerLine / 2];         // 1296
            for (var x = 0; x < samplesPerLine / 2; x++)        // 1296
            {
                diff[x].Y1 = ProcessColor(0x00);
                diff[x].Y2 = ProcessColor(0x00);
                diff[x].Cb = ProcessColor(0x01);
                diff[x].Cr = ProcessColor(0x01);
            }

            return diff;
        }

        private static DataBuf[] ProcessDiff(DiffBuf[] diff, DataBuf prev)
        {
            var memory = new DataBuf[2 * diff.Length];       // 2592
            for (var x = 0; x < diff.Length; x++)    // 2592
            {
                var y1 = (ushort)(prev.Y + diff[x].Y1);
                var y2 = (ushort)(prev.Y + diff[x].Y1 + diff[x].Y2);
                var cb = (short)(prev.Cb + diff[x].Cb);
                var cr = (short)(prev.Cr + diff[x].Cr);

                prev.Y = y2;
                prev.Cb = cb;
                prev.Cr = cr;

                memory[2 * x].Y = y1;
                memory[2 * x].Cb = cb;
                memory[2 * x].Cr = cr;

                memory[2 * x + 1].Y = y2;
                memory[2 * x + 1].Cb = cb;
                memory[2 * x + 1].Cr = cr;
            }

            return memory;
        }
    }
}
