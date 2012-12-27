namespace PhotoLib.Tiff
{
    using System.IO;

    public class CR2Header
    {
        #region Fields

        private readonly byte[] byteOrder;

        private readonly ushort cr2Magic;

        private readonly byte[] cr2Version;

        private readonly uint rawIfdOffset;

        private readonly ushort tiffMagic;

        private readonly uint tiffOffset;

        #endregion

        #region Constructors and Destructors

        public CR2Header(BinaryReader binaryReader)
        {
            byteOrder = binaryReader.ReadBytes(2);
            tiffMagic = binaryReader.ReadUInt16();
            tiffOffset = binaryReader.ReadUInt32();
            cr2Magic = binaryReader.ReadUInt16();
            cr2Version = binaryReader.ReadBytes(2);
            rawIfdOffset = binaryReader.ReadUInt32();
        }

        #endregion

        #region Public Properties

        public byte[] ByteOrder
        {
            get
            {
                return this.byteOrder;
            }
        }

        public ushort CR2Magic
        {
            get
            {
                return this.cr2Magic;
            }
        }

        public byte[] CR2Version
        {
            get
            {
                return this.cr2Version;
            }
        }

        public uint RawIfdOffset
        {
            get
            {
                return this.rawIfdOffset;
            }
        }

        public ushort TiffMagic
        {
            get
            {
                return this.tiffMagic;
            }
        }

        public uint TiffOffset
        {
            get
            {
                return this.tiffOffset;
            }
        }

        #endregion
    }
}