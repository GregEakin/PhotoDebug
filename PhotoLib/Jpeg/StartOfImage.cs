namespace PhotoLib.Jpeg
{
    using System;
    using System.IO;

    using PhotoLib.Utilities;

    public class StartOfImage
    {
        #region Fields

        private readonly DefineHuffmanTable huffmanTable;

        private readonly ImageData imageData;

        private readonly StartOfFrame lossless;

        private readonly byte mark;

        private readonly StartOfScan startOfScan;

        private readonly JfifMarker jfifMarker;

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

            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
            {
                var pos = binaryReader.BaseStream.Position;
                var nextMark = binaryReader.ReadByte();
                switch (nextMark)
                {
                    case 0xFE:
                        {
                            var nextTag = binaryReader.ReadByte();
                            binaryReader.BaseStream.Seek(pos, SeekOrigin.Begin);
                            switch (nextTag)
                            {
                                case 0xD5:
                                    this.imageData = new ImageData(binaryReader, address, length);
                                    break;

                                default:
                                    throw new NotImplementedException("Tag 0xFE 0x{0} is not implemented".FormatWith(nextTag.ToString("X2")));
                            }
                        }
                        break;

                    case 0xFF:
                        {
                            var nextTag = binaryReader.ReadByte();
                            binaryReader.BaseStream.Seek(pos, SeekOrigin.Begin);
                            switch (nextTag)
                            {
                                case 0xC0:
                                case 0xC3:
                                    this.lossless = new StartOfFrame(binaryReader);
                                    break;

                                case 0xC4:
                                    this.huffmanTable = new DefineHuffmanTable(binaryReader);
                                    break;

                                case 0xD9:
                                    var x3 = binaryReader.ReadByte();
                                    var x4 = binaryReader.ReadByte();
                                    break;

                                case 0xDA:
                                    this.startOfScan = new StartOfScan(binaryReader);
                                    // this.imageData = binaryReader.ReadBytes((int)(binaryReader.BaseStream.Length - 2));
                                    break;

                                case 0xDB:
                                    var defineQuantizatonTable = new DefineQuantizationTable(binaryReader);
                                    break;

                                case 0xE0:
                                    this.jfifMarker = new JfifMarker(binaryReader);
                                    break;

                                case 0xE1:
                                case 0xE4:
                                case 0xEC:
                                case 0xEE:
                                    var x1 = binaryReader.ReadByte();
                                    var x2 = binaryReader.ReadByte();
                                    var length1 = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());
                                    var data = binaryReader.ReadBytes(length1 - 2);
                                    break;

                                default:
                                    throw new NotImplementedException("Tag 0xFF 0x{0} is not implemented".FormatWith(nextTag.ToString("X2")));
                            }
                        }
                        break;

                    default:
                        throw new NotImplementedException("Tag 0x{0} is not implemented".FormatWith(nextMark.ToString("X2")));
                }
            }
        }

        #endregion

        #region Public Properties

        public DefineHuffmanTable HuffmanTable
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

        public StartOfFrame Lossless
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