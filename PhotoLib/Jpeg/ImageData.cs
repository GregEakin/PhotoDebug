// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoLib
// FILE:		ImageData.cs
// AUTHOR:		Greg Eakin

using System;
using System.IO;

namespace PhotoLib.Jpeg
{
    public class ImageData
    {
        private byte _currentByte;

        public ImageData(BinaryReader binaryReader, uint rawSize)
        {
            RawData = binaryReader.ReadBytes((int)rawSize);
            CheckByte();
        }

        public int BitsLeft { get; private set; } = -1;

        public int DistFromEnd => Index < 0 ? -1 : RawData.Length - Index;

        public bool EndOfFile { get; private set; }

        public int Index { get; private set; } = -1;

        public byte[] RawData { get; }

        private void CheckByte()
        {
            if (BitsLeft >= 0)
                return;
            BitsLeft = 7;
            _currentByte = GetNextByte();
        }

        public bool GetNextBit()
        {
            var nextBit = (_currentByte & (0x01 << BitsLeft)) != 0;
            BitsLeft--;
            CheckByte();
            return nextBit;
        }

        private byte GetNextByte()
        {
            if (EndOfFile)
                throw new Exception("Reading past EOF is bad!");

            byte nextByte;

            if (Index < RawData.Length - 1)
            {
                nextByte = RawData[++Index];
                if (nextByte != 0xFF)
                    return nextByte;

                var code = RawData[++Index];
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
                        throw new Exception($"Not supposed to happen 0xFF 0x{code:X2}: Position: {RawData.Length - Index}");
                }
            }
            else
            {
                Index++;
                EndOfFile = true;
                nextByte = 0xFF;

                Console.WriteLine("Read to EOF");
            }

            return nextByte;
        }

        public ushort GetNextShort(ushort lastShort)
        {
            var bit = GetNextBit() ? 0x01 : 0x00;
            var nextShort = (lastShort << 1) | bit;
            return (ushort)nextShort;
        }

        public ushort GetSetOfBits(ushort total)
        {
            var setOfBits = (ushort)0u;

            var length = (ushort)Math.Min(total, BitsLeft + 1);
            while (length > 0)
            {
                var shift = BitsLeft + 1 - length;
                var mask = (0x0001 << length) - 1;
                var next = _currentByte >> shift;
                setOfBits <<= length;
                setOfBits |= (ushort)(next & mask);

                BitsLeft -= length;
                CheckByte();
                total -= length;
                length = (ushort)Math.Min(total, BitsLeft + 1);
            }

            return setOfBits;
        }

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
            while (!table.Dictionary.TryGetValue(hufIndex, out hCode) || (hCode.Length != hufBits));

            return hCode.Code;
        }

        public ushort GetValue(int bits)
        {
            var hufIndex = (ushort)0;
            for (var i = 0; i < bits; i++)
                hufIndex = GetNextShort(hufIndex);

            return hufIndex;
        }

        public void Reset()
        {
            EndOfFile = false;
            _currentByte = 0xFF;
            Index = -1;
            BitsLeft = -1;
            CheckByte();
        }
    }
}
