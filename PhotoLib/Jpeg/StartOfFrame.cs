namespace PhotoLib.Jpeg
{
    using System;
    using System.IO;

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
            if (Mark != 0xFF || (Tag & 0xF0) != 0xC0)
            {
                throw new ArgumentException();
            }

            Console.WriteLine("SoF {0}: {1}", Tag.ToString("X2"), (binaryReader.BaseStream.Position - 2).ToString("X8"));

            length = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            precision = binaryReader.ReadByte();
            scanLines = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            samplesPerLine = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            var componentCount = binaryReader.ReadByte();
            components = new Component[componentCount];

            for (var i = 0; i < componentCount; i++)
            {
                var compId = binaryReader.ReadByte();
                var sampleFactors = binaryReader.ReadByte();
                var qTableId = binaryReader.ReadByte();
                var sampleHFactor = (byte)(sampleFactors >> 4);
                var sampleVFactor = (byte)(sampleFactors & 0x0f);
                components[i] = new Component(compId, qTableId, sampleHFactor, sampleVFactor);
            }

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