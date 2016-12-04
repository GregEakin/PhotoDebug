// Project Photo Library 0.1
// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		RawData.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;

namespace PhotoLib.Tiff
{
    public class RawData
    {
        #region Fields

        private readonly byte[] data;

        #endregion

        #region Constructors and Destructors

        public RawData(BinaryReader binaryReader, int height, int x, int y, int z)
        {
            var width = x * y + z;
            data = new byte[height * width];

            for (var block = 0; block < x; block++)
            {
                for (var row = 0; row < height; row++)
                {
                    var b = binaryReader.ReadBytes(y);
                    var b1 = row * width + block * y;
                    Array.Copy(b, 0L, data, b1, y);
                }
            }
            for (var row = 0; row < height; row++)
            {
                var c = binaryReader.ReadBytes(z);
                var c1 = row * width + x * y;
                Array.Copy(c, 0L, data, c1, z);
            }
        }

        #endregion

        #region Public Properties

        public byte[] Data
        {
            get
            {
                return data;
            }
        }

        #endregion
    }
}