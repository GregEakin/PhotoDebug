namespace PhotoLib.Jpeg
{
    using System;
    using System.IO;

    public class StartOfScan
    {
        #region Fields

        private readonly byte[] components;

        private readonly short length;

        private readonly byte mark;

        private readonly byte tag;

        #endregion

        #region Constructors and Destructors

        public StartOfScan(BinaryReader binaryReader)
        {
            mark = binaryReader.ReadByte();
            tag = binaryReader.ReadByte(); // JPG_MARK DA

            if (mark != 0xFF && tag != 0xDA)
            {
                throw new ArgumentException();
            }

            length = (short)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());

            var count = binaryReader.ReadByte();
            components = new byte[count];
            for (var i = 0; i < count; i++)
            {
                var id = binaryReader.ReadByte();
                var info = binaryReader.ReadByte();
                var dc = (info >> 4) & 0x0f;
                var ac = info & 0x0f;
                components[i] = id;
                // id, acTables[ac], dcTables[dc]
            }
            var bB1 = binaryReader.ReadByte(); // startSpectralSelection
            var bB2 = binaryReader.ReadByte(); // endSpectralSelection
            var bB3 = binaryReader.ReadByte(); // successiveApproximation

            if (2 * count + 6 != length)
            {
                throw new ArgumentException();
            }
        }

        #endregion

        #region Public Properties

        public byte[] Components
        {
            get
            {
                return components;
            }
        }

        public short Length
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