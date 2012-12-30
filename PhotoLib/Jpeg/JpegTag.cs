namespace PhotoLib.Jpeg
{
    using System.IO;

    public abstract class JpegTag
    {
        #region Fields

        private readonly byte mark;

        private readonly byte tag;

        #endregion

        #region Constructors and Destructors

        protected JpegTag(BinaryReader binaryReader)
        {
            mark = binaryReader.ReadByte();
            tag = binaryReader.ReadByte();
        }

        #endregion

        #region Public Properties

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