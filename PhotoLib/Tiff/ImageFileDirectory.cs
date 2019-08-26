// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		ImageFileDirectory.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace PhotoLib.Tiff
{
    public class ImageFileDirectory
    {
        // private readonly byte[] heap;

        public ImageFileDirectory(ushort length)
        {
            Entries = new ImageFileEntry[length];
        }

        public ImageFileDirectory(BinaryReader binaryReader)
        {
            // var dirStart = binaryReader.BaseStream.Position;

            var length = binaryReader.ReadUInt16();
            Entries = new ImageFileEntry[length];
            for (var i = 0; i < length; i++)
            {
                Entries[i] = new ImageFileEntry(binaryReader);
            }
            var next = binaryReader.ReadUInt32();
            NextEntry = next;

            //if (next == 0) next = (uint)(binaryReader.BaseStream.Length + 1);
            //Console.WriteLine($"### Directory {length}, [0x{dirStart:X8} - 0x{next - 1:X8}]");
            //var heapStart = binaryReader.BaseStream.Position;
            //Console.WriteLine($"    Heap [0x{heapStart:X8} - 0x{next - 1:X8}]");
        }

        public ImageFileEntry[] Entries { get; }

        public uint NextEntry { get; }

        public ImageFileEntry this[ushort key]
        {
            get
            {
                return Entries.FirstOrDefault(imageFileEntry => imageFileEntry.TagId == key);
            }
        }

        public void DumpDirectory(BinaryReader binaryReader, string prefix = "")
        {
            const string blockHeader = ":0x{1:X4} {0,2})  {2}: ";
            const string referencedItem = "[0x{0:X8}] ({1}): ";
            const string rationalItem = "{0}/{1} = {2}";

            var count = -1;
            foreach (var entry in Entries)
            {
                if (!string.IsNullOrWhiteSpace(prefix))
                    Console.Write(prefix);

                count++;

                if (entry.TagType == 0x01 && entry.TagId == 0x02BC)    // XMP metadata
                {
                    Console.Write(blockHeader, count, entry.TagId, "XMP metadata");
                    Console.WriteLine(referencedItem, entry.ValuePointer, entry.NumberOfValue);

                }
                else if (entry.TagType == 0x04 && entry.TagId == 0x8769) // TIF_EXIF IFD - A pointer to the Exif IFD.
                {
                    Console.Write(blockHeader, count, entry.TagId, "Image File Directory");
                    Console.WriteLine(referencedItem, entry.ValuePointer, entry.NumberOfValue);
                    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                    var tags = new ImageFileDirectory(binaryReader);
                    tags.DumpDirectory(binaryReader, prefix + $":0x{entry.TagId:X4}");
                }
                else if (entry.TagType == 0x04 && entry.TagId == 0x8825) // GPSInfo.
                {
                    Console.Write(blockHeader, count, entry.TagId, "GPS Info");
                    Console.WriteLine(referencedItem, entry.ValuePointer, entry.NumberOfValue);
                    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                    var tags = new ImageFileDirectory(binaryReader);
                    tags.DumpDirectory(binaryReader, prefix + $":0x{entry.TagId:X4}");
                }
                else if (entry.TagType == 0x07 && entry.TagId == 0x927c) // Makernote.
                {
                    Console.Write(blockHeader, count, entry.TagId, "Maker note");
                    Console.WriteLine(referencedItem, entry.ValuePointer, entry.NumberOfValue);
                    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                    var tags = new ImageFileDirectory(binaryReader);
                    tags.DumpDirectory(binaryReader, prefix + $":0x{entry.TagId:X4}");
                }
                else if (entry.TagType == 0x04 && entry.TagId == 0xA005) // Interoperability IFD Pointer
                {
                    Console.Write(blockHeader, count, entry.TagId, "Interoperability IFD");
                    Console.WriteLine(referencedItem, entry.ValuePointer, entry.NumberOfValue);
                    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                    var tags = new ImageFileDirectory(binaryReader);
                    tags.DumpDirectory(binaryReader, prefix + $":0x{entry.TagId:X4}");
                }
                //else if (entry.TagType == 0x04 && entry.TagId == 0x8825)
                //{
                //    Console.Write(blockHeader, count, entry.TagId, "??");
                //    Console.WriteLine(referencedItem, entry.ValuePointer, entry.NumberOfValue);
                //    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                //    var tags = new ImageFileDirectory(binaryReader);
                //    tags.DumpDirectory(binaryReader, prefix + $":0x{entry.TagId:X4}");
                //}
                else
                {
                    switch (entry.TagType)
                    {
                        case 0x01:  // ubyte
                            Console.Write(blockHeader, count, entry.TagId, "UByte 8-bit");
                            if (entry.NumberOfValue == 1)
                            {
                                Console.Write("{0:x2}", entry.ValuePointer);
                            }
                            else
                            {
                                Console.Write(referencedItem, entry.ValuePointer, entry.NumberOfValue);
                                if (binaryReader.BaseStream.Position != entry.ValuePointer)
                                {
                                    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                                }

                                for (var j = 0; j < entry.NumberOfValue; j++)
                                {
                                    var us = binaryReader.ReadByte();
                                    Console.Write("{0:x2}, ", us);
                                }
                            }
                            Console.WriteLine();
                            break;

                        case 0x02:  // string, null terminated
                            Console.Write(blockHeader, count, entry.TagId, "Ascii 8-bit, null terminated");
                            Console.Write(referencedItem, entry.ValuePointer, entry.NumberOfValue);

                            if (entry.NumberOfValue < 5)
                            {
                                var bytes = new[]
                                {
                                    (byte)(entry.ValuePointer >>  0 & 0xFF),
                                    (byte)(entry.ValuePointer >>  8 & 0xFF),
                                    (byte)(entry.ValuePointer >> 16 & 0xFF),
                                    (byte)(entry.ValuePointer >> 24 & 0xFF),
                                };
                                var str = Encoding.ASCII.GetString(bytes, 0, (int)entry.NumberOfValue - 1);
                                Console.WriteLine("\"{0}\"", str);
                            }
                            else
                            {
                                if (binaryReader.BaseStream.Position != entry.ValuePointer)
                                    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);

                                var len = entry.NumberOfValue;
                                var bytes = binaryReader.ReadBytes((int)len);
                                var str = Encoding.ASCII.GetString(bytes);
                                var zero = str.IndexOf('\0');
                                if (zero >= 0)
                                    str = str.Substring(0, zero);
                                Console.WriteLine("\"{0}\"", str);
                            }

                            break;

                        case 0x03:  // ushort
                            Console.Write(blockHeader, count, entry.TagId, "UShort 16-bit");
                            if (entry.NumberOfValue == 1)
                            {
                                Console.Write("{0}", entry.ValuePointer);
                            }
                            else
                            {
                                Console.Write(referencedItem, entry.ValuePointer, entry.NumberOfValue);
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
                            Console.Write(blockHeader, count, entry.TagId, "ULong 32-bit");
                            if (entry.NumberOfValue == 1)
                            {
                                Console.Write("{0}", entry.ValuePointer);
                            }
                            else
                            {
                                Console.Write(referencedItem, entry.ValuePointer, entry.NumberOfValue);
                                if (binaryReader.BaseStream.Position != entry.ValuePointer)
                                {
                                    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                                }

                                for (var j = 0; j < entry.NumberOfValue; j++)
                                {
                                    var long1 = binaryReader.ReadUInt32();
                                    Console.Write("{0:X4} ", long1);
                                }
                            }
                            Console.WriteLine();
                            break;

                        case 0x05:  // urational, numeration & denominator ulongs
                            Console.Write(blockHeader, count, entry.TagId, "URational 2x32-bit");
                            Console.Write(referencedItem, entry.ValuePointer, entry.NumberOfValue);
                            if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            {
                                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                            }

                            var us1 = binaryReader.ReadUInt32();
                            var us2 = binaryReader.ReadUInt32();
                            Console.WriteLine(rationalItem, us1, us2, us1 / (double)us2);
                            break;

                        case 0x06:  // sbyte
                            Console.Write(blockHeader, count, entry.TagId, "SByte 8-bit");
                            Console.WriteLine(referencedItem, entry.ValuePointer, entry.NumberOfValue);
                            throw new NotImplementedException($"Undfined message {entry.TagType}");

                        case 0x07:  // ubyte sequence
                            Console.Write(blockHeader, count, entry.TagId, "UByte[]");
                            if (entry.NumberOfValue <= 4)
                            {
                                Console.Write("{0}, ", entry.ValuePointer >> 0 & 0xFF);
                                Console.Write("{0}, ", entry.ValuePointer >> 8 & 0xFF);
                                Console.Write("{0}, ", entry.ValuePointer >> 16 & 0xFF);
                                Console.Write("{0}", entry.ValuePointer >> 24 & 0xFF);
                            }
                            else
                            {
                                Console.Write(referencedItem, entry.ValuePointer, entry.NumberOfValue);
                            }
                            Console.WriteLine();
                            break;

                        case 0x08:  // short
                            Console.Write(blockHeader, count, entry.TagId, "SShort 16-bit");
                            throw new NotImplementedException($"Undfined message {entry.TagType}");

                        case 0x09:  // long
                            Console.Write(blockHeader, count, entry.TagId, "SLong 32-bit");
                            throw new NotImplementedException($"Undfined message {entry.TagType}");

                        case 0x0A:  // rational, signed two longs
                            Console.Write(blockHeader, count, entry.TagId, "SRational 2x32-bit");
                            Console.Write(referencedItem, entry.ValuePointer, entry.NumberOfValue);
                            if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            {
                                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                            }

                            var s1 = binaryReader.ReadInt32();
                            var s2 = binaryReader.ReadInt32();
                            Console.WriteLine(rationalItem, s1, s2, s1 / (double)s2);
                            break;

                        case 0x0B:  // single precision, 2 bytes IEEE format
                            Console.Write(blockHeader, count, entry.TagId, "Float 4-Byte");
                            Console.Write(referencedItem, entry.ValuePointer, entry.NumberOfValue);
                            if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            {
                                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                            }

                            var x1 = binaryReader.ReadSingle();
                            Console.WriteLine("{0}", x1);
                            throw new NotImplementedException($"Undfined message {entry.TagType}");

                        case 0x0C:  // double precision, 4 bytes IEEE format
                            Console.Write(blockHeader, count, entry.TagId, "Double 8-Byte");
                            if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            {
                                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                            }

                            var x2 = binaryReader.ReadDouble();
                            Console.WriteLine("{0}", x2);
                            throw new NotImplementedException($"Undfined message {entry.TagType}");

                        default:
                            Console.Write(blockHeader, count, entry.TagId, "Undefined");
                            throw new NotImplementedException($"Undfined message {entry.TagType}");
                    }
                }
            }
        }
    }
}