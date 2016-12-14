// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		VrdData.cs
// AUTHOR:		Greg Eakin

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PhotoLib.RecipeData
{
    public abstract class VrdData
    {
        public sealed class VrdRow
        {
            public uint Position { get; }
            public string Type { get; }
            public uint Address { get; }
            public uint Length { get; }
            public uint F1 { get; }
            public uint F2 { get; }
            public uint F3 { get; }

            public VrdRow(BinaryReader binaryReader)
            {
                Position = binaryReader.ReadUInt32();
                Type = ParseType(binaryReader.ReadUInt32());
                F1 = binaryReader.ReadUInt32();
                F2 = binaryReader.ReadUInt32();
                F3 = binaryReader.ReadUInt32();
                Address = binaryReader.ReadUInt32();
                Length = binaryReader.ReadUInt32();
            }

            private static string ParseType(uint type)
            {
                switch (type)
                {
                    case 1:
                        return "int32u";
                    case 2:
                        return "string";
                    case 8:
                        return "int32u";
                    case 9:
                        return "int32s";
                    case 13:
                        return "double";
                    case 33:
                        return "int32u array";
                    case 38:
                        return "double array";
                    case 253:
                        return "unknown";
                    case 254:
                        return "unknown";
                    case 255:
                        return "unknown";
                    default:
                        throw new NotImplementedException();
                }
            }

            public override string ToString()
            {
                return $"key: 0x{Position:X8} addr: 0x{Address:X8}  type: {Type} size: {Length}";
                // Console.WriteLine($"        0x{f1:X8}  0x{f2:X8} 0x{f3:X8}");
            }
        }

        public static List<VrdData> ParseFile(BinaryReader binaryReader)
        {
            binaryReader.BaseStream.Seek(-2, SeekOrigin.End);
            var endTag = binaryReader.ReadByte() << 8 | binaryReader.ReadByte();
            if (endTag != 0xFFD9)
                throw new Exception();

            binaryReader.BaseStream.Seek(-64, SeekOrigin.End);
            var endBytes = binaryReader.ReadBytes(20);
            var endString = Encoding.ASCII.GetString(endBytes);
            if (endString != "CANON OPTIONAL DATA\0")
                throw new Exception();

            var offset = SwapBytes(binaryReader.ReadUInt32());

            binaryReader.BaseStream.Seek(-offset - 92, SeekOrigin.End);

            var startBytes = binaryReader.ReadBytes(20);
            var startString = Encoding.ASCII.GetString(startBytes);
            if (startString != "CANON OPTIONAL DATA\0")
                throw new Exception();

            var y1 = SwapBytes(binaryReader.ReadUInt32());  //0x00010000
            if (y1 != 0x00010000)
                throw new Exception();

            var y2 = SwapBytes(binaryReader.ReadUInt32());  //0x00001A30 (0) == Offset
            if (y2 != offset)
                throw new Exception();

            binaryReader.BaseStream.Seek(-offset - 64, SeekOrigin.End);
            var p1 = binaryReader.BaseStream.Position;

            var list = new List<VrdData>();
            while (true)
            {
                //var pos = binaryReader.BaseStream.Position - p1;
                //Console.WriteLine($"O = 0x{offset:X8}, O-64 = 0x{offset+64:x8}, pos = 0x{pos:X8}");
                // O = 0x00001610, O-64 = 0x00001650, pos = 0x00000000
                // O = 0x00001A30, O-64 = 0x00001a70, pos = 0x00000000
                // O = 0x00040788, O-64 = 0x000407c8, pos = 0x00000000
                // O = 0x00040788, O-64 = 0x000407c8, pos = 0x0003ED58

                var key = SwapBytes(binaryReader.ReadUInt32());
                var next = SwapBytes(binaryReader.ReadUInt32());
                var length = SwapBytes(binaryReader.ReadUInt32());

                //Console.WriteLine($"k: 0x{key:X8}, s: 0x{start:X8}, l: 0x{length:X8}");
                //Console.WriteLine($"               s: 0x{offset - start:X8}, l: 0x{offset - length:X8}");

                // k: 0xFFFF00F7, o: 0x00001610, n: 0x00001608 (O - 8), l: 0x00001600 (O - 16)
                // k: 0xFFFF00F7, o: 0x00001A30, n: 0x00001A28 (O - 8), l: 0x00001A20 (O - 16)

                // k: 0xFFFF00F4, o: 0x00040788, n: 0x0003ED50 (O - 0x1A38), l: 0x0000045E (O - 0x4032A)
                // k: 0xFFFF00F7, o: 0x00040788, n: 0x00001A28 (O - 0x3ED60), l: 0x00001A20 (O - 0x3ED68)

                if (key == 0xFFFF00F4)
                {
                    // Edit data
                    list.Add(new VrdDataF4(binaryReader, next, length));
                }
                else if (key == 0xFFFF00F5)
                {
                    // IHL data
                    throw new NotImplementedException();
                }
                else if (key == 0xFFFF00F6)
                {
                    // XMP data
                    throw new NotImplementedException();
                }
                else if (key == 0xFFFF00F7)
                {
                    list.Add(new VrdDataF7(binaryReader, next, length));
                }
                else if (key == 0x43414E4F)
                {
                    var exit1 = binaryReader.ReadUInt32();
                    var exit2 = binaryReader.ReadUInt32();
                    break; 
                }
                else
                {
                    throw new Exception();
                }
            }


            //var bytes = binaryReader.ReadBytes(20);
            //var tag = Encoding.ASCII.GetString(bytes);
            //if (tag != "CANON OPTIONAL DATA\0")
            //    throw new Exception();

            return list;
        }

        protected static uint SwapBytes(uint data)
        {
            var x1 = (data & 0x000000FF) << 24;
            var x2 = (data & 0x0000FF00) << 8;
            var x3 = (data & 0x00FF0000) >> 8;
            var x4 = (data & 0xFF000000) >> 24;
            return x1 | x2 | x3 | x4;
        }

        public abstract void DumpData();
    }

    public class VrdDataF4 : VrdData
    {
        public byte[] D1 { get; }
        public byte[] D2 { get; }

        public VrdDataF4(BinaryReader binaryReader, uint next, uint length)
        {
            // next == 0x0003ed50
            // length == 0x0000045E
            // var data = binaryReader.ReadBytes((int)(next - 04));
            D1 = binaryReader.ReadBytes((int)length);
            D2 = binaryReader.ReadBytes((int) (next - D1.Length) - 4);

            /////
            //var vrd1 = binaryReader.ReadBytes(0x272);

            // tool count == 3140
            // var stampToolCount = SwapBytes(binaryReader.ReadUInt32());

            //var vrd2 = binaryReader.ReadBytes(2000);
            //for (var i = 0; i < vrd2.Length - 4; i++)
            //{
            //    if (vrd2[i] == 0x58 || vrd2[i] == 0xDC || vrd2[i] == 0xDF || vrd2[i] == 0xE0)
            //        Console.WriteLine($"0x{i:X4}: 0x{vrd2[i - 2]:X2} 0x{vrd2[i - 1]:X2} 0x{vrd2[i]:X2} 0x{vrd2[i + 1]:X2} 0x{vrd2[i + 2]:X2} 0x{vrd2[i + 3]:X2}");
            //}
        }

        public override void DumpData()
        {
            Console.WriteLine($"D1.Length = {D1.Length}");
            Console.WriteLine($"D2.Length = {D2.Length}");
        }
    }

    public class VrdDataF7 : VrdData
    {
        public VrdRow[] Data { get; }
        public byte[] Heap { get; }
        public uint Camera { get; }

        public VrdDataF7(BinaryReader binaryReader, uint next, uint length)
        {
            // next == 0x00001608, length == 0x00001600
            // next == 0x00001A28, length == 0x00001A20

            var i1 = binaryReader.ReadInt32();             // 'IIII'
            if (i1 != 0x49494949)
                throw new Exception();

            var m1 = binaryReader.ReadInt32();             // 0x00040004
            if (m1 != 0x00040004)
                throw new Exception();

            var m2 = binaryReader.ReadInt32();             // 0x00000006
            if (m2 != 0x00000006)
                throw new Exception();

            Camera = binaryReader.ReadUInt32();            // 7d == 0x80000250

            var m4 = binaryReader.ReadInt32();             // 0x00000003
            if (m4 != 0x00000003)
                throw new Exception();

            var m5 = binaryReader.ReadInt32();             // 0x00000004
            if (m5 != 0x00000004)
                throw new Exception();

            var m6 = binaryReader.ReadInt32();             // 0x00000005
            if (m6 != 0x00000005)
                throw new Exception();

            var count = binaryReader.ReadInt32(); // dir count    // 0x00000051u
            var heapAddress = binaryReader.ReadInt32(); // offset       // 0x000019FCu  (O - 34)

            Data = new VrdRow[count];

            for (var i = 0; i < count; i++)
                Data[i] = new VrdRow(binaryReader);

            var heapSize = heapAddress - count * 28 + 4;
            Heap = binaryReader.ReadBytes(heapSize);
        }

        public override void DumpData()
        {
            Console.WriteLine($"Camera 0x{Camera:X8}");

            foreach (var row in Data)
                Console.WriteLine(row.ToString());

            var heapSize = Heap.Length;
            for (var i = 0; i < heapSize; i += 4)
            {
                if (i > 20 && i < heapSize - 20)
                    continue;

                var s0 = Heap[i + 0] << 8 | Heap[i + 1];
                var s1 = Heap[i + 2] << 8 | Heap[i + 3];

                var n = i + Data.Length * 28 + 36;
                Console.WriteLine($"0x{n:X8}: 0x{s0:X4}  0x{s1:X4}");
            }
        }
    }
}