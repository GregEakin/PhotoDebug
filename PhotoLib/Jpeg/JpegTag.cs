// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoDebug
// FILE:		JpegTag.cs
// AUTHOR:		Greg Eakin

using System.IO;

namespace PhotoLib.Jpeg
{
    public abstract class JpegTag
    {
        protected JpegTag(BinaryReader binaryReader)
        {
            Mark = binaryReader.ReadByte();
            Tag = binaryReader.ReadByte();
        }

        public byte Mark { get; }

        public byte Tag { get; }
    }
}