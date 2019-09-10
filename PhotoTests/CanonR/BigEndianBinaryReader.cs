// Log File Viewer - BigEndianBinaryReader.cs
// 
// Copyright ©  Greg Eakin.
// 
// Greg Eakin <greg@gdbtech.info>
// 
// All Rights Reserved.

using System.IO;
using System.Text;

namespace PhotoTests.CanonR
{
    class BigEndianBinaryReader : BinaryReader
    {
        public BigEndianBinaryReader(Stream input) : base(input)
        {
        }

        public override short ReadInt16()
        {
            var b = ReadBytes(2);
            return (short) (b[1] + (b[0] << 8));
        }

        public override int ReadInt32()
        {
            var b = ReadBytes(4);
            return b[3] + (b[2] << 8) + (b[1] << 16) + (b[0] << 24);
        }

        public override uint ReadUInt32()
        {
            var b = ReadBytes(4);
            return (uint) b[3] + (uint) (b[2] << 8) + (uint) (b[1] << 16) + (uint) (b[0] << 24);
        }

//        public override long ReadInt64()
//        {
//            this.FillBuffer(8);
//            return (long)(uint)((int)this.m_buffer[4] | (int)this.m_buffer[5] << 8 | (int)this.m_buffer[6] << 16 | (int)this.m_buffer[7] << 24) << 32 | (long)(uint)((int)this.m_buffer[0] | (int)this.m_buffer[1] << 8 | (int)this.m_buffer[2] << 16 | (int)this.m_buffer[3] << 24);
//        }
//
//        public override ulong ReadUInt64()
//        {
//            this.FillBuffer(8);
//            return (ulong)(uint)((int)this.m_buffer[4] | (int)this.m_buffer[5] << 8 | (int)this.m_buffer[6] << 16 | (int)this.m_buffer[7] << 24) << 32 | (ulong)(uint)((int)this.m_buffer[0] | (int)this.m_buffer[1] << 8 | (int)this.m_buffer[2] << 16 | (int)this.m_buffer[3] << 24);
//        }


        public override long ReadInt64()
        {
            var b = ReadBytes(8);
            return (long) b[7] + (b[6] << 8) + (b[5] << 16) + (b[4] << 24) + ((long) b[3] << 32) + ((long) b[2] << 40) +
                   ((long) b[1] << 48) + ((long) b[0] << 56);
        }

        public override ulong ReadUInt64()
        {
            var b = ReadBytes(8);
            return (ulong) b[7] + ((ulong) b[6] << 8) + ((ulong) b[5] << 16) + ((ulong) b[4] << 24) +
                   ((ulong) b[3] << 32) + ((ulong) b[2] << 40) + ((ulong) b[1] << 48) + ((ulong) b[0] << 56);
        }

        /// <summary>Returns <c>true</c> if the Int32 read is not zero, otherwise, <c>false</c>.</summary>
        /// <returns><c>true</c> if the Int32 is not zero, otherwise, <c>false</c>.</returns>
        public bool ReadInt32AsBool()
        {
            var b = ReadBytes(4);
            return b[0] != 0 && b[1] != 0 && b[2] != 0 && b[3] != 0;
        }

        /// <summary>
        /// Reads a string prefixed by a 32-bit integer identifying its length, in chars.
        /// </summary>
        public string ReadString32BitPrefix()
        {
            var length = ReadInt32();
            return Encoding.ASCII.GetString(ReadBytes(length));
        }

        public float ReadFloat()
        {
            return (float) ReadDouble();
        }
    }
}