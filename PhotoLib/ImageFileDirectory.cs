namespace PhotoLib
{
    using System;
    using System.IO;

    using PhotoLib.Utilities;

    public class ImageFileDirectory
    {
        #region Fields

        private readonly ImageFileEntry[] entries;

        private readonly uint nextEntry;

        #endregion

        #region Constructors and Destructors

        public ImageFileDirectory(ushort length)
        {
            this.entries = new ImageFileEntry[length];
        }

        public ImageFileDirectory(BinaryReader binaryReader, uint start)
        {
            if (binaryReader.BaseStream.Position != start)
            {
                binaryReader.BaseStream.Seek(start, SeekOrigin.Begin);
            }

            var length = binaryReader.ReadUInt16();
            this.entries = new ImageFileEntry[length];
            for (var i = 0; i < this.Length; i++)
            {
                this.entries[i] = new ImageFileEntry(binaryReader);
            }
            this.nextEntry = binaryReader.ReadUInt32();
            Console.WriteLine("### Directory [0x{0:x}], {1}, 0x{2:x}", start, length, this.nextEntry);

            var x = -1;
            foreach (var entry in this.Entries)
            {
                x++;

                if (entry.TagId == 0x8769)
                {
                    Console.WriteLine("{0}  [0x{1:x}] Image File Directory:", x, entry.ValuePointer);
                    var tags = new ImageFileDirectory(binaryReader, entry.ValuePointer);
                }
                else
                {
                    switch (entry.TagType)
                    {
                        case 0x01:
                            Console.Write("{0}  {1} Byte: ", x, entry.TagId);
                            Console.WriteLine("[0x{0:x}] ({1}): ", entry.ValuePointer, entry.NumberOfValue);
                            break;

                        case 0x02:
                            Console.Write("{0}  {1} Ascii: ", x, entry.TagId);

                            Console.Write("[0x{0:x}] ({1}): ", entry.ValuePointer, entry.NumberOfValue);
                            if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            {
                                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                            }

                            for (var j = 0; j < entry.NumberOfValue - 1; j++)
                            {
                                var us = binaryReader.ReadByte();
                                Console.Write("{0}", (char)us);
                            }
                            binaryReader.ReadByte();
                            Console.WriteLine();
                            break;

                        case 0x03:
                            Console.Write("{0}  {1} Short: ", x, entry.TagId);
                            if (entry.NumberOfValue == 1)
                            {
                                Console.Write("{0}", entry.ValuePointer);
                            }
                            else
                            {
                                Console.Write("[0x{0:x}] ({1}): ", entry.ValuePointer, entry.NumberOfValue);
                                if (binaryReader.BaseStream.Position != entry.ValuePointer)
                                {
                                    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                                }

                                for (var j = 0; j < entry.NumberOfValue; j++)
                                {
                                    var us = binaryReader.ReadUInt16();
                                    Console.Write("{0}, ", us);
                                }
                            }
                            Console.WriteLine();
                            break;

                        case 0x04:
                            Console.Write("{0}  {1} Long: ", x, entry.TagId);
                            Console.Write("[0x{0:x}] ({1}): ", entry.ValuePointer, entry.NumberOfValue);
                            //if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            //    binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);

                            //for (var j = 0; j < entry.NumberOfValue; j++)
                            //{
                            //    var us = binaryReader.ReadUInt16();
                            //    Console.Write("{0} ", us);
                            //}
                            Console.WriteLine();
                            break;

                        case 0x05:
                            Console.Write("{0}  {1} Rational: ", x, entry.TagId);
                            Console.Write("[0x{0:x}] (2):", entry.ValuePointer);
                            if (binaryReader.BaseStream.Position != entry.ValuePointer)
                            {
                                binaryReader.BaseStream.Seek(entry.ValuePointer, SeekOrigin.Begin);
                            }

                            var us1 = binaryReader.ReadUInt32();
                            var us2 = binaryReader.ReadUInt32();
                            Console.WriteLine("{0}/{1} = {2}", us1, us2, us1 / (double)us2);
                            break;

                        case 0x07:
                            Console.Write("{0}  {1} Byte[]: ", x, entry.TagId);
                            Console.WriteLine("[0x{0:x}] ({1}): ", entry.ValuePointer, entry.NumberOfValue);
                            break;

                        case 0x0A:
                            Console.Write("{0}  {1} SRational: ", x, entry.TagId);
                            Console.WriteLine("[0x{0:x}] ({1}): ", entry.ValuePointer, entry.NumberOfValue);
                            break;

                        default:
                            throw new NotImplementedException("Undfined message {0}".FormatWith(entry.TagType));
                    }
                }
            }

            // TODO move file chekcer to another class
            //Console.Write("EOB [0x{0:x}]: ", binaryReader.BaseStream.Position);
            //var readBytes = binaryReader.ReadBytes(20);
            //foreach (var t in readBytes)
            //{
            //    Console.Write("0x{0:x} ", t);
            //}
            //Console.WriteLine();
        }

        #endregion

        #region Public Properties

        public ImageFileEntry[] Entries
        {
            get
            {
                return this.entries;
            }
        }

        public ushort Length
        {
            get
            {
                return (ushort)this.Entries.Length;
            }
        }

        public uint NextEntry
        {
            get
            {
                return this.nextEntry;
            }
        }

        #endregion
    }
}