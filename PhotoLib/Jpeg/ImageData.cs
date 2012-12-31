namespace PhotoLib.Jpeg
{
    using System;
    using System.IO;

    public class ImageData
    {
        #region Fields

        private readonly byte[] rawData;

        #endregion

        #region Constructors and Destructors

        public ImageData(BinaryReader binaryReader, uint rawSize)
        {
            rawData = binaryReader.ReadBytes((int)rawSize);
        }

        private int nextBit = -1;

        private int index = -1;

        private byte currentByte;

        private bool EOF = false;

        public bool GetBit()
        {
            this.CheckByte();
            var bit = ((this.currentByte >> this.nextBit) & 0x01) != 0;
            nextBit--;
            return bit;
        }

        public ushort GetNextBit(ushort lastBit)
        {
            var retval = lastBit << 1 | (this.GetBit() ? 0x01 : 0x00);
            return (ushort)retval;
        }

        public ushort GetNextBits(ushort total)
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

        private void CheckByte()
        {
            if (this.nextBit < 0)
            {
                this.nextBit = 7;
                this.index++;
                this.currentByte = this.GetByte();
            }
        }

        public byte GetByte()
        {
            var retval = (byte)0x00;
            
            if (index < rawData.Length)
            {
                retval = rawData[index];
                if (retval == 0xFF)
                {
                    var code = rawData[++index];
                    if (code != 0)
                    {
                        retval = rawData[++index];
                    }
                }
            }
            else
            {
                EOF = true;
                retval = 0xFF;
            }

            return retval;
        }

        #endregion

        #region Public Properties

        public byte[] RawData
        {
            get
            {
                return rawData;
            }
        }

        #endregion
    }
}