// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		ImageFileEntry.cs
// AUTHOR:		Greg Eakin

using System.IO;

namespace PhotoLib.Tiff
{
    public class ImageFileEntry
    {
        // TagTypes
        // 1 BYTE       8-bit unsigned integer
        // 2 ASCII      8-bit, NULL-terminated string
        // 3 SHORT      16-bit unsigned integer
        // 4 LONG       32-bit unsigned integer
        // 5 RATIONAL   Two 32-bit unsigned integers, numerator and denominator
        // 6 SBYTE      8-bit signed integer
        // 7 UNDEFINE   8-bit byte
        // 8 SSHORT     16-bit signed integer
        // 9 SLONG      32-bit signed integer
        // 10 SRATIONAL Two 32-bit signed integers
        // 11 FLOAT     4-byte single-precision IEEE floating-point value
        // 12 DOUBLE    8-byte double-precision IEEE floating-point value

        public ImageFileEntry(BinaryReader binaryReader)
        {
            TagId = binaryReader.ReadUInt16();
            TagType = binaryReader.ReadUInt16();
            NumberOfValue = binaryReader.ReadUInt32();
            ValuePointer = binaryReader.ReadUInt32();
        }

        public uint NumberOfValue { get; }

        public ushort TagId { get; }

        public ushort TagType { get; }

        public uint ValuePointer { get; }
    }
}