namespace PhotoLib.Tiff
{
    using System.IO;

    public class ImageFileEntry
    {
        #region Fields

        private readonly uint numberOfValue;

        private readonly ushort tagId;

        private readonly ushort tagType;

        private readonly uint valuePointer;

        #endregion

        #region Constructors and Destructors

        public ImageFileEntry(BinaryReader binaryReader)
        {
            tagId = binaryReader.ReadUInt16();
            tagType = binaryReader.ReadUInt16();
            numberOfValue = binaryReader.ReadUInt32();
            valuePointer = binaryReader.ReadUInt32();
        }

        #endregion

        #region Public Properties

        public uint NumberOfValue
        {
            get
            {
                return this.numberOfValue;
            }
        }

        public ushort TagId
        {
            get
            {
                return this.tagId;
            }
        }

        public ushort TagType
        {
            get
            {
                return this.tagType;
            }
        }

        public uint ValuePointer
        {
            get
            {
                return this.valuePointer;
            }
        }

        #endregion
    }
}