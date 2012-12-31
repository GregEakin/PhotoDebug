namespace PhotoLib.Jpeg
{
    using System;
    using System.IO;

    public class StartOfScan : JpegTag
    {
        #region Fields

        private readonly ScanComponent[] components;

        private readonly ushort length;

        #endregion

        #region Constructors and Destructors

        public StartOfScan(BinaryReader binaryReader)
            : base(binaryReader)
        {
            if (Mark != 0xFF || Tag != 0xDA)
            {
                throw new ArgumentException();
            }

            length = (ushort)(binaryReader.ReadByte() << 8 | binaryReader.ReadByte());

            var count = binaryReader.ReadByte();
            // count >= 1 && count <= 4
            components = new ScanComponent[count];
            for (var i = 0; i < count; i++)
            {
                components[i] = new ScanComponent(binaryReader);
            }
            var bB1 = binaryReader.ReadByte(); // startSpectralSelection    // predictor selection value
            var bB2 = binaryReader.ReadByte(); // endSpectralSelection      // Se : End of spectral selection
            var bB3 = binaryReader.ReadByte(); // successiveApproximation   // Ah : 4bit, Successive approximation bit position high
                                                                            // Al : 4bit, Successive approximation bit position low
                                                                            //            or poin transform

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

        public ushort Length
        {
            get
            {
                return length;
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