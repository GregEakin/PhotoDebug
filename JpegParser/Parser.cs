// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	JpegParser
// FILE:		Parser.cs
// AUTHOR:		Greg Eakin    

using System;
using System.IO;

namespace JpegParser
{
    public class Parser
    {
        private readonly BinaryReader _binaryReader;

        public Parser(BinaryReader binaryReader)
        {
            _binaryReader = binaryReader;
            // binaryReader.BaseStream.Seek(0x000000D0u, SeekOrigin.Begin);
        }

        public void JpegData()
        {
            ExpectByte(0xD8, FillByff());
            TblsMisc();
            var data = FillByff();
            switch (data)
            {
                case 0xC0:
                case 0xC1:
                case 0xC2:
                case 0xC3:
                case 0xC9:
                case 0xCA:
                case 0xCB:
                    var b = ReadEightBytes();
                    Fspec();
                    ScanSet();
                    break;

                case 0xD9:
                    return;

                case 0xDE:
                    var c = ReadEightBytes();
                    Fspec();
                    TblsMisc();
                    var g = FillByff();
                    // g == { C0, C1, C2, C2, C9, CA, CB }
                    var d = ReadEightBytes();
                    Fspec();
                    ScanSet();
                    XFrames();
                    break;
            }

            ExpectByte(0xD9, FillByff());
        }

        private byte[] ReadEightBytes()
        {
            var data = new byte[8];
            for (var i = 0; i < data.Length; i++)
                data[i] = _binaryReader.ReadByte();
            return data;
        }

        private void Fspec()
        {
            throw new NotImplementedException();
        }

        public void ExpectByte(byte expected, byte actual)
        {
            if (expected == actual)
                Console.WriteLine("0x{0} found", actual);
        }

        public byte FillByff()
        {
            byte next;
            do
                next = _binaryReader.ReadByte();
            while (next == 0xFF);
            return next;
        }

        public void TblsMisc()
        {
            var data = FillByff();
            var length = (ushort)(_binaryReader.ReadByte() << 8 | _binaryReader.ReadByte());

            switch (data)
            {
                case 0xD8:
                    DqtData(length);
                    break;
                case 0xC4:
                    DhtData(length);
                    break;
                case 0xCC:
                    DacData(length);
                    break;
                case 0xDD:
                    var restinv = (ushort)(_binaryReader.ReadByte() << 8 | _binaryReader.ReadByte());
                    break;
                case 0xFE:
                    Appspec(length);
                    break;
                default:
                    if (data >= 0xE0 && data <= 0xEF)
                        Comspec(length);
                    break;
            }
        }

        private void Appspec(ushort length)
        {
            throw new NotImplementedException();
        }

        private void Comspec(ushort length)
        {
            throw new NotImplementedException();
        }

        public void ScanSet()
        {
            throw new NotImplementedException();
        }

        public void XFrames()
        {
            throw new NotImplementedException();
        }

        public bool Forbid()
        {
            var data = FillByff();

            // BY01, BY02 .. BYBF, BYC8, BYF0 .. BYFD
            return data == 0x01
                || (data >= 0x02 && data <= 0xBF)
                || data == 0xC8
                || (data >= 0xF0 && data <= 0xFD);
        }

        public void DqtData(ushort length)
        {
            // one or more of these....
            QtTbl();
            throw new NotImplementedException();
        }

        public void DhtData(ushort length)
        {
            throw new NotImplementedException();
        }

        public void DacData(ushort length)
        {
            throw new NotImplementedException();
        }

        public void EcDataF()
        {
            throw new NotImplementedException();
        }

        public void EcData()
        {
            throw new NotImplementedException();
        }

        public void DTblsMisc()
        {
            throw new NotImplementedException();
        }

        public void QtTbl()
        {
            throw new NotImplementedException();
        }

        public void TabSpec()
        {
            throw new NotImplementedException();
        }

        public void DacCond()
        {
            throw new NotImplementedException();
        }
    }
}
