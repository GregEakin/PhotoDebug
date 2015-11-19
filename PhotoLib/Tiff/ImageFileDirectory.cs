// Project Photo Library 0.1
// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoDebug
// FILE:		ImageFileDirectory.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;
using System.Linq;
using System.Text;
using PhotoLib.Utilities;

namespace PhotoLib.Tiff
{
    public class ImageFileDirectory
    {
        #region Fields

        private readonly ImageFileEntry[] entries;

        private readonly uint nextEntry;

        // private readonly byte[] heap;

        #endregion

        #region Constructors and Destructors

        public ImageFileDirectory(ushort length)
        {
            this.entries = new ImageFileEntry[length];
        }

        public ImageFileDirectory(BinaryReader binaryReader)
        {
            var dirStart = binaryReader.BaseStream.Position;

            var length = binaryReader.ReadUInt16();
            this.entries = new ImageFileEntry[length];
            for (var i = 0; i < length; i++)
            {
                this.entries[i] = new ImageFileEntry(binaryReader);
            }
            var next = binaryReader.ReadUInt32();
            this.nextEntry = next;

            if (next == 0) next = (uint)(binaryReader.BaseStream.Length + 1);
            Console.WriteLine("### Directory {0}, [0x{1} - 0x{2}]", length, dirStart.ToString("X8"), (next - 1).ToString("X8"));
            var heapStart = binaryReader.BaseStream.Position;
            Console.WriteLine("    Heap [0x{0} - 0x{1}]", heapStart.ToString("X8"), (next - 1).ToString("X8"));
        }

        #endregion

        #region Public Properties

        public ImageFileEntry[] Entries
        {
            get
            {
                return this.entries;
            }
        }

        public uint NextEntry
        {
            get
            {
                return this.nextEntry;
            }
        }

        public ImageFileEntry this[ushort key]
        {
            get
            {
                return this.entries.FirstOrDefault(imageFileEntry => imageFileEntry.TagId == key);
            }
        }

        #endregion

        #region Public Methods and Operators

        public void DumpDirectory(BinaryReader binaryReader)
        {
            const string BlockHeader = "{0,2})  0x{1} {2}: ";
            const string ReferencedItem = "[0x{0}] ({1}): ";
            const string RationalItem = "{0}/{1} = {2}";

            var count = -1;
            foreach (var entry in this.Entries)
            {
                count++;

                if (entry.TagType == 0x04 && entry.TagId == 0x8769) // TIF_EXIF IFD - A pointer to the Exif IFD.
                {
                    Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "Image File Directory");
                    Console.WriteLine(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);
                    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                    var tags = new ImageFileDirectory(binaryReader);
                    tags.DumpDirectory(binaryReader);
                }
                if (entry.TagType == 0x07 && entry.TagId == 0x927c) // Makernote.
                {
                    Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "Maker note");
                    Console.WriteLine(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);
                    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                    var tags = new ImageFileDirectory(binaryReader);
                    tags.DumpDirectory(binaryReader);
                }
                else
                {
                    switch (entry.TagType)
                    {
                        case 0x01:  // ubyte
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "UByte 8-bit");
                            Console.WriteLine(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);
                            break;

                        case 0x02:  // string, null terminated
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "Ascii 8-bit, null terminated");
                            Console.Write(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);

                            if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            {
                                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                            }

                            var len = entry.NumberOfValue;
                            var bytes = binaryReader.ReadBytes((int)len);
                            var str = Encoding.ASCII.GetString(bytes);
                            var zero = str.IndexOf('\0');
                            if (zero >= 0)
                                str = str.Substring(0, zero);
                            Console.WriteLine("\"{0}\"",str);
                            break;

                        case 0x03:  // ushort
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "UShort 16-bit");
                            if (entry.NumberOfValue == 1)
                            {
                                Console.Write("{0}", entry.ValuePointer);
                            }
                            else
                            {
                                Console.Write(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);
                                if (binaryReader.BaseStream.Position != entry.ValuePointer)
                                {
                                    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                                }

                                for (var j = 0; j < entry.NumberOfValue; j++)
                                {
                                    var us = binaryReader.ReadUInt16();
                                    Console.Write("{0}, ", us);
                                }
                            }
                            Console.WriteLine();
                            break;

                        case 0x04:  // ulong
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "ULong 32-bit");
                            if (entry.NumberOfValue == 1)
                            {
                                Console.Write("{0}", entry.ValuePointer);
                            }
                            else
                            {
                                Console.Write(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);
                                if (binaryReader.BaseStream.Position != entry.ValuePointer)
                                {
                                    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                                }

                                for (var j = 0; j < entry.NumberOfValue; j++)
                                {
                                    var long1 = binaryReader.ReadUInt32();
                                    Console.Write("{0} ", long1.ToString("X4"));
                                }
                            }
                            Console.WriteLine();
                            break;

                        case 0x05:  // urational, numeration & demoninator ulongs
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "URational 2x32-bit");
                            Console.Write(ReferencedItem, entry.ValuePointer.ToString("X8"), 2);
                            if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            {
                                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                            }

                            var us1 = binaryReader.ReadUInt32();
                            var us2 = binaryReader.ReadUInt32();
                            Console.WriteLine(RationalItem, us1, us2, us1 / (double)us2);
                            break;

                        case 0x06:  // sbyte
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "SByte 8-bit");
                            Console.WriteLine(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);
                            throw new NotImplementedException("Undfined message {0}".FormatWith(entry.TagType));

                        case 0x07:  // ubyte sequence
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "UByte[]");
                            if (entry.NumberOfValue <= 4)
                            {
                                Console.Write("{0}, ", entry.ValuePointer >> 0 & 0xFF);
                                Console.Write("{0}, ", entry.ValuePointer >> 8 & 0xFF);
                                Console.Write("{0}, ", entry.ValuePointer >> 16 & 0xFF);
                                Console.Write("{0}", entry.ValuePointer >> 24 & 0xFF);
                            }
                            else
                            {
                                Console.Write(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);
                            }
                            Console.WriteLine();
                            break;

                        case 0x08:  // short
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "SShort 16-bit");
                            throw new NotImplementedException("Undfined message {0}".FormatWith(entry.TagType));

                        case 0x09:  // long
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "SLong 32-bit");
                            throw new NotImplementedException("Undfined message {0}".FormatWith(entry.TagType));

                        case 0x0A:  // rational, signed two longs
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "SRational 2x32-bit");
                            Console.Write(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);
                            if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            {
                                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                            }

                            var s1 = binaryReader.ReadInt32();
                            var s2 = binaryReader.ReadInt32();
                            Console.WriteLine(RationalItem, s1, s2, s1 / (double)s2);
                            break;

                        case 0x0B:  // single persision, 2 bytes IEEE format
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "Float 4-Byte");
                            Console.Write(ReferencedItem, entry.ValuePointer.ToString("X8"), entry.NumberOfValue);
                            if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            {
                                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                            }

                            var x1 = binaryReader.ReadSingle();
                            Console.WriteLine("{0}", x1);
                            throw new NotImplementedException("Undfined message {0}".FormatWith(entry.TagType));

                        case 0x0C:  // double persision, 4 bytes IEEE format
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "Double 8-Byte");
                            if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            {
                                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                            }

                            var x2 = binaryReader.ReadDouble();
                            Console.WriteLine("{0}", x2);
                            throw new NotImplementedException("Undfined message {0}".FormatWith(entry.TagType));

                        default:
                            Console.Write(BlockHeader, count, entry.TagId.ToString("X4"), "Undefined");
                            throw new NotImplementedException("Undfined message {0}".FormatWith(entry.TagType));
                    }
                }
            }
        }

        #endregion
    }
}