// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		DefineHuffmanTable.cs
// AUTHOR:		Greg Eakin

using System;
using System.Collections.Generic;
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
            {
                throw new ArgumentException();
            }

            Length = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());

            var size = 2;
            while (size + 17 <= Length)
            {
                // 0 for DC, 1 for AC; 0 for the Y component and 1 for the colour components
                var index = binaryReader.ReadByte();
                var data1 = binaryReader.ReadBytes(16);
                var sum = data1.Sum(b => b);
                var data2 = binaryReader.ReadBytes(sum);
                Tables.Add(index, new HuffmanTable(index, data1, data2));
                size += 1 + data1.Length + data2.Length;
            }

            if (size != Length)
            {
                throw new ArgumentException();
            }
        }

        public ushort Length { get; }

        public Dictionary<byte, HuffmanTable> Tables { get; } = new Dictionary<byte, HuffmanTable>();

        public override string ToString()
        {
            var retval = new StringBuilder();
            foreach (var table in Tables.Values)
            {
                retval.AppendLine(table.ToString());
                retval.AppendLine();
            }
            return retval.ToString();
        }
    }
}