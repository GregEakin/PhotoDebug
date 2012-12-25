namespace PhotoLib
{
    using System.IO;

    public class ImageFileEntry
    {
        #region Constructors and Destructors

        public ImageFileEntry()
        {
        }

        public ImageFileEntry(BinaryReader binaryReader)
        {
            this.TagId = binaryReader.ReadUInt16();
            this.TagType = binaryReader.ReadUInt16();
            this.NumberOfValue = binaryReader.ReadUInt32();
            this.ValuePointer = binaryReader.ReadUInt32();
        }

        #endregion

        #region Public Properties

        public uint NumberOfValue { get; set; }

        public ushort TagId { get; set; }

        public ushort TagType { get; set; }

        public uint ValuePointer { get; set; }

        #endregion
    }
}