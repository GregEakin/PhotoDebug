namespace PhotoLib.Jpeg
{
    using System;
    using System.IO;

    public class DefineQuantizationTable
    {
        #region Fields

        private readonly ushort length;

        private readonly byte mark;

        private readonly byte tag;

        #endregion

        // DHT: Define Huffman Table

        #region Constructors and Destructors

        public DefineQuantizationTable(BinaryReader binaryReader)
        {
            mark = binaryReader.ReadByte();
            tag = binaryReader.ReadByte(); // JPG_MARK_DHT

            if (mark != 0xFF || tag != 0xDB)
            {
                throw new ArgumentException();
            }

            length = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());

            var size = 2;
            while (size < length)
            {
                // until the length is exhausted (loads two quantization tables for baseline JPEG)
                // the precision and the quantization table index -- one byte: precision is specified by the higher four bits and index is specified by the lower four bits
                //   precision in this case is either 0 or 1 and indicates the precision of the quantized values; 8-bit (baseline) for 0 and  up to 16-bit for 1
                // the quantization values -- 64 bytes
                // t  he quantization tables are stored in zigzag format

                var data = binaryReader.ReadBytes(length - 2);
                size += data.Length;
            }

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