namespace PhotoLib.Jpeg
{
    using System;
    using System.IO;

    public class StartOfScan
    {
        #region Fields

        private readonly ScanComponent[] components;

        private readonly short length;

        private readonly byte mark;

        private readonly byte tag;

        #endregion

        #region Constructors and Destructors

        public StartOfScan(BinaryReader binaryReader)
        {
            mark = binaryReader.ReadByte();
            tag = binaryReader.ReadByte(); // JPG_MARK DA

            if (mark != 0xFF || tag != 0xDA)
            {
                throw new ArgumentException();
            }

            length = (short)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());

            var count = binaryReader.ReadByte();
            // count >= 1 && count <= 4
            components = new ScanComponent[count];
            for (var i = 0; i < count; i++)
            {
                components[i] = new ScanComponent(binaryReader);
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

        public ScanComponent[] Components
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

        public struct ScanComponent
        {
            #region Fields

            private readonly byte ac;

            private readonly byte dc;

            private readonly byte id;

            #endregion

            #region Constructors and Destructors

            public ScanComponent(BinaryReader binaryReader)
            {
                // component id (1 = Y, 2 = Cb, 3 = Cr, 4 = I, 5 = Q)
                id = binaryReader.ReadByte();
                var info = binaryReader.ReadByte();
                dc = (byte)((info >> 4) & 0x0f);
                ac = (byte)(info & 0x0f);
            }

            #endregion

            #region Public Properties

            public byte Ac
            {
                get
                {
                    return ac;
                }
            }

            public byte Dc
            {
                get
                {
                    return dc;
                }
            }

            public byte Id
            {
                get
                {
                    return id;
                }
            }

            #endregion
        }
    }
}