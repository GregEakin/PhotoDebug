using System;
using System.IO;
using System.Text;

namespace PhotoLib.RecipeData
{
    public class VrdData
    {
        public VrdData(BinaryReader binaryReader)
        {
            binaryReader.BaseStream.Seek(-2, SeekOrigin.End);
            var endTag = binaryReader.ReadByte() << 8 | binaryReader.ReadByte();
            if (endTag != 0xFFD9)
                throw new Exception();

            binaryReader.BaseStream.Seek(-42, SeekOrigin.End);
            var offset = binaryReader.ReadByte() << 8 | binaryReader.ReadByte();

            binaryReader.BaseStream.Seek(-64, SeekOrigin.End);
            var endBytes = binaryReader.ReadBytes(20);
            var end = Encoding.ASCII.GetString(endBytes);
            if (end != "CANON OPTIONAL DATA\0")
                throw new Exception();

            binaryReader.BaseStream.Seek(-offset - 92, SeekOrigin.End);
            var startBytes = binaryReader.ReadBytes(20);
            var start = Encoding.ASCII.GetString(startBytes);
            if (start != "CANON OPTIONAL DATA\0")
                throw new Exception();

            var y1 = SwapBytes(binaryReader.ReadUInt32());  //0x00010000
            var y2 = SwapBytes(binaryReader.ReadUInt32());  //0x00001A30 (0) == Offset
            var y3 = SwapBytes(binaryReader.ReadUInt32());  //0xFFFF00F7
            if (y3 != 0xFFFF00F7)
                throw new Exception();

            var y4 = SwapBytes(binaryReader.ReadUInt32());  //0x00001A28 (-8)
            var y5 = SwapBytes(binaryReader.ReadUInt32());  //0x00001A20 (-16)

            var i1 = binaryReader.ReadUInt32();             // 'IIII'
            // Assert.AreEqual(0x49494949u, i1);
            var m1 = binaryReader.ReadUInt32();             // 0x00040004
            // Assert.AreEqual(0x00040004u, m1);
            var m2 = binaryReader.ReadUInt32();             // 0x00000006
            // Assert.AreEqual(0x00000006u, m2);
            var m3 = binaryReader.ReadUInt32();  // camera model    // 0x80000250
            var m4 = binaryReader.ReadUInt32();             // 0x00000003
            // Assert.AreEqual(0x00000003u, m4);
            var m5 = binaryReader.ReadUInt32();             // 0x00000004
            // Assert.AreEqual(0x00000004u, m5);
            var m6 = binaryReader.ReadUInt32();             // 0x00000005
            // Assert.AreEqual(0x00000005u, m6);

            var x6 = binaryReader.ReadUInt32(); // dir count    // 0x00000051u
            var x7 = binaryReader.ReadUInt32(); // offset       // 0x000019FCu  (-34)

            for (var i = 0; i < x6; i++)
            {
                var x1 = binaryReader.ReadUInt32();
                var x2 = binaryReader.ReadUInt32();
                var f1 = binaryReader.ReadUInt32();
                var f2 = binaryReader.ReadUInt32();
                var f3 = binaryReader.ReadUInt32();
                var x3 = binaryReader.ReadUInt32();
                var x4 = binaryReader.ReadUInt32();

                var n = i * 28 + 36;
                Console.WriteLine($"k:0x{n:X8}: 0x{x1:X8}  f:{x2} a:0x{x3:X8} s:{x4}");
                // Console.WriteLine($"        0x{f1:X8}  0x{f2:X8} 0x{f3:X8}");

                // vrd fromat - X2
                // 1: int32u
                // 2: string
                // 8: int32u
                // 9: int32u
                // 13: double
                // 33: int32u array
                // 38: double array
            }

            var ll = (int)(offset - x6 * 28 - 48);
            var heap = binaryReader.ReadBytes(ll);
            for (var i = ll - 40; i < heap.Length; i += 4)
            {
                var s0 = heap[i + 0] << 8 | heap[i + 1];
                var s1 = heap[i + 2] << 8 | heap[i + 3];

                var n = i + x6 * 28 + 36;
                Console.WriteLine($"0x{n:X8}: 0x{s0:X4}  0x{s1:X4}");
            }

            var bytes = binaryReader.ReadBytes(20);
            var tag = Encoding.ASCII.GetString(bytes);
            if (tag != "CANON OPTIONAL DATA\0")
                throw new Exception();

        }

        public string Data { get; }

        private static uint SwapBytes(uint data)
        {
            var x1 = (data & 0x000000FF) << 24;
            var x2 = (data & 0x0000FF00) << 8;
            var x3 = (data & 0x00FF0000) >> 8;
            var x4 = (data & 0xFF000000) >> 24;
            return x1 | x2 | x3 | x4;
        }
    }
}