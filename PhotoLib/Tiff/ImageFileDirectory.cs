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

namespace PhotoLib.Tiff;

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
        //builder.AppendLine(string.Format($"### Directory {length}, [0x{dirStart:X8} - 0x{next - 1:X8}]");
        //var heapStart = binaryReader.BaseStream.Position;
        //builder.AppendLine(string.Format($"    Heap [0x{heapStart:X8} - 0x{next - 1:X8}]");
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

    public string DumpDirectory(BinaryReader binaryReader, string prefix = "")
    {
        const string blockHeader = ":0x{1:X4} {0,2})  {2}: ";
        const string referencedItem = "[0x{0:X8}] ({1}): ";
        const string rationalItem = "{0}/{1} = {2}";
        var builder = new StringBuilder();

        var count = -1;
        foreach (var entry in Entries)
        {
            if (!string.IsNullOrWhiteSpace(prefix))
                builder.Append(prefix);

            count++;

            if (entry.TagType == ImageFileEntry.TagTypes.UByte && entry.TagId == 0x02BC)    // XMP metadata
            {
                builder.Append(string.Format(blockHeader, count, entry.TagId, "XMP metadata"));
                builder.AppendLine(string.Format(referencedItem, entry.ValuePointer, entry.NumberOfValue));

            }
            else if (entry.TagType == ImageFileEntry.TagTypes.ULong && entry.TagId == 0x8769) // TIF_EXIF IFD - A pointer to the Exif IFD.
            {
                builder.Append(string.Format(blockHeader, count, entry.TagId, "Image File Directory"));
                builder.AppendLine(string.Format(referencedItem, entry.ValuePointer, entry.NumberOfValue));
                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                var tags = new ImageFileDirectory(binaryReader);
                builder.Append(tags.DumpDirectory(binaryReader, prefix + $":0x{entry.TagId:X4}"));
            }
            else if (entry.TagType == ImageFileEntry.TagTypes.ULong && entry.TagId == 0x8825) // GPSInfo.
            {
                builder.Append(string.Format(blockHeader, count, entry.TagId, "GPS Info"));
                builder.AppendLine(string.Format(referencedItem, entry.ValuePointer, entry.NumberOfValue));
                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                var tags = new ImageFileDirectory(binaryReader);
                builder.Append(tags.DumpDirectory(binaryReader, prefix + $":0x{entry.TagId:X4}"));
            }
            else if (entry.TagType == ImageFileEntry.TagTypes.UByteSeq && entry.TagId == 0x927c) // Makernote.
            {
                builder.Append(string.Format(blockHeader, count, entry.TagId, "Maker note"));
                builder.AppendLine(string.Format(referencedItem, entry.ValuePointer, entry.NumberOfValue));
                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                var tags = new ImageFileDirectory(binaryReader);
                builder.Append(tags.DumpDirectory(binaryReader, prefix + $":0x{entry.TagId:X4}"));
            }
            else if (entry.TagType == ImageFileEntry.TagTypes.ULong && entry.TagId == 0xA005) // Interoperability IFD Pointer
            {
                builder.Append(string.Format(blockHeader, count, entry.TagId, "Interoperability IFD"));
                builder.AppendLine(string.Format(referencedItem, entry.ValuePointer, entry.NumberOfValue));
                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                var tags = new ImageFileDirectory(binaryReader);
                builder.Append(tags.DumpDirectory(binaryReader, prefix + $":0x{entry.TagId:X4}"));
            }
            //else if (entry.TagType == ImageFileEntry.TagTypes.ULong && entry.TagId == 0x8825)
            //{
            //    builder.Append(string.Format(blockHeader, count, entry.TagId, "??");
            //    builder.AppendLine(string.Format(referencedItem, entry.ValuePointer, entry.NumberOfValue);
            //    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
            //    var tags = new ImageFileDirectory(binaryReader);
            //    builder.Append(tags.DumpDirectory(binaryReader, prefix + $":0x{entry.TagId:X4}"));
            //}
            else
            {
                switch (entry.TagType)
                {
                    case ImageFileEntry.TagTypes.UByte:
                        builder.Append(string.Format(blockHeader, count, entry.TagId, "UByte 8-bit"));
                        if (entry.NumberOfValue == 1)
                            builder.Append($"{entry.ValuePointer:x2}");
                        else
                        {
                            builder.Append(string.Format(referencedItem, entry.ValuePointer, entry.NumberOfValue));
                            if (binaryReader.BaseStream.Position != entry.ValuePointer)
                                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);

                            for (var j = 0; j < entry.NumberOfValue; j++)
                            {
                                var us = binaryReader.ReadByte();
                                builder.Append($"{us:x2}, ");
                            }
                        }
                        builder.AppendLine();
                        break;

                    case ImageFileEntry.TagTypes.Ascii:
                        builder.Append(string.Format(blockHeader, count, entry.TagId, "Ascii 8-bit, null terminated"));
                        builder.Append(string.Format(referencedItem, entry.ValuePointer, entry.NumberOfValue));

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
                            builder.AppendLine($"\"{str}\"");
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
                            builder.AppendLine($"\"{str}\"");
                        }

                        break;

                    case ImageFileEntry.TagTypes.UShort:
                        builder.Append(string.Format(blockHeader, count, entry.TagId, "UShort 16-bit"));
                        if (entry.NumberOfValue == 1)
                            builder.Append($"{entry.ValuePointer}");
                        else
                        {
                            builder.Append(string.Format(referencedItem, entry.ValuePointer, entry.NumberOfValue));
                            if (binaryReader.BaseStream.Position != entry.ValuePointer)
                                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);

                            for (var j = 0; j < entry.NumberOfValue; j++)
                            {
                                var us = binaryReader.ReadUInt16();
                                builder.Append($"{us}, ");
                            }
                        }
                        builder.AppendLine();
                        break;

                    case ImageFileEntry.TagTypes.ULong:  
                        builder.Append(string.Format(blockHeader, count, entry.TagId, "ULong 32-bit"));
                        if (entry.NumberOfValue == 1)
                            builder.Append($"{entry.ValuePointer}");
                        else
                        {
                            builder.Append(string.Format(referencedItem, entry.ValuePointer, entry.NumberOfValue));
                            if (binaryReader.BaseStream.Position != entry.ValuePointer)
                                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);

                            for (var j = 0; j < entry.NumberOfValue; j++)
                            {
                                var long1 = binaryReader.ReadUInt32();
                                builder.Append($"{long1:X4} ");
                            }
                        }
                        builder.AppendLine();
                        break;

                    case ImageFileEntry.TagTypes.URational:  
                        builder.Append(string.Format(blockHeader, count, entry.TagId, "URational 2x32-bit"));
                        builder.Append(string.Format(referencedItem, entry.ValuePointer, entry.NumberOfValue));
                        if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);

                        var us1 = binaryReader.ReadUInt32();
                        var us2 = binaryReader.ReadUInt32();
                        builder.AppendLine(string.Format(rationalItem, us1, us2, us1 / (double)us2));
                        break;

                    case ImageFileEntry.TagTypes.SByte: 
                        builder.Append(string.Format(blockHeader, count, entry.TagId, "SByte 8-bit"));
                        builder.AppendLine(string.Format(referencedItem, entry.ValuePointer, entry.NumberOfValue));
                        throw new NotImplementedException($"Undefined message {entry.TagType}");

                    case ImageFileEntry.TagTypes.UByteSeq:  
                        builder.Append(string.Format(blockHeader, count, entry.TagId, "UByte[]"));
                        if (entry.NumberOfValue <= 4)
                        {
                            builder.Append($"{entry.ValuePointer >> 0 & 0xFF}, ");
                            builder.Append($"{entry.ValuePointer >> 8 & 0xFF}, ");
                            builder.Append($"{entry.ValuePointer >> 16 & 0xFF}, ");
                            builder.Append($"{entry.ValuePointer >> 24 & 0xFF}");
                        }
                        else
                            builder.Append(string.Format(referencedItem, entry.ValuePointer, entry.NumberOfValue));

                        builder.AppendLine();
                        break;

                    case ImageFileEntry.TagTypes.SShort: 
                        builder.Append(string.Format(blockHeader, count, entry.TagId, "SShort 16-bit"));
                        throw new NotImplementedException($"Undefined message {entry.TagType}");

                    case ImageFileEntry.TagTypes.SLong: 
                        builder.Append(string.Format(blockHeader, count, entry.TagId, "SLong 32-bit"));
                        throw new NotImplementedException($"Undefined message {entry.TagType}");

                    case ImageFileEntry.TagTypes.SRational:  
                        builder.Append(string.Format(blockHeader, count, entry.TagId, "SRational 2x32-bit"));
                        builder.Append(string.Format(referencedItem, entry.ValuePointer, entry.NumberOfValue));
                        if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);

                        var s1 = binaryReader.ReadInt32();
                        var s2 = binaryReader.ReadInt32();
                        builder.AppendLine(string.Format(rationalItem, s1, s2, s1 / (double)s2));
                        break;

                    case ImageFileEntry.TagTypes.Single:  
                        builder.Append(string.Format(blockHeader, count, entry.TagId, "Single 4-Byte"));
                        builder.Append(string.Format(referencedItem, entry.ValuePointer, entry.NumberOfValue));
                        if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);

                        var x1 = binaryReader.ReadSingle();
                        builder.AppendLine($"{x1}");
                        throw new NotImplementedException($"Undefined message {entry.TagType}");

                    case ImageFileEntry.TagTypes.Double:  
                        builder.Append(string.Format(blockHeader, count, entry.TagId, "Double 8-Byte"));
                        if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);

                        var x2 = binaryReader.ReadDouble();
                        builder.AppendLine($"{x2}");
                        throw new NotImplementedException($"Undefined message {entry.TagType}");

                    default:
                        builder.Append(string.Format(blockHeader, count, entry.TagId, "Undefined"));
                        throw new NotImplementedException($"Undefined message {entry.TagType}");
                }
            }
        }

        return builder.ToString();
    }
}
