// Project Photo Library 0.1
// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoDebug
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
            this.header = new CR2Header(binaryReader);

            var next = this.header.TiffOffset;
            while (next > 0)
            {
                binaryReader.BaseStream.Seek(next, SeekOrigin.Begin);
                var dir = new ImageFileDirectory(binaryReader);
                this.directoryList.Add(next, dir);
                next = dir.NextEntry;
            }

            next = this.header.RawIfdOffset;
            while (next > 0 && !this.directoryList.ContainsKey(next))
            {
                binaryReader.BaseStream.Seek(next, SeekOrigin.Begin);
                var dir = new ImageFileDirectory(binaryReader);
                this.directoryList.Add(next, dir);
                next = dir.NextEntry;
            }
        }

        #endregion

        #region Public Properties

        public IEnumerable<ImageFileDirectory> Directories
        {
            get
            {
                return this.directoryList.Values;
            }
        }

        public CR2Header Header
        {
            get
            {
                return this.header;
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

        public static string ReadChars(BinaryReader binaryReader, ImageFileEntry imageFileEntry)
        {
            var retval = new StringBuilder();

            if (binaryReader.BaseStream.Position != imageFileEntry.ValuePointer)
            {
                binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
            }

            for (var j = 0; j < imageFileEntry.NumberOfValue; j++)
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

        public static UInt32[] ReadULongs(BinaryReader binaryReader, ImageFileEntry imageFileEntry)
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