namespace PhotoLib.Jpeg
{
    using System;
    using System.IO;

    public class JfifMarker
    {
        #region Fields

        private readonly ushort length;

        private readonly byte mark;

        private readonly byte tag;

        #endregion

        #region Constructors and Destructors

        public JfifMarker(BinaryReader binaryReader)
        {
            mark = binaryReader.ReadByte();
            tag = binaryReader.ReadByte(); // JFIF Marker

            if (mark != 0xFF || tag != 0xE0)
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

        #endregion
    }
}