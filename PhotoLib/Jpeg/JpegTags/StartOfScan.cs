// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		StartOfScan.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;

namespace PhotoLib.Jpeg.JpegTags
{
    /// <summary>
    /// SOS 0xFFDA
    /// </summary>
    public class StartOfScan : JpegTag
    {
        public StartOfScan(BinaryReader binaryReader)
            : base(binaryReader)
        {
            if (Mark != 0xFF || Tag != 0xDA)
            {
                throw new ArgumentException();
            }

            Length = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());

            var count = binaryReader.ReadByte();
            // count >= 1 && count <= 4
            Components = new ScanComponent[count];
            for (var i = 0; i < count; i++)
            {
                Components[i] = new ScanComponent(binaryReader);
            }
            Bb1 = binaryReader.ReadByte(); // startSpectralSelection    // predictor selection value
            Bb2 = binaryReader.ReadByte(); // endSpectralSelection      // Se : End of spectral selection
            Bb3 = binaryReader.ReadByte(); // successiveApproximation   // Ah : 4bit, Successive approximation bit position high
                                                                        // Al : 4bit, Successive approximation bit position low
                                                                        //            or point transform

            // var psv = bB1;
            // var bits -= (bB3 & 0x04);

            if (2 * count + 6 != Length)
            {
                throw new ArgumentException();
            }
        }

        public ScanComponent[] Components { get; }

        public ushort Length { get; }

        public byte Bb1 { get; }

        public byte Bb2 { get; }

        public byte Bb3 { get; }

        public struct ScanComponent
        {
            public ScanComponent(BinaryReader binaryReader)
            {
                // component id (1 = Y, 2 = Cb, 3 = Cr, 4 = I, 5 = Q)
                Id = binaryReader.ReadByte();
                var info = binaryReader.ReadByte();
                Dc = (byte)((info >> 4) & 0x0f);
                Ac = (byte)(info & 0x0f);
            }

            public byte Ac { get; }

            public byte Dc { get; }

            public byte Id { get; }
        }
    }
}