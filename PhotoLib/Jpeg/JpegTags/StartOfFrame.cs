// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		StartOfFrame.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;

namespace PhotoLib.Jpeg
{
    public class StartOfFrame : JpegTag
    {
        public StartOfFrame(BinaryReader binaryReader)
            : base(binaryReader)
        {
            // Nondifferential Huffman-coding frames:
            // case 0xFFC0: // Baseline DCT
            // case 0xFFC1: // Extended sequential DCT
            // case 0xFFC2: // Progressive DCT
            // case 0xFFC3: // Lossless (sequential)
            //   break;

            // Differential Huffman-coding frames:
            // case 0xFFC5: // Differential sequential DCT
            // case 0xFFC6: // Differential progressive DCT
            // case 0xFFC7: // Differential lossless
            //   break;

            // Nondifferential arithmetic-coded frames:
            // case 0xFFC9: // Extended sequential DCT
            // case 0xFFCA: // Progressive DCT
            // case 0xFFCB: // Lossless (sequential)
            //   break;

            // Differential arithmetic-coded frames:
            // case 0xFFCD: // Differential sequential DCT
            // case 0xFFCE: // Differential progressive DCT
            // case 0xFFCF: // Differential lossless
            //   break;

            if (Mark != 0xFF || (Tag & 0xF0) != 0xC0 || Tag == 0xC4 || Tag == 0xC8 || Tag == 0xCC)
                throw new ArgumentException();

            // Console.WriteLine("SoF {0}: {1}", Tag.ToString("X2"), (binaryReader.BaseStream.Position - 2).ToString("X8"));

            Length = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            Precision = binaryReader.ReadByte();    // bits
            ScanLines = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());   // high
            SamplesPerLine = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());  // wide

            var componentCount = binaryReader.ReadByte();
            Components = new Component[componentCount];
            for (var i = 0; i < componentCount; i++)
                Components[i] = new Component(binaryReader);

            // var clrs = components.Sum(comp => comp.HFactor * comp.VFactor);

            if (3 * componentCount + 8 != Length)
                throw new ArgumentException();
        }

        public Component[] Components { get; }

        public ushort Length { get; }

        public byte Precision { get; }

        public ushort SamplesPerLine { get; }

        public ushort ScanLines { get; }

        public int Width => SamplesPerLine * Components.Length;

        public struct Component
        {
            // 0, 1, 2 for the YCbCr
            // 1 for the colour components, 1 or 2 for the Y component
            // 1 for the colour components, 1 or 2 for the Y component
            // 0 for the Y component and 1 for the colour components

            public Component(byte componentId, byte tableId, byte hFactor, byte vFactor)
            {
                ComponentId = componentId;
                TableId = tableId;
                HFactor = hFactor;
                VFactor = vFactor;
            }

            public Component(BinaryReader binaryReader)
            {
                ComponentId = binaryReader.ReadByte();
                var sampleFactors = binaryReader.ReadByte();
                TableId = binaryReader.ReadByte();
                HFactor = (byte)(sampleFactors >> 4);
                VFactor = (byte)(sampleFactors & 0x0f);
            }

            public byte ComponentId { get; }

            public byte HFactor { get; }

            public byte TableId { get; }

            public byte VFactor { get; }
        }
    }
}