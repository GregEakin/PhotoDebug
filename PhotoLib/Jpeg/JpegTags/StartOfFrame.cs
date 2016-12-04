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
        #region Fields

        private readonly Component[] components;

        private readonly ushort length;

        private readonly byte precision;

        private readonly ushort samplesPerLine;

        private readonly ushort scanLines;

        #endregion

        #region Constructors and Destructors

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
            {
                throw new ArgumentException();
            }

            // Console.WriteLine("SoF {0}: {1}", Tag.ToString("X2"), (binaryReader.BaseStream.Position - 2).ToString("X8"));

            length = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            precision = binaryReader.ReadByte();    // bits
            scanLines = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());   // high
            samplesPerLine = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());  // wide

            var componentCount = binaryReader.ReadByte();
            components = new Component[componentCount];
            for (var i = 0; i < componentCount; i++)
            {
                components[i] = new Component(binaryReader);
            }

            // var clrs = components.Sum(comp => comp.HFactor * comp.VFactor);

            if (3 * componentCount + 8 != length)
            {
                throw new ArgumentException();
            }
        }

        #endregion

        #region Public Properties

        public Component[] Components
        {
            get
            {
                return components;
            }
        }

        public ushort Length
        {
            get
            {
                return length;
            }
        }

        public byte Precision
        {
            get
            {
                return precision;
            }
        }

        public ushort SamplesPerLine
        {
            get
            {
                return samplesPerLine;
            }
        }

        public ushort ScanLines
        {
            get
            {
                return scanLines;
            }
        }

        public int Width
        {
            get
            {
                return samplesPerLine * components.Length;
            }
        }

        #endregion

        public struct Component
        {
            #region Fields

            // 0, 1, 2 for the YCbCr
            private readonly byte componentId;

            // 1 for the colour components, 1 or 2 for the Y component
            private readonly byte hFactor;

            // 1 for the colour components, 1 or 2 for the Y component
            private readonly byte tableId;

            // 0 for the Y component and 1 for the colour components
            private readonly byte vFactor;

            #endregion

            #region Constructors and Destructors

            public Component(byte componentId, byte tableId, byte hFactor, byte vFactor)
            {
                this.componentId = componentId;
                this.tableId = tableId;
                this.hFactor = hFactor;
                this.vFactor = vFactor;
            }

            public Component(BinaryReader binaryReader)
            {
                componentId = binaryReader.ReadByte();
                var sampleFactors = binaryReader.ReadByte();
                tableId = binaryReader.ReadByte();
                hFactor = (byte)(sampleFactors >> 4);
                vFactor = (byte)(sampleFactors & 0x0f);
            }

            #endregion

            #region Public Properties

            public byte ComponentId
            {
                get
                {
                    return componentId;
                }
            }

            public byte HFactor
            {
                get
                {
                    return hFactor;
                }
            }

            public byte TableId
            {
                get
                {
                    return tableId;
                }
            }

            public byte VFactor
            {
                get
                {
                    return vFactor;
                }
            }

            #endregion
        }
    }
}