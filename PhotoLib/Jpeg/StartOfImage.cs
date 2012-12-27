namespace PhotoLib.Jpeg
{
    using System;
    using System.IO;

    public class StartOfImage
    {
        #region Fields

        private readonly HuffmanTable huffmanTable;

        private readonly ImageData imageData;

        private readonly Lossless lossless;

        private readonly byte mark;

        private readonly StartOfScan startOfScan;

        private readonly byte tag;

        #endregion

        #region Constructors and Destructors

        public StartOfImage(BinaryReader binaryReader, uint address, uint length)
        {
            binaryReader.BaseStream.Seek(address, SeekOrigin.Begin);

            mark = binaryReader.ReadByte();
            tag = binaryReader.ReadByte(); // JPG_MARK_SOI

            if (mark != 0xFF || tag != 0xD8)
            {
                throw new ArgumentException();
            }

            huffmanTable = new HuffmanTable(binaryReader);

            lossless = new Lossless(binaryReader);

            startOfScan = new StartOfScan(binaryReader);

            imageData = new ImageData(binaryReader, address, length);
        }

        #endregion

        #region Public Properties

        public HuffmanTable HuffmanTable
        {
            get
            {
                return huffmanTable;
            }
        }

        public ImageData ImageData
        {
            get
            {
                return imageData;
            }
        }

        public Lossless Lossless
        {
            get
            {
                return lossless;
            }
        }

        public byte Mark
        {
            get
            {
                return mark;
            }
        }

        public StartOfScan StartOfScan
        {
            get
            {
                return startOfScan;
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