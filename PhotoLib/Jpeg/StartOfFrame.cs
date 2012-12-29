namespace PhotoLib.Jpeg
{
    using System;
    using System.IO;

    public class StartOfFrame
    {
        #region Fields

        private readonly Component[] components;

        private readonly short length;

        private readonly byte mark;

        private readonly byte precision;

        private readonly short samplesPerLine;

        private readonly short scanLines;

        private readonly byte tag;

        #endregion

        #region Constructors and Destructors

        public StartOfFrame(BinaryReader binaryReader)
        {
            mark = binaryReader.ReadByte();
            tag = binaryReader.ReadByte(); // JPG_MARK C3

            if (mark != 0xFF || (tag & 0xF0) != 0xC0)
            {
                throw new ArgumentException();
            }

            length = (short)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            precision = binaryReader.ReadByte();
            scanLines = (short)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            samplesPerLine = (short)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
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

        public short Length
        {
            get
            {
                return length;
            }
        }

        public byte Mark
        {
            get
            {
                return mark;
            }
        }

        public byte Precision
        {
            get
            {
                return precision;
            }
        }

        public short SamplesPerLine
        {
            get
            {
                return samplesPerLine;
            }
        }

        public short ScanLines
        {
            get
            {
                return scanLines;
            }
        }

        public byte Tag
        {
            get
            {
                return tag;
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