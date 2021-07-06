// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
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
        public RawData(BinaryReader binaryReader, int height, int x, int y, int z)
        {
            var width = x * y + z;
            Data = new byte[height * width];

            for (var block = 0; block < x; block++)
            {
                for (var row = 0; row < height; row++)
                {
                    var b = binaryReader.ReadBytes(y);
                    var b1 = row * width + block * y;
                    Array.Copy(b, 0L, Data, b1, y);
                }
            }

            for (var row = 0; row < height; row++)
            {
                var c = binaryReader.ReadBytes(z);
                var c1 = row * width + x * y;
                Array.Copy(c, 0L, Data, c1, z);
            }
        }

        public byte[] Data { get; }
    }
}