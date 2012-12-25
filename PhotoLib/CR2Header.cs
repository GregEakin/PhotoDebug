namespace PhotoLib
{
    using System.IO;

    public class CR2Header
    {
        #region Constructors and Destructors

        public CR2Header()
        {
        }

        public CR2Header(BinaryReader binaryReader)
        {
            this.ByteOrder = binaryReader.ReadBytes(2);
            this.TiffMagic = binaryReader.ReadUInt16();
            this.TiffOffset = binaryReader.ReadUInt32();
            this.CR2Magic = binaryReader.ReadUInt16();
            this.CR2Version = binaryReader.ReadBytes(2);
            this.RawIfdOffset = binaryReader.ReadUInt32();
        }

        #endregion

        #region Public Properties

        public byte[] ByteOrder { get; set; }

        public ushort CR2Magic { get; set; }

        public byte[] CR2Version { get; set; }

        public uint RawIfdOffset { get; set; }

        public ushort TiffMagic { get; set; }

        public uint TiffOffset { get; set; }

        #endregion
    }

    // TODO Validate the header's information
}