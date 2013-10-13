namespace PhotoLib.Jpeg
{
    using System.Collections.Generic;
    using System.Text;

    using PhotoLib.Utilities;

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
            this.dictionary = this.BuildTree();
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

        public string[] BuildTextTree()
        {
            var retval = new string[this.data2.Length];
            var offset = 0;
            var bits = 0;
            for (var i = 0; i < 16; i++)
            {
                bits = bits << 1;
                for (var j = 0; j < this.data1[i]; j++)
                {
                    retval[offset] = PrintBits(bits, i);
                    bits++;
                    offset++;
                }
            }
            return retval;
        }

        private Dictionary<int, HCode> BuildTree()
        {
            var retval = new Dictionary<int, HCode>();

            var offset = 0;
            var bits = 0;
            for (var i = 0; i < 16; i++)
            {
                bits = bits << 1;
                for (var j = 0; j < this.data1[i]; j++)
                {
                    var value = new HCode { Code = this.data2[offset], Length = (byte)(i + 1) };
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
            var bits = this.BuildTextTree();

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

            public byte Code;

            public byte Length;

            #endregion
        }
    }
}