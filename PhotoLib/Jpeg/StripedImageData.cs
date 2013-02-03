namespace PhotoLib.Jpeg
{
    using System;
    using System.IO;

    public class StripedImageData : IImageData
    {
        #region Fields

        private readonly int height;

        private readonly byte[] rawData;

        private readonly int width;

        private byte currentByte;

        private int index = -1;

        private int nextBit = -1;

        private int x;

        private int y;

        private int z;

        #endregion

        #region Constructors and Destructors

        public StripedImageData(BinaryReader binaryReader, uint rawSize, int width, int height, int x, int y, int z)
        {
            rawData = binaryReader.ReadBytes((int)rawSize);
            this.width = width;
            this.height = height;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        #endregion

        #region Public Properties

        public bool EndOfFile { get; private set; }

        public int Index
        {
            get
            {
                return index;
            }
        }

        public byte[] RawData
        {
            get
            {
                return rawData;
            }
        }

        #endregion

        #region Public Methods and Operators

        public bool GetNextBit()
        {
            this.CheckByte();
            var bit = ((this.currentByte >> this.nextBit) & 0x01) != 0;
            nextBit--;
            return bit;
        }

        public ushort GetNextBit(ushort lastBit)
        {
            var retval = lastBit << 1 | (this.GetNextBit() ? 0x01 : 0x00);
            return (ushort)retval;
        }

        public byte GetNextByte()
        {
            byte retval;
            if (index < rawData.Length)
            {
                var position = index;
                var y = index % width;

                retval = rawData[position];
                if (retval == 0xFF)
                {
                    position = ++index;
                    var code = rawData[position];
                    if (code != 0)
                    {
                        position = ++index;
                        retval = rawData[position];
                    }
                }
            }
            else
            {
                this.EndOfFile = true;
                retval = 0xFF;
            }
            return retval;
        }

        public ushort GetSetOfBits(ushort total)
        {
            var retval = (ushort)0u;
            this.CheckByte();

            var length = (ushort)Math.Min(total, nextBit + 1);
            while (length > 0)
            {
                var shift = nextBit + 1 - length;
                var mask = (0x0001 << length) - 1;
                var next = currentByte >> shift;
                retval <<= length;
                retval |= (ushort)(next & mask);

                nextBit -= length;
                this.CheckByte();
                total -= length;
                length = (ushort)Math.Min(total, nextBit + 1);
            }

            return retval;
        }

        #endregion

        #region Methods

        private void CheckByte()
        {
            if (this.nextBit < 0)
            {
                this.nextBit = 7;
                this.index++;
                this.currentByte = this.GetNextByte();
            }
        }

        #endregion
    }
}