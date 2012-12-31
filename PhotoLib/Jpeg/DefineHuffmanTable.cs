namespace PhotoLib.Jpeg
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public struct Table
    {
        #region Fields

        private readonly byte[] data1;

        private readonly byte[] data2;

        private readonly byte index;

        #endregion

        #region Constructors and Destructors

        public Table(byte index, byte[] data1, byte[] data2)
        {
            this.index = index;
            this.data1 = data1;
            this.data2 = data2;
        }

        #endregion

        #region Public Properties

        public byte[] Data1
        {
            get
            {
                return data1;
            }
        }

        public byte[] Data2
        {
            get
            {
                return data2;
            }
        }

        public byte Index
        {
            get
            {
                return index;
            }
        }

        #endregion
    }

    public class DefineHuffmanTable : JpegTag
    {
        #region Fields

        private readonly ushort length;

        private readonly Dictionary<byte, Table> tables = new Dictionary<byte, Table>();

        #endregion

        // DHT: Define Huffman Table

        #region Constructors and Destructors

        public DefineHuffmanTable(BinaryReader binaryReader)
            : base(binaryReader)
        {
            if (Mark != 0xFF || Tag != 0xC4)
            {
                throw new ArgumentException();
            }

            length = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());

            var size = 2;
            while (size + 17 <= length)
            {
                // HT Info, bits 0..3 is number, bits 4 is 0 = DC, 1 = AC, bits 5..7 must be zero
                var index = binaryReader.ReadByte();
                var data1 = binaryReader.ReadBytes(16);
                var sum = data1.Aggregate(0, (current, b) => current + b);
                // sum must be <= 256
                var data2 = binaryReader.ReadBytes(sum);
                this.tables.Add(index, new Table(index, data1, data2));
                size += 1 + data1.Length + data2.Length;
            }

            if (size != length)
            {
                throw new ArgumentException();
            }
        }

        #endregion

        #region Public Properties

        public ushort Length
        {
            get
            {
                return length;
            }
        }

        public IEnumerable<Table> Tables
        {
            get
            {
                return tables.Values;
            }
        }

        #endregion

        #region Public Methods and Operators

        public static string[] BuildTree(Table table)
        {
            var retval = new string[table.Data2.Length];
            var index = 0;
            var bits = 0;
            for (var i = 0; i < 16; i++)
            {
                bits = bits << 1;
                for (var j = 0; j < table.Data1[i]; j++)
                {
                    retval[index] = PrintBits(bits, i);
                    bits++;
                    index++;
                }
            }
            return retval;
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

        public void DumpTable()
        {
            foreach (var table in tables.Values)
            {
                // HT Info, bits 0..3 is number, bits 4 is 0 = DC, 1 = AC, bits 5..7 must be zero
                var tableNumber = table.Index & 0x07;
                var tableType = (table.Index & 0x08) == 0 ? "DC" : "AC";
                Console.WriteLine("Table {0} {1}", tableType, tableNumber);
                var bits = BuildTree(table);

                var index = 0;
                for (byte i = 0; i < 16; i++)
                {
                    if (table.Data1[i] <= 0)
                    {
                        continue;
                    }

                    Console.Write("{0} : ", i + 1);
                    for (var j = 0; j < table.Data1[i]; j++)
                    {
                        Console.Write("{0:x} ({1}) ", table.Data2[index], bits[index]);
                        index++;
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
        }

        public static int DcValueEncoding(byte dcCode, byte bits)
        {
            int retval;
            if (dcCode > 0)
            {
                var mask = (1u << dcCode) - 1;
                var num = bits & mask;

                var sign = bits & (1u << (dcCode - 1));
                if (sign == 0)
                {
                    var i = (int)(num ^ mask);
                    retval = -1 * i;
                }
                else
                {
                    retval = (int)num;
                }
            }
            else
            {
                retval = 0;
            }

            return retval;
        }

        #endregion
    }
}