// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		DefineHuffmanTable.cs
// AUTHOR:		Greg Eakin

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace PhotoLib.Jpeg.JpegTags
{
    /// <summary>
    /// DHT 0xFFC4
    /// </summary>
    public class DefineHuffmanTable : JpegTag
    {
        public DefineHuffmanTable(BinaryReader binaryReader)
            : base(binaryReader)
        {
            if (Mark != 0xFF || Tag != 0xC4)
                throw new ArgumentException();

            Length = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());

            var tables = new Dictionary<byte, HuffmanTable>();
            var size = 2;
            while (size + 17 <= Length)
            {
                // 0 for DC, 1 for AC; 0 for the Y component and 1 for the colour components
                var index = binaryReader.ReadByte();
                var data1 = binaryReader.ReadBytes(16);
                var sum = data1.Sum(b => b);
                var data2 = binaryReader.ReadBytes(sum);
                tables.Add(index, new HuffmanTable(index, data1, data2));
                size += 1 + data1.Length + data2.Length;
            }

            if (size != Length)
                throw new ArgumentException();

            Tables = new ReadOnlyDictionary<byte, HuffmanTable>(tables);
        }

        public ushort Length { get; }

        public ReadOnlyDictionary<byte, HuffmanTable> Tables { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var table in Tables.Values)
            {
                builder.AppendLine(table.ToString());
                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}