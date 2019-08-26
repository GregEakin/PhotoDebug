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
        private readonly Dictionary<uint, ImageFileDirectory> _directoryList = new Dictionary<uint, ImageFileDirectory>();

        public RawImage(BinaryReader binaryReader)
        {
            Header = new CR2Header(binaryReader);

            var next = Header.TiffOffset;
            while (next > 0)
            {
                binaryReader.BaseStream.Seek(next, SeekOrigin.Begin);
                var dir = new ImageFileDirectory(binaryReader);
                _directoryList.Add(next, dir);
                next = dir.NextEntry;
            }

            next = Header.RawIfdOffset;
            while (next > 0 && !_directoryList.ContainsKey(next))
            {
                binaryReader.BaseStream.Seek(next, SeekOrigin.Begin);
                var dir = new ImageFileDirectory(binaryReader);
                _directoryList.Add(next, dir);
                next = dir.NextEntry;
            }
        }

        public IEnumerable<ImageFileDirectory> Directories => _directoryList.Values;

        public CR2Header Header { get; }

        public ImageFileDirectory this[uint key]
        {
            get => _directoryList[key];
            set => _directoryList[key] = value;
        }

        public void DumpHeader(BinaryReader binaryReader)
        {
            var index = 0;
            foreach (var item in _directoryList)
            {
                Console.WriteLine("== Tiff Directory [0x{0:X8}]:", item.Key);
                item.Value.DumpDirectory(binaryReader, $"IFD{index++}");
            }
        }

        public static byte[] ReadBytes(BinaryReader binaryReader, ImageFileEntry imageFileEntry)
        {
            var bytes = new byte[imageFileEntry.NumberOfValue];

            if (binaryReader.BaseStream.Position != imageFileEntry.ValuePointer)
            {
                binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
            }

            for (var j = 0; j < imageFileEntry.NumberOfValue; j++)
            {
                var us = binaryReader.ReadByte();
                bytes[j] = us;
            }

            return bytes;
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

            var builder = new StringBuilder();

            for (var i = 0; i < imageFileEntry.NumberOfValue; i++)
            {
                var us = binaryReader.ReadByte();
                builder.Append((char)us);
            }

            var lastPosition = builder.Length - 1;
            if (builder[lastPosition] == 0)
            {
                builder.Remove(lastPosition, 1);
            }

            return builder.ToString();
        }

        public static UInt16[] ReadUInts16(BinaryReader binaryReader, ImageFileEntry imageFileEntry)
        {
            var ushorts = new UInt16[imageFileEntry.NumberOfValue];

            if (binaryReader.BaseStream.Position != imageFileEntry.ValuePointer)
            {
                binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
            }

            for (var j = 0; j < imageFileEntry.NumberOfValue; j++)
            {
                var us = binaryReader.ReadUInt16();
                ushorts[j] = us;
            }

            return ushorts;
        }

        public static UInt32[] ReadRational(BinaryReader binaryReader, ImageFileEntry imageFileEntry)
        {
            var uints = new UInt32[imageFileEntry.NumberOfValue * 2];

            if (binaryReader.BaseStream.Position != imageFileEntry.ValuePointer)
            {
                binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
            }

            for (var j = 0; j < imageFileEntry.NumberOfValue * 2; j++)
            {
                var us = binaryReader.ReadUInt32();
                uints[j] = us;
            }

            return uints;
        }

        public static UInt32[] ReadUInts(BinaryReader binaryReader, ImageFileEntry imageFileEntry)
        {
            var uints = new UInt32[imageFileEntry.NumberOfValue];

            if (binaryReader.BaseStream.Position != imageFileEntry.ValuePointer)
            {
                binaryReader.BaseStream.Seek(imageFileEntry.ValuePointer, SeekOrigin.Begin);
            }

            for (var j = 0; j < imageFileEntry.NumberOfValue; j++)
            {
                var us = binaryReader.ReadUInt32();
                uints[j] = us;
            }

            return uints;
        }
    }
}