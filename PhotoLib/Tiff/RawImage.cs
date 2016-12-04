// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		RawImage.cs
// AUTHOR:		Greg Eakin

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PhotoLib.Tiff
{
    public class RawImage
    {
        #region Fields

        private readonly Dictionary<uint, ImageFileDirectory> directoryList = new Dictionary<uint, ImageFileDirectory>();

        private readonly CR2Header header;

        #endregion

        #region Constructors and Destructors

        public RawImage(BinaryReader binaryReader)
        {
            header = new CR2Header(binaryReader);

            var next = header.TiffOffset;
            while (next > 0)
            {
                binaryReader.BaseStream.Seek(next, SeekOrigin.Begin);
                var dir = new ImageFileDirectory(binaryReader);
                directoryList.Add(next, dir);
                next = dir.NextEntry;
            }

            next = header.RawIfdOffset;
            while (next > 0 && !directoryList.ContainsKey(next))
            {
                binaryReader.BaseStream.Seek(next, SeekOrigin.Begin);
                var dir = new ImageFileDirectory(binaryReader);
                directoryList.Add(next, dir);
                next = dir.NextEntry;
            }
        }

        #endregion

        #region Public Properties

        public IEnumerable<ImageFileDirectory> Directories
        {
            get
            {
                return directoryList.Values;
            }
        }

        public CR2Header Header
        {
            get
            {
                return header;
            }
        }

        public ImageFileDirectory this[uint key]
        {
            get
            {
                return directoryList[key];
            }

            set
            {
                directoryList[key] = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        public void DumpHeader(BinaryReader binaryReader)
        {
            foreach (var item in directoryList)
            {
                Console.WriteLine("== Tiff Direcotry [0x{0}]:", item.Key.ToString("X8"));
                item.Value.DumpDirectory(binaryReader);
            }
        }

        public static byte[] ReadBytes(BinaryReader binaryReader, ImageFileEntry imageFileEntry)
        {
            var retval = new byte[imageFileEntry.NumberOfValue];

            if (binaryReader.BaseStream.Position != imageFileEntry.ValuePointer)
            {
                binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
            }

            for (var j = 0; j < imageFileEntry.NumberOfValue; j++)
            {
                var us = binaryReader.ReadByte();
                retval[j] = us;
            }

            return retval;
        }

        public static string ReadChars(BinaryReader binaryReader, ImageFileEntry imageFileEntry)
        {
            if (imageFileEntry.NumberOfValue <= 4)
            {
                var bytes = new[]
                {
                    (byte)(imageFileEntry.ValuePointer >>  0 & 0xFF),
                    (byte)(imageFileEntry.ValuePointer >>  8 & 0xFF),
                    (byte)(imageFileEntry.ValuePointer >> 16 & 0xFF),
                    (byte)(imageFileEntry.ValuePointer >> 24 & 0xFF),
                };

                return imageFileEntry.NumberOfValue == 4u && bytes[3] != 0
                    ? Encoding.ASCII.GetString(bytes, 0, (int)imageFileEntry.NumberOfValue)
                    : Encoding.ASCII.GetString(bytes, 0, (int)imageFileEntry.NumberOfValue - 1);
            }

            if (binaryReader.BaseStream.Position != imageFileEntry.ValuePointer)
            {
                binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
            }

            //var data = new byte[imageFileEntry.NumberOfValue];
            //for (var i = 0; i < imageFileEntry.NumberOfValue; i++)
            //    data[i] = binaryReader.ReadByte();

            //return data[imageFileEntry.NumberOfValue - 1] != 0
            //    ? Encoding.ASCII.GetString(data, 0, (int)imageFileEntry.NumberOfValue)
            //    : Encoding.ASCII.GetString(data, 0, (int)imageFileEntry.NumberOfValue - 1);

            var retval = new StringBuilder();

            for (var i = 0; i < imageFileEntry.NumberOfValue; i++)
            {
                var us = binaryReader.ReadByte();
                retval.Append((char)us);
            }

            var lastPosition = retval.Length - 1;
            if (retval[lastPosition] == 0)
            {
                retval.Remove(lastPosition, 1);
            }

            return retval.ToString();
        }

        public static UInt16[] ReadUInts16(BinaryReader binaryReader, ImageFileEntry imageFileEntry)
        {
            var retval = new UInt16[imageFileEntry.NumberOfValue];

            if (binaryReader.BaseStream.Position != imageFileEntry.ValuePointer)
            {
                binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
            }

            for (var j = 0; j < imageFileEntry.NumberOfValue; j++)
            {
                var us = binaryReader.ReadUInt16();
                retval[j] = us;
            }

            return retval;
        }

        public static UInt32[] ReadRational(BinaryReader binaryReader, ImageFileEntry imageFileEntry)
        {
            var retval = new UInt32[imageFileEntry.NumberOfValue * 2];

            if (binaryReader.BaseStream.Position != imageFileEntry.ValuePointer)
            {
                binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
            }

            for (var j = 0; j < imageFileEntry.NumberOfValue * 2; j++)
            {
                var us = binaryReader.ReadUInt32();
                retval[j] = us;
            }

            return retval;
        }

        public static UInt32[] ReadUInts(BinaryReader binaryReader, ImageFileEntry imageFileEntry)
        {
            var retval = new UInt32[imageFileEntry.NumberOfValue];

            if (binaryReader.BaseStream.Position != imageFileEntry.ValuePointer)
            {
                binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
            }

            for (var j = 0; j < imageFileEntry.NumberOfValue; j++)
            {
                var us = binaryReader.ReadUInt32();
                retval[j] = us;
            }

            return retval;
        }

        #endregion
    }
}