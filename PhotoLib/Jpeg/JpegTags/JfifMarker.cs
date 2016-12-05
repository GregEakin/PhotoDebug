// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		JfifMarker.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;
using System.Text;

namespace PhotoLib.Jpeg.JpegTags
{
    /// <summary>
    /// APP0 0xFFEO
    /// </summary>
    public class JfifMarker : JpegTag
    {
        public JfifMarker(BinaryReader binaryReader)
            : base(binaryReader)
        {
            if (Mark != 0xFF || Tag != 0xE0)
                throw new ArgumentException();

            Length = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());

            var identifer = binaryReader.ReadBytes(5);
            if (Encoding.ASCII.GetString(identifer) != "JFIF\0")
                throw new ArgumentException();

            Version = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            Units = binaryReader.ReadByte();
            DensityX = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            DensityY = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            ThumbX = binaryReader.ReadByte();
            ThumbY = binaryReader.ReadByte();

            var thumbLen = 3 * ThumbX * ThumbY;
            Thumb = binaryReader.ReadBytes(thumbLen);

            var size = 16 + thumbLen;

            if (size != Length)
                throw new ArgumentException();
        }

        public ushort Length { get; }

        public ushort Version { get; }

        public byte Units { get; }

        public ushort DensityX { get; }

        public ushort DensityY { get; }

        public byte ThumbX { get; }

        public byte ThumbY { get; }

        public byte[] Thumb { get; }
    }
}