// Project Photo Library 0.1
// Copyright © 2013-2015. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		Comment.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;

namespace PhotoLib.Jpeg.JpegTags
{
    /// <summary>
    /// Comment 0xFFFE
    /// </summary>
    public class Comment : JpegTag
    {
        public Comment(BinaryReader binaryReader)
            : base(binaryReader)
        {
            if (Mark != 0xFF || Tag != 0xFE)
            {
                throw new ArgumentException();
            }

            Length = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            Data = binaryReader.ReadBytes(Length - 2);
        }

        public ushort Length { get; }

        public byte[] Data { get; }
    }
}
