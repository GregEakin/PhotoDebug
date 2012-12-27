namespace PhotoLib.Jpeg
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

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

    public class HuffmanTable
    {
        #region Fields

        private readonly short length;

        private readonly byte mark;

        private readonly Dictionary<byte, Table> tables = new Dictionary<byte, Table>();

        private readonly byte tag;

        #endregion

        #region Constructors and Destructors

        public HuffmanTable(BinaryReader binaryReader)
        {
            mark = binaryReader.ReadByte();
            tag = binaryReader.ReadByte(); // JPG_MARK_DHT

            if (mark != 0xFF || tag != 0xC4)
            {
                throw new ArgumentException();
            }

            length = (short)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());

            var size = 2;
            while (size + 17 <= length)
            {
                var index = binaryReader.ReadByte();
                var data1 = binaryReader.ReadBytes(16);
                var sum = data1.Aggregate(0, (current, b) => current + b);
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

        public short Length
        {
            get
            {
                return length;
            }
        }

        public byte Mark
        {
            get
            {
                return mark;
            }
        }

        public IEnumerable<Table> Tables
        {
            get
            {
                return tables.Values;
            }
        }

        public byte Tag
        {
            get
            {
                return tag;
            }
        }

        #endregion
    }
}