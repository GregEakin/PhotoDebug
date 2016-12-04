// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		CR2Header.cs
// AUTHOR:		Greg Eakin

using System.IO;

namespace PhotoLib.Tiff
{
    /// <summary>
    /// TIFF and CR2 file header
    /// </summary>
    public class CR2Header
    {
        #region Fields

        private readonly byte[] byteOrder;

        private readonly ushort cr2Magic;

        private readonly byte[] cr2Version;

        private readonly uint rawIfdOffset;

        private readonly ushort tiffMagic;

        private readonly uint tiffOffset;

        #endregion

        #region Constructors and Destructors

        public CR2Header(BinaryReader binaryReader)
        {
            byteOrder = binaryReader.ReadBytes(2);     // either "II" or "MM" 
            tiffMagic = binaryReader.ReadUInt16();     // "*\0"
            tiffOffset = binaryReader.ReadUInt32();

            //var cr2Magic = binaryReader.ReadBytes(4);
            //if (Encoding.ASCII.GetString(cr2Magic) != "CR2\0")
            //    throw new ArgumentException();
            cr2Magic = binaryReader.ReadUInt16();
            cr2Version = binaryReader.ReadBytes(2);

            rawIfdOffset = binaryReader.ReadUInt32();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// "II" or 0x4949 means Intel byte order (little endian)
        /// "MM" or 0x4d4d means Motorola byte order (big endian)
        /// </summary>
        public byte[] ByteOrder
        {
            get
            {
                return byteOrder;
            }
        }

        /// <summary>
        /// "CR" or 0x4352
        /// </summary>
        public ushort CR2Magic
        {
            get
            {
                return cr2Magic;
            }
        }

        public byte[] CR2Version
        {
            get
            {
                return cr2Version;
            }
        }

        public uint RawIfdOffset
        {
            get
            {
                return rawIfdOffset;
            }
        }

        /// <summary>
        /// 0x002a
        /// </summary>
        public ushort TiffMagic
        {
            get
            {
                return tiffMagic;
            }
        }

        public uint TiffOffset
        {
            get
            {
                return tiffOffset;
            }
        }

        #endregion
    }
}