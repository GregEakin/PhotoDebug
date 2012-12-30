namespace PhotoLib.Jpeg
{
    using System;
    using System.IO;

    public class JfifMarker : JpegTag
    {
        #region Fields

        private readonly ushort length;

        #endregion

        #region Constructors and Destructors

        public JfifMarker(BinaryReader binaryReader)
            : base(binaryReader)
        {
            if (Mark != 0xFF || Tag != 0xE0)
            {
                throw new ArgumentException();
            }

            length = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());

            var indentifer = binaryReader.ReadBytes(5);
            // TODO see if identifier == "JFIF";

            var version = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            var units = binaryReader.ReadByte();
            var xDensity = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            var yDensity = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
            var xThumb = binaryReader.ReadByte();
            var yThumb = binaryReader.ReadByte();

            var thumbLen = 3 * xThumb * yThumb;
            var thumb = binaryReader.ReadBytes(thumbLen);

            var size = 16 + thumbLen;

            if (size != length)
            {
                throw new ArgumentException();
            }
        }

        #endregion

        #region Public Properties

        public ushort Length
        {
            get
            {
                return length;
            }
        }

        #endregion
    }
}