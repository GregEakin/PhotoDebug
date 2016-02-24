// Project Photo Library 0.1
// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoDebug
// FILE:		HuffmanTable.cs
// AUTHOR:		Greg Eakin

using System.Collections.Generic;
using System.Text;

using PhotoLib.Utilities;

namespace PhotoLib.Jpeg
{
    public class HuffmanTable
    {
        #region Fields

        private readonly byte[] data1;

        private readonly byte[] data2;

        private readonly Dictionary<int, HCode> dictionary;

        private readonly byte index;

        #endregion

        #region Constructors and Destructors

        public HuffmanTable(byte index, byte[] data1, byte[] data2)
        {
            this.index = index;
            this.data1 = data1;
            this.data2 = data2;
            this.dictionary = BuildTree(data1, data2);
        }

        #endregion

        #region Public Properties

        public byte[] Data1
        {
            get
            {
                return this.data1;
            }
        }

        public byte[] Data2
        {
            get
            {
                return this.data2;
            }
        }

        public Dictionary<int, HCode> Dictionary
        {
            get
            {
                return this.dictionary;
            }
        }

        /// <summary>
        /// HT Info, bits 0..3 is number, bit 4 is 0 = DC, 1 = AC, bits 5..7 must be zero
        /// </summary>
        public byte Index
        {
            get
            {
                return this.index;
            }
        }

        #endregion

        #region Public Methods and Operators

        public static int DcValueEncoding(int dcCode, int bits)
        {
            int retval;
            if (dcCode <= 0)
            {
                retval = 0;
            }
            else
            {
                var sign = bits & (1u << (dcCode - 1));
                var num = bits & ((1u << dcCode) - 1);
                retval = sign != 0 ? (int)num : (int)num - (int)((1u << dcCode) - 1);
            }
            return retval;
        }

        public static void DecodeD()
        {
            // var c = 0;
            // var j = 0;
            // while (c > maxcoded[j]) do
            {
                //nbit
                //c = 2 * c + bit
                //j = j + 1
            }
            //val = huffvald[valptrd[j] + c - mincoded[j]]
        }

        public static void DecodeA()
        {
            //  except that the number val (byte) now is divided up in two half-bytes: 
            // nz = val div 16 
            // val = val - nz * 16 - the first half-byte nz stating a number of zeros.
        }

        public static string PrintBits(int value, int number)
        {
            var retval = new StringBuilder();
            for (var i = number; i >= 0; i--)
            {
                var mask = 0x01 << i;
                retval.Append((value & mask) != 0 ? '1' : '0');
            }
            return retval.ToString();
        }

        /// <summary>
        /// Assert.AreEqual(16, data1.Length);
        /// Assert.AreEqual(data2.Length, data1.Sum(b => b));
        /// Assert.IsTrue(data2.Length <= 256);
        /// </summary>
        public static string[] ToTextTree(IList<byte> data1, IList<byte> data2)
        {
            var retval = new string[data2.Count];
            var offset = 0;
            var bits = 0;
            for (var i = 0; i < 16; i++)
            {
                bits = bits << 1;
                for (var j = 0; j < data1[i]; j++)
                {
                    retval[offset] = PrintBits(bits, i);
                    bits++;
                    offset++;
                }
            }

            return retval;
        }

        /// <summary>
        /// Assert.AreEqual(16, data1.Length);
        /// Assert.AreEqual(data2.Length, data1.Sum(b => b));
        /// Assert.IsTrue(data2.Length <= 256);
        /// </summary>
        public static Dictionary<int, HCode> BuildTree(IList<byte> data1, IList<byte> data2)
        {
            var retval = new Dictionary<int, HCode>();

            var offset = 0;
            var bits = 0;
            for (var i = 0; i < 16; i++)
            {
                bits = bits << 1;
                for (var j = 0; j < data1[i]; j++)
                {
                    var value = new HCode(data2[offset], (byte)(i + 1));
                    retval.Add(bits, value);
                    bits++;
                    offset++;
                }
            }
            return retval;
        }

        public override string ToString()
        {
            var retval = new StringBuilder();
            var tableNumber = this.index & 0x0F;
            var tableType = (this.index & 0x10) == 0 ? "DC" : "AC";

            retval.AppendLine("HuffmanTable {0} {1}".FormatWith(tableType, tableNumber));
            var bits = ToTextTree(data1, data2);

            var offset = 0;
            for (byte i = 0; i < 16; i++)
            {
                if (this.data1[i] <= 0)
                {
                    continue;
                }

                retval.Append("{0,2} : ".FormatWith(i + 1));
                for (var j = 0; j < this.data1[i]; j++)
                {
                    retval.Append("{0} ({1}) ".FormatWith(this.data2[offset].ToString("X2"), bits[offset]));
                    offset++;
                }
                retval.AppendLine();
            }

            return retval.ToString();
        }

        #endregion

        public struct HCode
        {
            #region Fields

            public readonly byte Code;

            public readonly byte Length;

            #endregion

            public HCode(byte code, byte length)
            {
                this.Code = code;
                this.Length = length;
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