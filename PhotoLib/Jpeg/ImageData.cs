// Project Photo Library 0.1
// Copyright © 2013-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		ImageData.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;

using PhotoLib.Utilities;

namespace PhotoLib.Jpeg
{
    public class ImageData
    {
        public void Reset()
        {
            EndOfFile = false;
            currentByte = 0xFF;
            index = -1;
            nextBit = -1;
            CheckByte();
        }

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
            CheckByte();
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
                //Console.WriteLine("Length = 0x{0}", rawData.Length.ToString("X8"));
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
            var bit = (currentByte & (0x01 << nextBit)) != 0;
            nextBit--;
            CheckByte();
            return bit;
        }

        public ushort GetNextShort(ushort lastShort)
        {
            var bit = GetNextBit() ? 0x01 : 0x00;
            var retval = (lastShort << 1) | bit;
            return (ushort)retval;
        }

        private byte GetNextByte()
        {
            byte retval;

            if (EndOfFile)
            {
                throw new Exception("Reading past EOF is bad!");
            }

            if (index < rawData.Length - 1)
            {
                retval = rawData[++index];
                if (retval != 0xFF)
                {
                    return retval;
                }

                var code = rawData[++index];
                switch (code)
                {
                    case 0x00:
                    case 0xFF:
                        break;

                    case 0xD9:
                        EndOfFile = true;
                        Console.WriteLine("Found 0xD9 EOI marker");
                        break;

                    default:
                        throw new Exception(
                            "Not supposed to happen 0xFF 0x{0}: Position: {1}".FormatWith(code.ToString("X2"), (rawData.Length - index)));
                }
            }
            else
            {
                index++;
                EndOfFile = true;
                retval = 0xFF;

                Console.WriteLine("Read to EOF");
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
                CheckByte();
                total -= length;
                length = (ushort)Math.Min(total, nextBit + 1);
            }

            return retval;
        }

        #endregion

        #region Methods

        private void CheckByte()
        {
            if (nextBit < 0)
            {
                nextBit = 7;
                currentByte = GetNextByte();
            }
        }

        #endregion

        public byte GetValue(HuffmanTable table)
        {
            var hufIndex = (ushort)0;
            var hufBits = (ushort)0;
            HuffmanTable.HCode hCode;
            do
            {
                hufIndex = GetNextShort(hufIndex);
                hufBits++;
            }
            while (!table.Dictionary.TryGetValue(hufIndex, out hCode) || hCode.Length != hufBits);

            return hCode.Code;
        }

        public ushort GetValue(int bits)
        {
            var hufIndex = (ushort)0;
            for (var i = 0; i < bits; i++)
                hufIndex = GetNextShort(hufIndex);

            return hufIndex;
        }

        public int DistFromEnd
        {
            get
            {
                if (index < 0)
                    return -1;
                return rawData.Length - index;
            }
        }
    }
}