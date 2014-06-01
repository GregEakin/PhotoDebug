// Project Photo Library 0.1
// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoDebug
// FILE:		JpegTag.cs
// AUTHOR:		Greg Eakin

using System.IO;

namespace PhotoLib.Jpeg
{
    public abstract class JpegTag
    {
        #region Fields

        private readonly byte mark;

        private readonly byte tag;

        #endregion

        #region Constructors and Destructors

        protected JpegTag(BinaryReader binaryReader)
        {
            mark = binaryReader.ReadByte();
            tag = binaryReader.ReadByte();
        }

        #endregion

        #region Public Properties

        public byte Mark
        {
            get
            {
                return mark;
            }
        }

        public byte Tag
        {
            get
            {
                return tag;
            }
        }

        #endregion
    }
}