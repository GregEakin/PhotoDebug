// Project Photo Library 0.1
// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoDebug
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
            // case 0xffc0: // SOF0, Start of Frame 0, Baseline DCT
            //   jh->bits = data[0];
            //   jh->high = data[1] << 8 | data[2];
            //   jh->wide = data[3] << 8 | data[4];
            //   jh->clrs = data[5] + jh->sraw;
            //   if (len == 9 && !dng_version) getc(ifp);
            //   break;
            // case 0xffc3: // SOF3, Start of Frame 3, Lossless (sequential)
            //   jh->sraw = ((data[7] >> 4) * (data[7] & 15) - 1) & 3;
            // case 0xffc4:
            //   if (info_only) break;
            //   for (dp = data; dp < data+len && (c = *dp++) < 4; )
            //     jh->free[c] = jh->huff[c] = make_decoder_ref (&dp);
            //   break;

            if (Mark != 0xFF || (Tag & 0xF0) != 0xC0)
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

            private readonly byte componentId;

            private readonly byte hFactor;

            private readonly byte tableId;

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