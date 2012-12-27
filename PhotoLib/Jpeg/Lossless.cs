namespace PhotoLib.Jpeg
{
    using System;
    using System.IO;

    public class Lossless
    {
        #region Fields

        private readonly byte componentCount;

        private readonly short length;

        private readonly byte mark;

        private readonly byte precision;

        private readonly short samplesPerLine;

        private readonly short scanLines;

        private readonly byte tag;

        #endregion

        #region Constructors and Destructors

        public Lossless(BinaryReader binaryReader)
        {
            mark = binaryReader.ReadByte();
            tag = binaryReader.ReadByte(); // JPG_MARK C3

            if (mark != 0xFF || tag != 0xC3)
            {
                throw new ArgumentException();
            }

            length = (short)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            precision = binaryReader.ReadByte();
            scanLines = (short)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            samplesPerLine = (short)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            componentCount = binaryReader.ReadByte();

            //var imageSize = width * scanLines;
            //Assert.AreEqual(18845760, imageSize);

            for (var i = 0; i < componentCount; i++)
            {
                var compId = binaryReader.ReadByte();
                var sampleFactors = binaryReader.ReadByte();
                var qTableId = binaryReader.ReadByte();

                // var sampleHFactor = (byte)(sampleFactors >> 4);
                // var sampleVFactor = (byte)(sampleFactors & 0x0f);
                // frame.AddComponent(compId, sampleHFactor, sampleVFactor, qTableId);
            }

            if (3 * componentCount + 8 != length)
            {
                throw new ArgumentException();
            }
        }

        #endregion

        #region Public Properties

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
                return samplesPerLine * componentCount;
            }
        }

        #endregion
    }
}