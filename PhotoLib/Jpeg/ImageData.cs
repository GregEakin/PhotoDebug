namespace PhotoLib.Jpeg
{
    using System;
    using System.IO;

    public class ImageData
    {
        #region Fields

        private readonly byte mark;

        private readonly byte[] rawData;

        private readonly byte tag;

        #endregion

        #region Constructors and Destructors

        public ImageData(BinaryReader binaryReader, uint address, uint length)
        {
            mark = binaryReader.ReadByte();
            tag = binaryReader.ReadByte(); // JPG_MARK_ D5

            if (mark != 0xFF || tag != 0xD5)
            {
                throw new ArgumentException();
            }

            var pos = binaryReader.BaseStream.Position - 2;
            var rawSize = address + length - pos;

            binaryReader.BaseStream.Seek(pos, SeekOrigin.Begin);
            rawData = binaryReader.ReadBytes((int)rawSize);

            // GetBits(rawData);
            // GetLosslessJpgRow(null, rawData, TB0, TL0, TB1, TL1, Prop);

            // for (var iRow = 0; iRow < height; iRow++)
            {
                // var rowBuf = new ushort[width];
                // GetLosslessJpgRow(rowBuf, rawData, TL0, TB0, TL1, TB1, Prop);
                // PutUnscrambleRowSlice(rowBuf, imageData, iRow, Prop);
            }
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

        public byte[] RawData
        {
            get
            {
                return rawData;
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