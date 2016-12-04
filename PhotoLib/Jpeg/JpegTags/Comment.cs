// Project Photo Library 0.1
// Copyright © 2013-2015. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoDebug
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
        #region Fields

        private readonly ushort length;

        private readonly byte[] data;

        #endregion

        #region Constructors and Destructors

        public Comment(BinaryReader binaryReader)
            : base(binaryReader)
        {
            if (Mark != 0xFF || Tag != 0xFE)
            {
                throw new ArgumentException();
            }

            length = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            data = binaryReader.ReadBytes(length - 2);
        }

        #endregion

        #region Public Properties

        public ushort Length
        {
            get { return length; }
        }

        public byte[] Data
        {
            get { return data; }
        }

        #endregion
    }
}
