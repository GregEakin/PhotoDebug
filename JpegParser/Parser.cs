using System;
using System.IO;

namespace JpegParser
{
    public class Parser
    {
        private readonly BinaryReader binaryReader;

        public Parser(BinaryReader binaryReader)
        {
            this.binaryReader = binaryReader;
            // binaryReader.BaseStream.Seek(0x000000D0u, SeekOrigin.Begin);
        }

        public void JpegData()
        {
            ExpectByte(0xD8, FillBYFF());
            TblsMisc();
            var data = FillBYFF();
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
                    fspec();
                    ScanSet();
                    break;

                case 0xD9:
                    return;

                case 0xDE:
                    var c = ReadEightBytes();
                    fspec();
                    TblsMisc();
                    var g = FillBYFF();
                    // g == { C0, C1, C2, C2, C9, CA, CB }
                    var d = ReadEightBytes();
                    fspec();
                    ScanSet();
                    XFrames();
                    break;
            }

            ExpectByte(0xD9, FillBYFF());
        }

        private byte[] ReadEightBytes()
        {
            var data = new byte[8];
            for (var i = 0; i < data.Length; i++)
                data[i] = binaryReader.ReadByte();
            return data;
        }

        private void fspec()
        {
            throw new NotImplementedException();
        }

        public void ExpectByte(byte expected, byte actual)
        {
            if (expected == actual)
                Console.WriteLine("0x{0} found", actual);
        }

        public byte FillBYFF()
        {
            byte next;
            do
                next = binaryReader.ReadByte();
            while (next == 0xFF);
            return next;
        }

        public void TblsMisc()
        {
            var data = FillBYFF();
            var b1 = binaryReader.ReadByte();
            var b2 = binaryReader.ReadByte();
            if (data == 0xD8)
                DqtData();
            if (data == 0xC4)
                DhtData();
            if (data == 0xCC)
                DacData();
            if (data == 0xDD)
            {
                var b3 = binaryReader.ReadByte();
                var b4 = binaryReader.ReadByte();
            }
            if (data >= 0xE0 && data <= 0xEF)
                comspec(b1, b2);
            if (data == 0xFE)
                appspec(b1, b2);
        }

        private void appspec(byte b1, byte b2)
        {
            throw new NotImplementedException();
        }

        private void comspec(byte b1, byte b2)
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
            var data = FillBYFF();

            // BY01, BY02 .. BYBF, BYC8, BYF0 .. BYFD
            return (data >= 0x01 && data <= 0xBF)
                || data == 0xC8
                || (data >= 0xF0 && data <= 0xFD);
        }

        public void DqtData()
        {
            // one or more of these....
            QtTbl();
            throw new NotImplementedException();
        }

        public void DhtData()
        {
            throw new NotImplementedException();
        }

        public void DacData()
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
