﻿// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		HuffmanTable.cs
// AUTHOR:		Greg Eakin

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace PhotoLib.Jpeg
{
    public class HuffmanTable
    {
        public HuffmanTable(byte index, byte[] data1, byte[] data2)
        {
            Index = index;
            Data1 = data1;
            Data2 = data2;
            Dictionary = BuildTree(data1, data2);
        }

        public byte[] Data1 { get; }

        public byte[] Data2 { get; }

        public ReadOnlyDictionary<int, HCode> Dictionary { get; }

        /// <summary>
        /// HT Info, bits 0..3 is number, bit 4 is 0 = DC, 1 = AC, bits 5..7 must be zero
        /// </summary>
        public byte Index { get; }

        /// <summary>
        /// Assert.AreEqual(16, data1.Length);
        /// Assert.AreEqual(data2.Length, data1.Sum(b => b));
        /// Assert.IsTrue(data2.Length <= 256);
        /// </summary>
        public static ReadOnlyDictionary<int, HCode> BuildTree(IList<byte> data1, IList<byte> data2)
        {
            var tree = new Dictionary<int, HCode>();

            var offset = 0;
            var bits = 0;
            for (var i = 0; i < 16; i++)
            {
                bits = bits << 1;
                for (var j = 0; j < data1[i]; j++)
                {
                    var value = new HCode(data2[offset], (byte)(i + 1));
                    tree.Add(bits, value);
                    bits++;
                    offset++;
                }
            }
            return new ReadOnlyDictionary<int, HCode>(tree);
        }

        public static int DcValueEncoding(int dcCode, int bits)
        {
            if (dcCode <= 0)
                return 0;

            var sign = bits & (1u << (dcCode - 1));
            var num = bits & ((1u << dcCode) - 1);
            return sign != 0 ? (int) num : (int) num - (int) ((1u << dcCode) - 1);
        }

        public static string PrintBits(int value, int number)
        {
            var bits = new StringBuilder();
            for (var i = number; i >= 0; i--)
            {
                var mask = 0x01 << i;
                bits.Append((value & mask) != 0 ? '1' : '0');
            }
            return bits.ToString();
        }

        /// <summary>
        /// Assert.AreEqual(16, data1.Length);
        /// Assert.AreEqual(data2.Length, data1.Sum(b => b));
        /// Assert.IsTrue(data2.Length <= 256);
        /// </summary>
        public static string[] ToTextTree(IList<byte> data1, IList<byte> data2)
        {
            var textTree = new string[data2.Count];
            var offset = 0;
            var bits = 0;
            for (var i = 0; i < 16; i++)
            {
                bits = bits << 1;
                for (var j = 0; j < data1[i]; j++)
                {
                    textTree[offset] = PrintBits(bits, i);
                    bits++;
                    offset++;
                }
            }

            return textTree;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var tableType = (Index & 0x10) == 0 ? "DC" : "AC";
            var tableNumber = Index & 0x0F;
            builder.AppendLine($"HuffmanTable {tableType} {tableNumber}");
            var bits = ToTextTree(Data1, Data2);
            var offset = 0;
            for (byte i = 0; i < 16; i++)
            {
                if (Data1[i] <= 0)
                    continue;

                builder.Append($"{i + 1,2} : ");
                for (var j = 0; j < Data1[i]; j++)
                {
                    builder.Append($"{Data2[offset]:X2} ({bits[offset]}) ");
                    offset++;
                }
                builder.AppendLine();
            }

            return builder.ToString();
        }

        public struct HCode
        {
            public readonly byte Code;

            public readonly byte Length;

            public HCode(byte code, byte length)
            {
                Code = code;
                Length = length;
            }
        }

        public static short DecodeDifBits(ushort difBits, ushort difCode)
        {
            if (difBits == 0)
                return 0;

            //if (difBits >= 16)
            //    return 32768;

            if ((difCode & (0x01u << (difBits - 1))) != 0)
            {
                // msb is 1, thus decoded DifCode is positive
                return (short)difCode;
            }

            // msb is 0, thus DifCode is negative
            var mask = (1 << difBits) - 1;
            var m1 = difCode ^ mask;
            return (short)(0 - m1);
        }
    }
}
