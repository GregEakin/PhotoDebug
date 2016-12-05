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
        public CR2Header(BinaryReader binaryReader)
        {
            ByteOrder = binaryReader.ReadBytes(2);     // either "II" or "MM" 
            TiffMagic = binaryReader.ReadUInt16();     // "*\0"
            TiffOffset = binaryReader.ReadUInt32();

            //var cr2Magic = binaryReader.ReadBytes(4);
            //if (Encoding.ASCII.GetString(cr2Magic) != "CR2\0")
            //    throw new ArgumentException();
            CR2Magic = binaryReader.ReadUInt16();
            CR2Version = binaryReader.ReadBytes(2);

            RawIfdOffset = binaryReader.ReadUInt32();
        }

        /// <summary>
        /// "II" or 0x4949 means Intel byte order (little endian)
        /// "MM" or 0x4d4d means Motorola byte order (big endian)
        /// </summary>
        public byte[] ByteOrder { get; }

        /// <summary>
        /// "CR" or 0x4352
        /// </summary>
        public ushort CR2Magic { get; }

        public byte[] CR2Version { get; }

        public uint RawIfdOffset { get; }

        /// <summary>
        /// 0x002a
        /// </summary>
        public ushort TiffMagic { get; }

        public uint TiffOffset { get; }
    }
}