// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		ImageFileEntry.cs
// AUTHOR:		Greg Eakin

using System.IO;

namespace PhotoLib.Tiff;

public class ImageFileEntry
{
    public enum TagTypes
    {
        Undefined = 0x00,
        UByte     = 0x01,  // 8-bit unsigned integer
        Ascii     = 0x02,  // 8-bit, NULL-terminated string
        UShort    = 0x03,  // 16-bit unsigned integer
        ULong     = 0x04,  // 32-bit unsigned integer
        URational = 0x05,  // Two 32-bit unsigned integers, numerator and denominator
        SByte     = 0x06,  // 8-bit signed integer
        UByteSeq  = 0x07,  // 8-bit byte
        SShort    = 0x08,  // 16-bit signed integer
        SLong     = 0x09,  // 32-bit signed integer
        SRational = 0x0A,  // Two 32-bit signed integers
        Single    = 0x0B,  // 4-byte single-precision IEEE floating-point value
        Double    = 0x0C,  // 8-byte double-precision IEEE floating-point value
    }

    public ImageFileEntry(BinaryReader binaryReader)
    {
        TagId = binaryReader.ReadUInt16();
        TagType = (TagTypes)binaryReader.ReadUInt16();
        NumberOfValue = binaryReader.ReadUInt32();
        ValuePointer = binaryReader.ReadUInt32();
    }

    public ushort TagId { get; }

    public TagTypes TagType { get; }

    public uint NumberOfValue { get; }

    public uint ValuePointer { get; }
}

public class ImageFileTagParser
{
    // name
    // groups
    // notes
    // writable
    // toString()
    // Flags
    // SubDirectory
    // Count
}

public class Tag0x8822 : ImageFileTagParser
{
    // name = "ExposureProgram"
    // groups = 2 => camera
    // notes = "the value of 9 is not standard EXIF, but it is used by the Canon EOS 7D"
    // writable = int16u
    public override string ToString()
    {
        // 0 => 'Not Defined',
        // 1 => 'Manual',
        // 2 => 'Program AE',
        // 3 => 'Aperture-priority AE',
        // 4 => 'Shutter speed priority AE',
        // 5 => 'Creative (Slow speed)',
        // 6 => 'Action (High speed)',
        // 7 => 'Portrait',
        // 8 => 'Landscape',
        // 9 => 'Bulb', #25

        return base.ToString();
    }
}
