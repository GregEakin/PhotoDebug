// Project Photo Library 0.1
// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoDebug
// FILE:		WhiteBalance.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;

namespace PhotoLib.Tiff
{
    public class WhiteBalance
    {
        public WhiteBalance(BinaryReader binaryReader, ImageFileEntry imageFileEntry)
        {
            if (binaryReader.BaseStream.Position != imageFileEntry.ValuePointer)
            {
                binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
            }

            var ar = 0;
            var length = binaryReader.ReadUInt16();
            Console.WriteLine("0x{0} Len = {1} Length", ar.ToString("X4"), length);
            ar += 2;
            for (var i = 0; i < 6; i++)
            {
                var v1 = binaryReader.ReadUInt16();
                var v2 = binaryReader.ReadUInt16();
                var v3 = binaryReader.ReadUInt16();
                var v4 = binaryReader.ReadUInt16();
                Console.WriteLine("0x{0} {1}: [{2}, {3}, {4}, {5}]", ar.ToString("X4"), i, v1, v2, v3, v4);
                ar += 8;
            }

            //var eob = binaryReader.ReadUInt16();
            //Console.WriteLine("0x{0} EOB = {1}", ar.ToString("X4"), eob);
            //ar += 2;
            //for (var i = 0; i < 10; i++)
            //{
            //    var x1 = binaryReader.ReadUInt16();

            //    var v1 = binaryReader.ReadUInt16();
            //    var v2 = binaryReader.ReadUInt16();
            //    var v3 = binaryReader.ReadUInt16();
            //    var v4 = binaryReader.ReadUInt16();

            //    Console.WriteLine("0x{0} {1}: [{2}, {3}, {4}, {5}]", ar.ToString("X4"), x1, v1, v2, v3, v4);
            //    ar += 10;
            //}

            for (var i = ar; i < imageFileEntry.NumberOfValue; i++)
            {
                Console.WriteLine("0x{0} : {1}, ", ar.ToString("X4"), binaryReader.ReadUInt16());
                ar += 2;
            }

            Console.WriteLine();
        }
    }
}