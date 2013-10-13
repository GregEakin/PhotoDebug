namespace PhotoLib.Jpeg
{
    using System;
    using System.IO;

    using PhotoLib.Utilities;

    public class ImageData
    {
        #region Fields

        private readonly byte[] rawData;

        private byte currentByte;

        private int index = -1;

        private int nextBit = -1;

        #endregion

        #region Constructors and Destructors

        public ImageData(BinaryReader binaryReader, uint rawSize)
        {
            rawData = binaryReader.ReadBytes((int)rawSize);
            this.CheckByte();
        }

        #endregion

        #region Public Properties

        public int BitsLeft
        {
            get
            {
                return nextBit;
            }
        }

        public bool EndOfFile { get; private set; }

        public int Index
        {
            get
            {
                //Console.WriteLine("Lenght = 0x{0}", rawData.Length.ToString("X8"));
                //Console.WriteLine("Index = 0x{0}, nextBit = {1}, currentByte = 0x{2}", index.ToString("X8"), nextBit, currentByte.ToString("X2"));
                //Console.WriteLine("Diff = {0}", rawData.Length - index);
                ////Console.WriteLine("Val = 0x{0}", this.GetNextByte());
                ////Console.WriteLine("Val = 0x{0}", this.GetNextByte());

                //for (var i = rawData.Length - 16; i < rawData.Length; i++)
                //    Console.Write("{0} ", rawData[i].ToString("X2"));
                //Console.WriteLine();
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
            var bit = (this.currentByte & (0x01 << this.nextBit)) != 0;
            nextBit--;
            this.CheckByte();
            return bit;
        }

        public ushort GetNextShort(ushort lastShort)
        {
            var retval = lastShort << 1 | (this.GetNextBit() ? 0x01 : 0x00);
            return (ushort)retval;
        }

        private byte GetNextByte()
        {
            byte retval;

            if (this.EndOfFile)
            {
                throw new Exception("Reading past EOF is bad!");
            }

            if (index < rawData.Length - 1)
            {
                retval = rawData[++index];
                if (retval == 0xFF)
                {
                    var code = rawData[++index];
                    if (code == 0xD9)
                    {
                        this.EndOfFile = true;
                    }
                    else if (code != 0)
                    {
                        throw new Exception(
                            "Not supposed to happen 0xFF 0x{0}: Position: {1}".FormatWith(code.ToString("X2"), (this.rawData.Length - this.index)));
                    }
                }
            }
            else
            {
                index++;
                this.EndOfFile = true;
                retval = 0xFF;
            }

            return retval;
        }

        public ushort GetSetOfBits(ushort total)
        {
            var retval = (ushort)0u;

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
                this.currentByte = this.GetNextByte();
            }
        }

        #endregion
    }
}