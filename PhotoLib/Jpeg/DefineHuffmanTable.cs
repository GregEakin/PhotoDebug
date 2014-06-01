// Project Photo Library 0.1
// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoDebug
// FILE:		DefineHuffmanTable.cs
// AUTHOR:		Greg Eakin

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PhotoLib.Jpeg
{
    /// <summary>
    /// DHT 0xFFC4
    /// </summary>
    public class DefineHuffmanTable : JpegTag
    {
        #region Fields

        private readonly ushort length;

        private readonly Dictionary<byte, HuffmanTable> tables = new Dictionary<byte, HuffmanTable>();

        #endregion

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
                // HT Info, bits 0..3 is number, bit 4 is 0 = DC, 1 = AC, bits 5..7 must be zero
                var index = binaryReader.ReadByte();
                var data1 = binaryReader.ReadBytes(16);
                var sum = data1.Sum(b => b);
                // sum must be <= 256
                var data2 = binaryReader.ReadBytes(sum);
                this.tables.Add(index, new HuffmanTable(index, data1, data2));
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

        public Dictionary<byte, HuffmanTable> Tables
        {
            get
            {
                return tables;
            }
        }

        #endregion

        #region Public Methods and Operators

        public override string ToString()
        {
            var retval = new StringBuilder();
            foreach (var table in tables.Values)
            {
                retval.AppendLine(table.ToString());
                retval.AppendLine();
            }
            return retval.ToString();
        }

        #endregion
    }
}