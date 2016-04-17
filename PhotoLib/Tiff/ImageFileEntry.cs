// Project Photo Library 0.1
// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoDebug
// FILE:		ImageFileEntry.cs
// AUTHOR:		Greg Eakin

using System.IO;

namespace PhotoLib.Tiff
{
    public class ImageFileEntry
    {
        #region Fields

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

        private readonly uint numberOfValue;

        private readonly ushort tagId;

        private readonly ushort tagType;

        private readonly uint valuePointer;

        #endregion

        #region Constructors and Destructors

        public ImageFileEntry(BinaryReader binaryReader)
        {
            tagId = binaryReader.ReadUInt16();
            tagType = binaryReader.ReadUInt16();
            numberOfValue = binaryReader.ReadUInt32();
            valuePointer = binaryReader.ReadUInt32();
        }

        #endregion

        #region Public Properties

        public uint NumberOfValue
        {
            get
            {
                return numberOfValue;
            }
        }

        public ushort TagId
        {
            get
            {
                return tagId;
            }
        }

        public ushort TagType
        {
            get
            {
                return tagType;
            }
        }

        public uint ValuePointer
        {
            get
            {
                return valuePointer;
            }
        }

        #endregion
    }
}